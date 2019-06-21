using System;

namespace BcatBotFramework.Core.Config.NintendoCdn
{
    public class SdkVersion
    {
        public int Major
        {
            get;
            set;
        }

        public int Minor
        {
            get;
            set;
        }

        public int Revision
        {
            get;
            set;
        }

        public int Build
        {
            get;
            set;
        }

        public SdkVersion(int major, int minor, int revision, int build)
        {
            this.Major = major;
            this.Minor = minor;
            this.Revision = revision;
            this.Build = build;
        }

    }
}