using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;



namespace PSF.AMQP.AzureServiceBus.Util
{
    public static class BusHelpers
    {
        public static void InitializeTopic(string connectionString, string topicName)
        {
            
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.TopicExists(topicName))
            {
                var TopicDescription = new TopicDescription(topicName)
                {
                    EnableBatchedOperations = true,
                    DefaultMessageTimeToLive = new TimeSpan(14, 0, 0, 0)
                };
                namespaceManager.CreateTopic(TopicDescription);
            }
        }

        public static void InitializeSubscription(string connectionString, string topicName, string subscriptionName)
        {
            InitializeTopic(connectionString, topicName);
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.SubscriptionExists(topicName, subscriptionName))
            {
                var subscriptionDescription = new SubscriptionDescription(topicName, subscriptionName);
                namespaceManager.CreateSubscription(subscriptionDescription);
            }

        }
    }
}
