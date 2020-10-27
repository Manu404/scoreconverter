using System.Collections.Generic;

namespace ScoreConverter
{
    public class ConversionJobFile
    {
        public string Name { get; set; }
        public string NameWithoutExtension { get; set; }
        public string FullPath { get; set; }
        public string WorkingDir { get; set; }
        public List<ConversionJobTask> Tasks { get; set; }
        public List<ConversionPartJobTask> Parts {get; set; }

        public ConversionJobFile()
        {
            Tasks = new List<ConversionJobTask>();
            Parts = new List<ConversionPartJobTask>();
        }
    }
}
