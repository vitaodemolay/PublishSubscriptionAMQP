using Newtonsoft.Json;
using PSF.AMQP.Interfaces;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;

namespace PSF.AMQP.RabbitMq
{
    /// <summary>
    /// The Publish is the Sender Object.  It have two methods for send the message for topic/queue on RabbitMQ Host 
    /// </summary>
    public class Publish : IPublish
    {
        private const string argType = "topic";
        private const string argTypeName = "x-delayed-type";
        private const string scheduleTypeName = "x-delayed-message";
        private const string headerMsgDelay = "x-delay";

        internal class BrokeredMessage
        {
            public string routingKey { get; set; }
            public byte[] body { get; set; }
            public IBasicProperties properties { get; set; }
        }

        private IConnection connection { get; set; }
        private string Exchange { get; }
        private IDictionary<string, object> args { get; }

        /// <summary>
        /// Constructor Method
        /// </summary>
        /// <param name="topicName">Topic Name</param>
        /// <param name="hostname">Host name of your RabbitMQ</param>
        /// <param name="port">Port of your host (is optional)</param>
        /// <param name="userName">User Name for connect in your host (is optional)</param>
        /// <param name="password">Password of User for connect in your host (is optional)</param>
        /// <param name="virtualhost">Virtual Host address (is optional)</param>
        public Publish(string topicName, string hostname, int? port = null, string userName = null, string password = null, string virtualhost = null)
        {
            this.Exchange = topicName;
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
        }

        /// <summary>
        /// Dispose Method
        /// </summary>
        public void Dispose()
        {
            if (this.connection != null)
            {
                if (this.connection.IsOpen)
                    this.connection.Close();

                this.connection.Dispose();
            }
        }

        /// <summary>
        /// The send method serialize the message object, mark this with configurations of expire and schedule delivery and send this for topic/queue;
        /// </summary>
        /// <typeparam name="T">Message Type</typeparam>
        /// <param name="message">Message object for to send</param>
        /// <param name="expireMessage">Expiration date</param>
        /// <param name="scheduleDelivery">Schedule Delivery date (delivery with delay)</param>
        public void Send<T>(T message, DateTime? expireMessage = null, DateTime? scheduleDelivery = null)
        {
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: this.Exchange, type: scheduleTypeName, arguments: this.args);
                BrokeredMessage brokedmsg = MessagesMaker(message, channel.CreateBasicProperties(), expireMessage, scheduleDelivery);
                channel.BasicPublish(exchange: this.Exchange,
                                     routingKey: brokedmsg.routingKey,
                                     basicProperties: brokedmsg.properties,
                                     body: brokedmsg.body);
            }
        }


        /// <summary>
        /// The send async method serialize the message object, mark this with configurations of expire and schedule delivery and send this for topic/queue;
        /// </summary>
        /// <typeparam name="T">Message Type</typeparam>
        /// <param name="message">Message object for to send</param>
        /// <param name="expireMessage">Expiration date</param>
        /// <param name="scheduleDelivery">Schedule Delivery date (delivery with delay)</param>
        public Task SendAsync<T>(T message, DateTime? expireMessage = null, DateTime? scheduleDelivery = null)
        {
            var t = new Task(() => this.Send(message, expireMessage, scheduleDelivery));
            t.Start();
            return t;
        }



        private BrokeredMessage MessagesMaker<T>(T message, IBasicProperties props, DateTime? expired = null, DateTime? schedule = null)
        {
            var name = typeof(T).Name;
            string jsonMessage = JsonConvert.SerializeObject(message);

            var _message = new BrokeredMessage
            {
                properties = props,
                routingKey = name,
                body = Encoding.UTF8.GetBytes(jsonMessage)
            };

            double delay = 0;


            if (schedule != null)
                delay = (((DateTime)schedule).ToUniversalTime().Subtract(DateTime.UtcNow)).TotalMilliseconds;

            _message.properties.Headers = new ExpandoObject();
            _message.properties.Headers.Add(headerMsgDelay, ((Int64)delay).ToString("d"));
            _message.properties.Type = name;

            if (expired != null)
            {
                double expiration = (((DateTime)expired).ToUniversalTime().Subtract(DateTime.UtcNow)).TotalMilliseconds;
                _message.properties.Expiration = ((Int64)expiration).ToString("d");
            }

            return _message;
        }
    }
}
