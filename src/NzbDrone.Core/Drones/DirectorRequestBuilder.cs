using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Drones;

public interface IDirectorRequestBuilder
{
    HttpRequestBuilder Builder { get; }
}

public class DirectorRequestBuilder : IDirectorRequestBuilder,
    IHandle<ConfigFileSavedEvent>
{
    private readonly IConfigFileProvider _configFile;

    private IHttpRequestBuilderFactory _factory;

    public DirectorRequestBuilder(IConfigFileProvider configFile)
    {
        _configFile = configFile;
        UpdateFactory();
    }


    public HttpRequestBuilder Builder => _factory.Create();

    public void Handle(ConfigFileSavedEvent message)
    {
        UpdateFactory();
    }

    private void UpdateFactory()
    {
        if (_configFile.IsDirector)
        {
            _factory = new HttpRequestBuilder(_configFile.DirectorAddress)
                .Resource("/api/v1/drone")
                .SetHeader("X-Api-Key", _configFile.ApiKey)
                .CreateFactory();
        }
    }
}
