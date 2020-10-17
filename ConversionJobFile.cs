using System.Collections.Generic;

namespace ScoreConverter
{
    public class ConversionJobFile
    {
        public string Name { get; set; }
        public string NameWithoutExtension { get; set; }
        public string FullPath { get; set; }

        public string WorkingDir { get; set; }

        public List<ConversionJobTask> Task { get; set; }
    }
}
