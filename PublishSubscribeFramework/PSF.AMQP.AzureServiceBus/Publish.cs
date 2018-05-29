using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using PSF.AMQP.Interfaces;



namespace PSF.AMQP.AzureServiceBus
{
    /// <summary>
    /// The Publish is the Sender Object.  It have two methods for send the message for topic/queue on Azure ServiceBus 
    /// </summary>
    public class Publish : IPublish
    {

        private readonly TopicClient topicClient;

        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="connectionString">Connection string of your Azure servicebus</param>
        /// <param name="topicName">Topic Name</param>
        public Publish(string connectionString, string topicName)
        {
            Util.BusHelpers.InitializeTopic(connectionString, topicName);
            this.topicClient = TopicClient.CreateFromConnectionString(connectionString, topicName);
        }

        /// <summary>
        /// Dispose Method
        /// </summary>
        public void Dispose()
        {
            if (this.topicClient != null && !this.topicClient.IsClosed)
            {
                this.topicClient.Close();
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
            this.topicClient.Send(MessagesMaker(message, expireMessage, scheduleDelivery));
        }


        /// <summary>
        /// The send async method serialize the message object, mark this with configurations of expire and schedule delivery and send this for topic/queue;
        /// </summary>
        /// <typeparam name="T">Message Type</typeparam>
        /// <param name="message">Message object for to send</param>
        /// <param name="expireMessage">Expiration date</param>
        /// <param name="scheduleDelivery">Schedule Delivery date (delivery with delay)</param>
        public async Task SendAsync<T>(T message, DateTime? expireMessage = null, DateTime? scheduleDelivery = null)
        {
            await this.topicClient.SendAsync(MessagesMaker(message, expireMessage, scheduleDelivery));
        }



        private BrokeredMessage MessagesMaker(dynamic message, DateTime? expired = null, DateTime? schedule = null)
        {
            var name = message.GetType().Name;
            string jsonMessage = JsonConvert.SerializeObject(message);
            byte[] body = Encoding.UTF8.GetBytes(jsonMessage);

            BrokeredMessage _message = new BrokeredMessage(body)
            {
                MessageId = Guid.NewGuid().ToString(),
                Label = name
            };

            if (expired != null)
                _message.TimeToLive = Convert.ToDateTime(expired).Subtract(DateTime.UtcNow);

            if (schedule != null)
                _message.ScheduledEnqueueTimeUtc = (DateTime)schedule;

            return _message;
        }

    }
}
