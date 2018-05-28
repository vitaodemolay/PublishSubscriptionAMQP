using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using PSF.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PSF.AMQP.AzureServiceBus
{
    public class Publish : IPublish
    {

        private readonly TopicClient topicClient;


        public Publish(string connectionString, string topicName)
        {
            Util.BusHelpers.InitializeTopic(connectionString, topicName);
            this.topicClient = TopicClient.CreateFromConnectionString(connectionString, topicName);
        }

        public void Dispose()
        {
            if (this.topicClient != null && !this.topicClient.IsClosed)
            {
                this.topicClient.Close();
            }
        }

        public void Send<T>(T message, DateTime? expireMessage = null, DateTime? scheduleDelivery = null)
        {
            this.topicClient.Send(MessagesMaker(message, expireMessage, scheduleDelivery));
        }



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
