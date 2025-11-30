using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dislMagicGarden.Models
{
    public class StorySettings
    {
        public string ChildName { get; set; }
        public string SidekickAnimal { get; set; }
        public string WorldSetting { get; set; }   // Wald, Weltraum, Meer …
        public string Mood { get; set; }          // beruhigend, lustig, abenteuer
        public int ChapterCount { get; set; } = 4;
        public string IllustrationStyle { get; set; } // cartoon, watercolor, etc.
    }

}
