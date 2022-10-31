using System;

namespace FallGuysRecord_WPF_Framework
{
    public class LevelMap
    {
        public String name { get; set; }
        public String showname { get; set; }
        public String type { get; set; }

        public LevelMap()
        {
            this.name = "未知";
            this.showname = "未知";
            this.type = "未知";
        }
    }
}
