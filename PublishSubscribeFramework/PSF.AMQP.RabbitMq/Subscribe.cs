using Newtonsoft.Json;
using PSF.Interfaces;
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
