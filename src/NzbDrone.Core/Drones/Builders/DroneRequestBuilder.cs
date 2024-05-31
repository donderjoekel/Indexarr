using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Drones.Builders;

public interface IDroneRequestBuilder
{
    HttpRequestBuilder Create(Drone drone);
}

public class DroneRequestBuilder : IDroneRequestBuilder
{
    private readonly IConfigFileProvider _configFile;

    public DroneRequestBuilder(IConfigFileProvider configFile)
    {
        _configFile = configFile;
    }

    public HttpRequestBuilder Create(Drone drone)
    {
        return new HttpRequestBuilder(drone.Address)
            .Resource("/api/v1/drone")
            .SetHeader("X-Api-Key", _configFile.ApiKey)
            .CreateFactory()
            .Create();
    }
}
