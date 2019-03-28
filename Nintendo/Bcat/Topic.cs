using MessagePack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nintendo.Bcat
{
    [MessagePackObject]
    public class Topic
    {
        [Key("topic_id")]
        public string TopicId
        {
            get;
            set;
        }

        [Key("service_status")]
        public string ServiceStatus
        {
            get;
            set;
        }

        [Key("na_required")]
        public bool NaRequired
        {
            get;
            set;
        }

        [Key("required_application_version")]
        public int RequiredAppVersion
        {
            get;
            set;
        }

        [Key("directories")]
        public List<Directory> Directories
        {
            get;
            set;
        }

        public void IterateOverAllFiles(Action<Directory, Data> action)
        {
            // Loop over the directories
            foreach (Bcat.Directory directory in Directories)
            {
                // Loop over each file
                foreach (Bcat.Data data in directory.Data)
                {
                    // Call the action
                    action.Invoke(directory, data);
                }
            }
        }

        public async Task IterateOverAllFilesAsync(Func<Directory, Data, Task> function)
        {
            // Loop over the directories
            foreach (Bcat.Directory directory in Directories)
            {
                // Loop over each file
                foreach (Bcat.Data data in directory.Data)
                {
                    // Call the action
                    await function.Invoke(directory, data);
                }
            }
        }

    }
}
