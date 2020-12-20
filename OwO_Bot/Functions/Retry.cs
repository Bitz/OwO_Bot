using System;
using System.Collections.Generic;
using System.Threading;
using Org.BouncyCastle.Security;

namespace OwO_Bot.Functions
{
    public static class Retry
    {
        public static void Do(
            Action action,
            TimeSpan? retryInterval = null,
            int maxAttemptCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, maxAttemptCount);
        }

        public static T Do<T>(
            Func<T> action,
            TimeSpan? retryInterval = null,
            int maxAttemptCount = 3)
        {
            if (!retryInterval.HasValue)
            {
                retryInterval = TimeSpan.FromSeconds(5);
            }
            else
            {
                throw new InvalidParameterException(nameof(retryInterval));
            }

            var exceptions = new List<Exception>();

            for (int attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0)
                    {
                        Thread.Sleep(retryInterval.Value);
                    }
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException(exceptions);
        }
    }
}
