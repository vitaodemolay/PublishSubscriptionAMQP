using Newtonsoft.Json;
using PSF.AMQP.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PSF.AMQP.RabbitMq
{

    /// <summary>
    /// The Subscribe is the Receiver object. It create a listener for when to receive a message on an open topic/queue, it start a callback method
    /// </summary>
    /// <typeparam name="IRequest">Here you set a Request interface what is on your code or in other library</typeparam>
    /// <typeparam name="INotification">Here you set a Notification interface what is on your code or in other library</typeparam>
    public class Subscribe<IRequest, INotification> : ISubscribe<IRequest, INotification>, IDisposable
    {
        private const string argType = "topic";
        private const string argTypeName = "x-delayed-type";
        private const string scheduleTypeName = "x-delayed-message";

        private IConnection connection { get; set; }
        private IList<IModel> channels { get; }
        private string Exchange { get; }
        private string SubscribeName { get; }
        private IDictionary<string, object> args { get; }
        private string[] routingKeys { get; }

        private readonly Dictionary<Type, string> messageTypeAddresses = new Dictionary<Type, string>();


        /// <summary>
        /// Constructor Method
        /// </summary>
        /// <param name="assemblyBase">type of a object from your library for to read the assembly and to find all other classes that use the Request or Notification interface informed</param>
        /// <param name="topicName">Topic Name</param>
        /// <param name="hostname">Host name of your RabbitMQ</param>
        /// <param name="port">Port of your host (is optional)</param>
        /// <param name="userName">User Name for connect in your host (is optional)</param>
        /// <param name="password">Password of User for connect in your host (is optional)</param>
        /// <param name="virtualhost">Virtual Host address (is optional)</param>
        /// <param name="subscriptionName">Subscription Name (is optional)</param>
        public Subscribe(Type assemblyBase, string topicName, string hostname,  int? port = null, string userName = null, string password = null, string virtualhost = null, string subscriptionName = null)
        {
            this.Exchange = topicName;
            this.SubscribeName = subscriptionName;
            this.args = new ExpandoObject();
            this.args.Add(argTypeName, argType);

            ConnectionFactory factory = null;
            factory = new ConnectionFactory
            {
                HostName = hostname
            };

            if (port != null)
                factory.Port = (int)port;

            if (!string.IsNullOrEmpty(userName))
                factory.UserName = userName;

            if (!string.IsNullOrEmpty(password))
                factory.Password = password;

            if (!string.IsNullOrEmpty(virtualhost))
                factory.VirtualHost = virtualhost;

            this.connection = factory.CreateConnection();

            this.channels = new List<IModel>();

            Assembly assembly = assemblyBase.Assembly;

            foreach (var requestTypeAddress in assembly.GetTypes().Where(filterType => filterType.GetInterfaces().Contains(typeof(IRequest))))
            {
                messageTypeAddresses.Add(requestTypeAddress, requestTypeAddress.Name);
            }

            foreach (var notificationTypeAddress in assembly.GetTypes().Where(filterType => filterType.GetInterfaces().Contains(typeof(INotification))))
            {
                messageTypeAddresses.Add(notificationTypeAddress, notificationTypeAddress.Name);
            }

            this.routingKeys = (from query in messageTypeAddresses select query.Value).ToArray();

        }

        private object Deserialize(string messageBody, string type)
        {
            Type typeMessage = messageTypeAddresses.Where(f => f.Value == type).SingleOrDefault().Key;
            var messageResult = JsonConvert.DeserializeObject(messageBody, typeMessage);
            return messageResult;
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            if(this.channels.Count > 0)
            {
                foreach (var channel in this.channels)
                {
                    if (channel.IsOpen)
                        channel.Close();

                    channel.Dispose();
                }
            }
            if (this.connection != null)
            {
                if (this.connection.IsOpen)
                    this.connection.Close();

                this.connection.Dispose();
            }
        }

        /// <summary>
        /// This method create a listener and define a callback method for when to receive a message
        /// </summary>
        /// <param name="callback">Callback method delegate</param>
        public void OnMessage(Action<object> callback)
        {
            var channel = this.connection.CreateModel();
            channel.ExchangeDeclare(exchange: this.Exchange, type: scheduleTypeName, arguments: this.args);

            var queueName = channel.QueueDeclare(queue: this.SubscribeName, autoDelete:true).QueueName;

            foreach (var bindingKey in this.routingKeys)
            {
                channel.QueueBind(queue: queueName,
                                  exchange: this.Exchange,
                                  routingKey: bindingKey);
            }

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var type = ea.BasicProperties.Type;
                var body = Encoding.UTF8.GetString(ea.Body);

                var message = Deserialize(body, type);
                callback.Invoke(message);
            };
            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            this.channels.Add(channel);
        }
    }
}
