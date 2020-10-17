using Newtonsoft.Json;

namespace ScoreConverter
{
    public class ConversionJobTask
    {
        [JsonProperty("in")]
        public string In { get; set; }
        [JsonProperty("out")]
        public string Out { get; set; }
    }
}
