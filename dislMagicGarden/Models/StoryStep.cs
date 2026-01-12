using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dislMagicGarden.Models
{
    public class StoryStep
    {
        public string StoryText { get; set; }
        public List<string> Options { get; set; }
        public string PlotTwistFactor { get; set; } // Bonus: Wie verrückt ist es diesmal?
    }
}
