using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRL
{
    public class ChannelConfig
    {
        public List<double> Data { get; set; }
        public double PresetUpperLimit { get; set; }
        public double PresetLowerLimit { get; set; }
        public double Mean { get; set; }
        public double MKT_C { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public double WithinLimits { get; set; }
        public double OutsideLimits { get; set; }
        public double AboveLimits { get; set; }
        public double BelowLimits { get; set; }
        public string BreachedAbove { get; set; }
        public string BreachedBelow { get; set; }
        public string TimeWithinLimits { get; set; }
        public string TimeOutLimits { get; set; }
        public string TimeAboveLimits { get; set; }
        public string TimeBelowLimits { get; set; }
        public string SensorName { get; set; }
        public string Unit { get; set; }
    }
}
