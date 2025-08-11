using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wdkkr.Models
{
    public class Settings
    {
        public int IntervalMinutes { get; set; } = 30;
        public int DisplaySeconds { get; set; } = 5;
        public bool PlaySound { get; set; } = true;
    }
}
