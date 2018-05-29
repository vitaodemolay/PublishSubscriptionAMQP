using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;



namespace PSF.AMQP.AzureServiceBus.Util
{
    public static class BusHelpers
    {
        /// <summary>
        /// This Method open a exist topic or create the topic when it not exist. This is used for sender.
        /// </summary>
        /// <param name="connectionString">Connection String for your Azure ServiceBus</param>
        /// <param name="topicName">Topic Name</param>
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


        /// <summary>
        /// This method open a exist topic and subscription or create it when it not exists. This is used for receiver.
        /// </summary>
        /// <param name="connectionString">Connection String for your Azure ServiceBus</param>
        /// <param name="topicName">Topic name</param>
        /// <param name="subscriptionName">Subscription name</param>
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
