using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NzbDrone.Core.Messaging.Commands
{
    public class CommandQueue : IEnumerable
    {
        private readonly object _mutex = new object();
        private readonly List<CommandModel> _items;

        public CommandQueue()
        {
            _items = new List<CommandModel>();
        }

        public int Count => _items.Count;

        public void Add(CommandModel item)
        {
            lock (_mutex)
            {
                _items.Add(item);

                Monitor.PulseAll(_mutex);
            }
        }

        public IEnumerator<CommandModel> GetEnumerator()
        {
            List<CommandModel> copy = null;

            lock (_mutex)
            {
                copy = new List<CommandModel>(_items);
            }

            return copy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public List<CommandModel> All()
        {
            List<CommandModel> rval = null;

            lock (_mutex)
            {
                rval = _items;
            }

            return rval;
        }

        public CommandModel Find(Guid id)
        {
            return All().FirstOrDefault(q => q.Id == id);
        }

        public void RemoveMany(IEnumerable<CommandModel> commands)
        {
            lock (_mutex)
            {
                foreach (var command in commands)
                {
                    _items.Remove(command);
                }

                Monitor.PulseAll(_mutex);
            }
        }

        public bool RemoveIfQueued(Guid id)
        {
            var rval = false;

            lock (_mutex)
            {
                var command = _items.FirstOrDefault(q => q.Id == id);

                if (command?.Status == CommandStatus.Queued)
                {
                    _items.Remove(command);
                    rval = true;

                    Monitor.PulseAll(_mutex);
                }
            }

            return rval;
        }

        public List<CommandModel> QueuedOrStarted()
        {
            return All().Where(q => q.Status == CommandStatus.Queued || q.Status == CommandStatus.Started)
                        .ToList();
        }

        public IEnumerable<CommandModel> GetConsumingEnumerable()
        {
            return GetConsumingEnumerable(CancellationToken.None);
        }

        public IEnumerable<CommandModel> GetConsumingEnumerable(CancellationToken cancellationToken)
        {
            cancellationToken.Register(PulseAllConsumers);

            while (!cancellationToken.IsCancellationRequested)
            {
                CommandModel command = null;

                lock (_mutex)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!TryGet(out command))
                    {
                        Monitor.Wait(_mutex);
                        continue;
                    }
                }

                if (command != null)
                {
                    yield return command;
                }
            }
        }

        public void PulseAllConsumers()
        {
            // Signal all consumers to reevaluate cancellation token
            lock (_mutex)
            {
                Monitor.PulseAll(_mutex);
            }
        }

        public bool TryGet(out CommandModel item)
        {
            var rval = true;
            item = default(CommandModel);

            lock (_mutex)
            {
                if (_items.Count == 0)
                {
                    rval = false;
                }
                else
                {
                    var startedCommands = _items.Where(c => c.Status == CommandStatus.Started)
                                                .ToList();

                    var exclusiveTypes = startedCommands.Where(x => x.Body.IsTypeExclusive)
                        .Select(x => x.Body.Name)
                        .ToList();

                    var queuedCommands = _items.Where(c => c.Status == CommandStatus.Queued);

                    if (startedCommands.Any(x => x.Body.RequiresDiskAccess))
                    {
                        queuedCommands = queuedCommands.Where(c => !c.Body.RequiresDiskAccess);
                    }

                    if (startedCommands.Any(x => x.Body.IsTypeExclusive))
                    {
                        queuedCommands = queuedCommands.Where(c => !exclusiveTypes.Any(x => x == c.Body.Name));
                    }

                    var localItem = queuedCommands.OrderByDescending(c => c.Priority)
                        .ThenBy(c => c.QueuedAt)
                        .FirstOrDefault();

                    // Nothing queued that meets the requirements
                    if (localItem == null)
                    {
                        rval = false;
                    }

                    // If any executing command is exclusive don't want return another command until it completes.
                    else if (startedCommands.Any(c => c.Body.IsExclusive))
                    {
                        rval = false;
                    }

                    // If the next command to execute is exclusive wait for executing commands to complete.
                    // This will prevent other tasks from starting so the exclusive task executes in the order it should.
                    else if (localItem.Body.IsExclusive && startedCommands.Any())
                    {
                        rval = false;
                    }

                    // A command ready to execute
                    else
                    {
                        localItem.StartedAt = DateTime.UtcNow;
                        localItem.Status = CommandStatus.Started;

                        item = localItem;
                    }
                }
            }

            return rval;
        }
    }
}
