using Newtonsoft.Json;

namespace ScoreConverter
{
    public class ConversionPartJobTask
    {
        [JsonProperty("in")]
        public string In { get; set; }

        [JsonProperty("out")]
        public string[][] Out { get; set; }
    }

    public class ConversionJobTask
    {
        [JsonProperty("in")]
        public string In { get; set; }

        [JsonProperty("out")]
        public string[] Out { get; set; }
    }
}
