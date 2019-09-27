using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nintendo;

namespace NinStatusBot.Json
{
    public class Downtime
    {
        public uint Id
        {
            get;
            set;
        }
        
        public List<NintendoPlatform> Platforms
        {
            get;
            set;
        }

        public string AffectedTitle
        {
            get;
            set;
        }

        public List<string> AffectedServices
        {
            get;
            set;
        }

        public DowntimeType Type
        {
            get;
            set;
        }

        public DateTime? StartTime
        {
            get;
            set;
        }

        public DateTime? ExpectedEndTime
        {
            get;
            set;
        }
        
    }
}