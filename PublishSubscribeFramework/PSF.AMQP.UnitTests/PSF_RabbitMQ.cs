using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PSF.AMQP.RabbitMq;

namespace PSF.AMQP.UnitTests
{
    [TestClass]
    public class PSF_RabbitMQ
    {
        private const string host = "[apply your host address]";
        private const string topic = "commands_tests_m";
        private const string subscription = "test";


        internal class Command : INotification
        {
            public int id { get; set; }
        }

        internal interface IRequest
        {

        }

        internal interface INotification
        {

        }

        private int teste = 0;

        private void receiveCallback(dynamic message)
        {
            Command cmd = message;
            this.teste = cmd.id;
            return;
        }

        [TestInitialize]
        public void init()
        {
            this.teste = 0;
        }

        [TestMethod]
        public void Publish_Test()
        {
            Publish publish = new Publish(topic, host);
            publish.Send(new Command { id = 10 }, expireMessage: DateTime.UtcNow.AddMinutes(1), scheduleDelivery: DateTime.UtcNow.AddSeconds(30));
        }

        [TestMethod]
        public void Subscribe_Test()
        {
            var subscribe = new Subscribe<IRequest, INotification>(typeof(Command), topic, host, subscriptionName: subscription);
            subscribe.OnMessage(receiveCallback);
            Publish_Test();
            while (true)
            {
                if (this.teste != 0)
                    break;
            }
        }
    }
}
