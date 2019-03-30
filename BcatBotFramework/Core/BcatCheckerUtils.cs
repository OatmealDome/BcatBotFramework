using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BcatBotFramework.Difference;
using BcatBotFramework.Social.Discord;
using Nintendo.Bcat;

namespace BcatBotFramework.Core
{
    public static class BcatCheckerUtils
    {
        public static async Task<Dictionary<string, byte[]>> DownloadAllData(Topic topic, string titleId, string passphrase, string targetFolder)
        {
            // Create the target folder
            System.IO.Directory.CreateDirectory(targetFolder);

            // Create a Dictionary
            Dictionary<string, byte[]> dataDict = new Dictionary<string, byte[]>();

            // Loop over every directory
            foreach (Nintendo.Bcat.Directory directory in topic.Directories)
            {
                // Create the local directory path
                string localDirectory = Path.Combine(targetFolder, directory.Name);

                // Create the folder
                System.IO.Directory.CreateDirectory(localDirectory);

                // Loop over every data
                foreach (Nintendo.Bcat.Data data in directory.Data)
                {
                    // Download the file
                    byte[] rawData = await BcatApi.DownloadContainerAndDecrypt(data.Url, titleId, passphrase);

                    // Add this to the Dictionary
                    dataDict.Add(directory.Name + "/" + data.Name, rawData);

                    // Construct the path
                    string path = Path.Combine(targetFolder, directory.Name, data.Name);

                    // Write out the file
                    File.WriteAllBytes(path, rawData);
                }
            }

            return dataDict;
        }

        public static List<KeyValuePair<DifferenceType, string>> GetTopicChanges(Topic oldTopic, Topic newTopic)
        {
            Dictionary<string, string> GenerateDigestDictionary(Topic topic)
            {
                Dictionary<string, string> digestDictionary = new Dictionary<string, string>();

                // Loop over every directory
                foreach (Nintendo.Bcat.Directory directory in topic.Directories)
                {
                    // Loop over every data
                    foreach (Nintendo.Bcat.Data data in directory.Data)
                    {
                        // Add this to the List
                        digestDictionary.Add(directory.Name + "/" + data.Name, data.Digest);
                    }
                }

                return digestDictionary;
            }

            // Create a difference Dictionary
            List<KeyValuePair<DifferenceType, string>> differences = new List<KeyValuePair<DifferenceType, string>>();

            // Get the file Lists
            Dictionary<string, string> oldFileDigests = GenerateDigestDictionary(oldTopic);
            Dictionary<string, string> newFileDigests = GenerateDigestDictionary(newTopic);

            // Get added and removed files
            IEnumerable<string> addedList = newFileDigests.Keys.Except(oldFileDigests.Keys);
            IEnumerable<string> removedList = oldFileDigests.Keys.Except(newFileDigests.Keys);
            IEnumerable<string> sameList = newFileDigests.Keys.Intersect(oldFileDigests.Keys);

            // Add each added file
            foreach (string path in addedList)
            {
                differences.Add(new KeyValuePair<DifferenceType, string>(DifferenceType.Added, path));
            }

            // Add each removed file
            foreach (string path in removedList)
            {
                differences.Add(new KeyValuePair<DifferenceType, string>(DifferenceType.Removed, path));
            }

            // Compare digests of files that are still here
            foreach (string path in sameList)
            {
                // Compare the old digest with the new one
                if (oldFileDigests[path] != newFileDigests[path])
                {
                    // Add this to the Dictionary
                    differences.Add(new KeyValuePair<DifferenceType, string>(DifferenceType.Changed, path));
                }
            }

            // Return the differences
            return differences;
        }

        public static async Task CallDifferenceHandlers(int fileType, DifferenceType differenceType, object[] parameters)
        {
            // Get the handlers
            SortedList<int, MethodInfo> methodInfos = HandlerMapper.GetHandlers((int)fileType, differenceType);

            // Loop over every handler
            foreach (MethodInfo methodInfo in methodInfos.Values)
            {
                await DiscordBot.LoggingChannel.SendMessageAsync("**[BCAT]** Calling " + methodInfo.DeclaringType.Name + "." + methodInfo.Name + "()");
                
                try
                {
                    // Invoke the method
                    object returnValue = methodInfo.Invoke(null, parameters);

                    // Check the return value
                    if (returnValue != null && returnValue is Task)
                    {
                        await (Task)returnValue;
                    }
                }
                catch (Exception exception)
                {
                    // Notify the logging channel
                    await DiscordUtil.HandleException((exception is TargetInvocationException) ? ((TargetInvocationException)exception).InnerException : exception, $"in ``{methodInfo.DeclaringType.Name}.{methodInfo.Name}()``");
                }
            }
        }
        
    }
}