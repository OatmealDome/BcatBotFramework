using System;
using System.Collections.Generic;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Core.Config.Twitter;
using Tweetinvi;

namespace BcatBotFramework.Social.Twitter
{
    public class TwitterManager
    {
        private static Dictionary<string, TwitterAccount> Accounts;

        public static void Initialize()
        {
            // Don't swallow Twitter exceptions
            ExceptionHandler.SwallowWebExceptions = false;
            
            // Create a new Dictionary
            Accounts = new Dictionary<string, TwitterAccount>();

            // Loop over every pair
            foreach (KeyValuePair<string, CachedTwitterCredentials> pair in Configuration.LoadedConfiguration.TwitterConfig.TwitterCredentials)
            {
                // Initialize a new TwitterAccount
                Accounts[pair.Key] = new TwitterAccount(pair.Value);
            }
        }

        public static void Dispose()
        {

        }

        public static TwitterAccount GetAccount(string accountName)
        {
            // Try to get the account
            if (Accounts.TryGetValue(accountName, out TwitterAccount account))
            {
                return account;
            }

            throw new Exception("No Twitter account exists with this name");
        }

    }
}