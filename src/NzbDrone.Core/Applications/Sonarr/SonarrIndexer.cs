using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NzbDrone.Core.Applications.Sonarr
{
    public class SonarrIndexer
    {
        public int Id { get; set; }
        public bool EnableRss { get; set; }
        public bool EnableAutomaticSearch { get; set; }
        public bool EnableInteractiveSearch { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
        public string ImplementationName { get; set; }
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public string InfoLink { get; set; }
        public HashSet<int> Tags { get; set; }
        public List<SonarrField> Fields { get; set; }

        public bool Equals(SonarrIndexer other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            var baseUrl = (string)Fields.FirstOrDefault(x => x.Name == "baseUrl").Value == (string)other.Fields.FirstOrDefault(x => x.Name == "baseUrl").Value;
            var apiPath = (string)Fields.FirstOrDefault(x => x.Name == "apiPath").Value == (string)other.Fields.FirstOrDefault(x => x.Name == "apiPath").Value;
            var apiKey = (string)Fields.FirstOrDefault(x => x.Name == "apiKey").Value == (string)other.Fields.FirstOrDefault(x => x.Name == "apiKey").Value;
            var cats = JToken.DeepEquals((JArray)Fields.FirstOrDefault(x => x.Name == "categories").Value, (JArray)other.Fields.FirstOrDefault(x => x.Name == "categories").Value);
            var animeCats = JToken.DeepEquals((JArray)Fields.FirstOrDefault(x => x.Name == "animeCategories").Value, (JArray)other.Fields.FirstOrDefault(x => x.Name == "animeCategories").Value);

            return other.EnableRss == EnableRss &&
                other.EnableAutomaticSearch == EnableAutomaticSearch &&
                other.EnableInteractiveSearch == EnableAutomaticSearch &&
                other.Name == Name &&
                other.Implementation == Implementation &&
                other.Priority == Priority &&
                other.Id == Id &&
                apiKey && apiPath && baseUrl && cats && animeCats;
        }
    }
}
