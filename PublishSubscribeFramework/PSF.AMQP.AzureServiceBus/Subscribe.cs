using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using PSF.AMQP.Interfaces;

namespace PSF.AMQP.AzureServiceBus
{
    /// <summary>
    /// The Subscribe is the Receiver object. It create a listener for when to receive a message on an open topic/queue, it start a callback method
    /// </summary>
    /// <typeparam name="IRequest">Here you set a Request interface what is on your code or in other library</typeparam>
    /// <typeparam name="INotification">Here you set a Notification interface what is on your code or in other library</typeparam>
    public class Subscribe<IRequest, INotification> : ISubscribe<IRequest, INotification>
    {

        private readonly Dictionary<Type, string> messageTypeAddresses = new Dictionary<Type, string>();
        private readonly SubscriptionClient subscriptionClient;
        private readonly string connectionString;
        private readonly string topicName;
        private readonly string subbscriptionName;


        /// <summary>
        /// Constructor Methdo
        /// </summary>
        /// <param name="assemblyBase">type of a object from your library for to read the assembly and to find all other classes that use the Request or Notification interface informed</param>
        /// <param name="connectionString">Connection string of your Azure ServiceBus</param>
        /// <param name="topicName">Topic Name</param>
        /// <param name="subscriptionName">Subscription Name</param>
        public Subscribe(Type assemblyBase, String connectionString, String topicName, String subscriptionName)
        {

            this.connectionString = connectionString;
            this.topicName = topicName;
            this.subbscriptionName = subscriptionName;

            Util.BusHelpers.InitializeSubscription(connectionString, topicName, subscriptionName);
            this.subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName);

            Assembly assembly = assemblyBase.Assembly;

            foreach (var requestTypeAddress in assembly.GetTypes().Where(filterType => filterType.GetInterfaces().Contains(typeof(IRequest))))
            {
                messageTypeAddresses.Add(requestTypeAddress, requestTypeAddress.Name);
            }

            foreach (var notificationTypeAddress in assembly.GetTypes().Where(filterType => filterType.GetInterfaces().Contains(typeof(INotification))))
            {
                messageTypeAddresses.Add(notificationTypeAddress, notificationTypeAddress.Name);
            }

            string[] rules = (from query in messageTypeAddresses select query.Value).ToArray();

            AddRule(rules);

        }

        /// <summary>
        /// This method create a listener and define a callback method for when to receive a message
        /// </summary>
        /// <param name="callback">Callback method delegate</param>
        public void OnMessage(Action<dynamic> callback)
        {
            this.subscriptionClient.OnMessage(message => callback.Invoke(Deserialize(message)));
        }

        private dynamic Deserialize(BrokeredMessage message)
        {
            string messageBody = Encoding.UTF8.GetString(message.GetBody<byte[]>());
            Type typeMessage = messageTypeAddresses.Where(f => f.Value == message.Label).SingleOrDefault().Key;
            var messageResult = JsonConvert.DeserializeObject(messageBody, typeMessage);
            return messageResult;
        }

        private void AddRule(string[] rules)
        {
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(this.connectionString);
            var namespaceRules = namespaceManager.GetRules(this.topicName, this.subbscriptionName);

            foreach (var rule in namespaceRules)
            {
                subscriptionClient.RemoveRule(rule.Name);
            }

            if (namespaceRules.Where(f => f.Name == "$Default").SingleOrDefault() != null)
            {
                subscriptionClient.RemoveRule("$Default");
            }

            foreach (string rule in rules)
            {
                subscriptionClient.AddRule(new RuleDescription()
                {
                    Filter = new CorrelationFilter { Label = rule },
                    Name = rule
                });
            }

        }

    }
}
