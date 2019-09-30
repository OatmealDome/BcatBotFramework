using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using BcatBotFramework.Core.Config;
using BcatBotFramework.Core.Config.Scheduler;

namespace BcatBotFramework.Scheduler
{
    public class QuartzScheduler
    {
        private static IScheduler Scheduler;

        public static async Task Initialize()
        {
            // Grab the Scheduler instance from the Factory
            StdSchedulerFactory factory = new StdSchedulerFactory();
            Scheduler = await factory.GetScheduler();

            // Start the Scheduler
            await Scheduler.Start();
        }

        public static async Task Dispose()
        {
            await Scheduler.Shutdown(false);
        }

        public static async Task ScheduleJob<T>(string jobName, JobSchedule jsonJobDetail)
        {
            await ScheduleJob(typeof(T), jobName, jsonJobDetail.TriggerMinute, jsonJobDetail.Interval);
        }

        public static async Task ScheduleJob<T>(string jobName, int triggerMinute, int interval)
        {
            await ScheduleJob(typeof(T), jobName, triggerMinute, interval);
        }

        public static async Task ScheduleJob<T>(string jobName, int secondsInterval)
        {
            // Get the Type
            Type type = typeof(T);

            // Create the job
            IJobDetail jobDetail = JobBuilder.Create(type)
                .WithIdentity(type.Name + jobName)
                .Build();

            // Set up the Trigger
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(type.Name + jobName + "Trigger")
                .StartNow()
                .WithSimpleSchedule(builder => builder
                    .WithIntervalInSeconds(secondsInterval)
                    .RepeatForever())
                .Build();

            // Schedule the Job
            await Scheduler.ScheduleJob(jobDetail, trigger);
        }

        public static async Task ScheduleJob<T>(string jobName, DateTime dateTime, JobDataMap dataMap)
        {
            // Get the Type
            Type type = typeof(T);

            // Create the job
            IJobDetail jobDetail = JobBuilder.Create(type)
                .UsingJobData(dataMap)
                .Build();

            // Set up the Trigger
            ITrigger trigger = TriggerBuilder.Create()
                .StartAt(new DateTimeOffset(dateTime))
                .Build();

            // Schedule the Job
            await Scheduler.ScheduleJob(jobDetail, trigger);
        }

        public static async Task ScheduleJob<T>(string jobName)
        {
            await ScheduleJob(typeof(T), jobName);
        }

        public static async Task ScheduleJob(Type type, string jobName)
        {
            // Create the job
            IJobDetail jobDetail = JobBuilder.Create(type)
                .WithIdentity(type.Name + jobName)
                .Build();

            // Set up the Trigger
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(type.Name + jobName)
                .StartNow()
                .Build();

            // Schedule the Job
            await Scheduler.ScheduleJob(jobDetail, trigger);
        }

        public static async Task ScheduleJob(Type type, string jobName, JobSchedule schedule)
        {
            await ScheduleJob(type, jobName, schedule.TriggerMinute, schedule.Interval);
        }

        public static async Task ScheduleJob(Type type, string jobName, int triggerMinute, int interval)
        {
            // Create the job
            IJobDetail jobDetail = JobBuilder.Create(type)
                .WithIdentity(type.Name + jobName)
                .Build();

            // Set up the Trigger
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(type.Name + jobName + "Trigger")
                .StartAt(GetNextTrigger(triggerMinute))
                .WithSimpleSchedule(builder => builder
                    .WithIntervalInMinutes(interval)
                    .RepeatForever())
                .Build();

            // Schedule the Job
            await Scheduler.ScheduleJob(jobDetail, trigger);
        }

        private static DateTimeOffset GetNextTrigger(int triggerMinute)
        {
            // Get the current time
            DateTimeOffset comparisonOffset = DateTimeOffset.Now;

            // Check if the trigger minute has passed
            int hour = comparisonOffset.Hour;
            if (comparisonOffset.Minute >= triggerMinute)
            {
                // Increment the hour
                hour += 1;
            }

            // Return the trigger offset
            return new DateTimeOffset(comparisonOffset.Year, comparisonOffset.Month, comparisonOffset.Day,
                hour, triggerMinute, 0, comparisonOffset.Offset);
        }

    }
}