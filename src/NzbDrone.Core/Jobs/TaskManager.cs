using System;
using System.Collections.Generic;
using System.Linq;
using Indexarr.Core.Purging.Commands;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Applications;
using NzbDrone.Core.Backup;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Drones;
using NzbDrone.Core.Drones.Commands;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.History;
using NzbDrone.Core.Housekeeping;
using NzbDrone.Core.IndexerVersions;
using NzbDrone.Core.Indexing.Commands;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Matching.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Metadata.Commands;
using NzbDrone.Core.Update.Commands;

namespace NzbDrone.Core.Jobs
{
    public interface ITaskManager
    {
        IList<ScheduledTask> GetPending();
        List<ScheduledTask> GetAll();
        DateTime GetNextExecution(Type type);
    }

    public class TaskManager : ITaskManager, IHandle<ApplicationStartedEvent>, IHandle<CommandExecutedEvent>, IHandleAsync<ConfigSavedEvent>
    {
        private readonly IScheduledTaskRepository _scheduledTaskRepository;
        private readonly IConfigService _configService;
        private readonly Logger _logger;
        private readonly ICached<ScheduledTask> _cache;

        public TaskManager(IScheduledTaskRepository scheduledTaskRepository, IConfigService configService, ICacheManager cacheManager, Logger logger)
        {
            _scheduledTaskRepository = scheduledTaskRepository;
            _configService = configService;
            _cache = cacheManager.GetCache<ScheduledTask>(GetType());
            _logger = logger;
        }

        public IList<ScheduledTask> GetPending()
        {
            return _cache.Values
                         .Where(c => c.Interval > 0 && c.LastExecution.AddMinutes(c.Interval) < DateTime.UtcNow)
                         .ToList();
        }

        public List<ScheduledTask> GetAll()
        {
            return _cache.Values.ToList();
        }

        public DateTime GetNextExecution(Type type)
        {
            var scheduledTask = _cache.Find(type.FullName);

            return scheduledTask.LastExecution.AddMinutes(scheduledTask.Interval);
        }

        public void Handle(ApplicationStartedEvent message)
        {
            var defaultTasks = new List<ScheduledTask>
                {
                    new ScheduledTask
                    {
                        Interval = 5,
                        TypeName = typeof(MessagingCleanupCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 6 * 60,
                        TypeName = typeof(ApplicationCheckUpdateCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 6 * 60,
                        TypeName = typeof(CheckHealthCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 24 * 60,
                        TypeName = typeof(HousekeepingCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 24 * 60,
                        TypeName = typeof(CleanUpHistoryCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 24 * 60,
                        TypeName = typeof(IndexerDefinitionUpdateCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 6 * 60,
                        TypeName = typeof(ApplicationIndexerSyncCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = GetBackupInterval(),
                        TypeName = typeof(BackupCommand).FullName
                    },

                    new ScheduledTask
                    {
                        Interval = 24 * 7 * 60,
                        TypeName = typeof(FullIndexCommand).FullName
                    },

                    new ScheduledTask()
                    {
                        Interval = int.MaxValue,
                        TypeName = typeof(MatchMangasCommand).FullName
                    },

                    new ScheduledTask()
                    {
                        Interval = int.MaxValue,
                        TypeName = typeof(MetadataRefreshCommand).FullName
                    },

                    new ScheduledTask()
                    {
                        Interval = int.MaxValue,
                        TypeName = typeof(PurgeCommand).FullName
                    },

                    new ScheduledTask()
                    {
                        Interval = DroneService.RefreshInterval,
                        TypeName = typeof(RegisterDroneCommand).FullName
                    },

                    new ScheduledTask()
                    {
                        Interval = DroneService.RefreshInterval * 2,
                        TypeName = typeof(RemoveUnresponsiveDronesCommand).FullName
                    }
                };

            var currentTasks = _scheduledTaskRepository.All().ToList();

            _logger.Trace("Initializing jobs. Available: {0} Existing: {1}", defaultTasks.Count, currentTasks.Count);

            foreach (var job in currentTasks)
            {
                if (!defaultTasks.Any(c => c.TypeName == job.TypeName))
                {
                    _logger.Trace("Removing job from database '{0}'", job.TypeName);
                    _scheduledTaskRepository.Delete(job.Id);
                }
            }

            foreach (var defaultTask in defaultTasks)
            {
                var currentDefinition = currentTasks.SingleOrDefault(c => c.TypeName == defaultTask.TypeName) ?? defaultTask;

                currentDefinition.Interval = defaultTask.Interval;

                if (currentDefinition.Id == Guid.Empty)
                {
                    currentDefinition.LastExecution = DateTime.UtcNow;
                }

                currentDefinition.Priority = defaultTask.Priority;

                _cache.Set(currentDefinition.TypeName, currentDefinition);
                _scheduledTaskRepository.Upsert(currentDefinition);
            }
        }

        private int GetBackupInterval()
        {
            var intervalMinutes = _configService.BackupInterval;

            if (intervalMinutes < 1)
            {
                intervalMinutes = 1;
            }

            if (intervalMinutes > 7)
            {
                intervalMinutes = 7;
            }

            return intervalMinutes * 60 * 24;
        }

        public void Handle(CommandExecutedEvent message)
        {
            var scheduledTask = _scheduledTaskRepository.All().SingleOrDefault(c => c.TypeName == message.Command.Body.GetType().FullName);

            if (scheduledTask != null && message.Command.Body.UpdateScheduledTask)
            {
                _logger.Trace("Updating last run time for: {0}", scheduledTask.TypeName);

                var lastExecution = DateTime.UtcNow;
                var startTime = message.Command.StartedAt.Value;

                _scheduledTaskRepository.SetLastExecutionTime(scheduledTask.Id, lastExecution, startTime);

                var cached = _cache.Find(scheduledTask.TypeName);

                cached.LastExecution = lastExecution;
                cached.LastStartTime = startTime;
            }
        }

        public void HandleAsync(ConfigSavedEvent message)
        {
            var backup = _scheduledTaskRepository.GetDefinition(typeof(BackupCommand));
            backup.Interval = GetBackupInterval();

            _scheduledTaskRepository.UpdateMany(new List<ScheduledTask> { backup });

            _cache.Find(backup.TypeName).Interval = backup.Interval;
        }
    }
}
