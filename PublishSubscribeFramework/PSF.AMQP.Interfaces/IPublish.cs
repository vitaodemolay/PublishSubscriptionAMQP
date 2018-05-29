using System;
using System.Threading.Tasks;

namespace PSF.AMQP.Interfaces
{
    /// <summary>
    /// The Publish is the Sender Object.  It have two methods for send the message for topic/queue  
    /// </summary>
    public interface IPublish : IDisposable
    {
        /// <summary>
        /// The send method serialize the message object, mark this with configurations of expire and schedule delivery and send this for topic/queue;
        /// </summary>
        /// <typeparam name="T">Message Type</typeparam>
        /// <param name="message">Message object for to send</param>
        /// <param name="expireMessage">Expiration date</param>
        /// <param name="scheduleDelivery">Schedule Delivery date (delivery with delay)</param>
        void Send<T>(T message, DateTime? expireMessage = null, DateTime? scheduleDelivery = null);

        /// <summary>
        /// The send async method serialize the message object, mark this with configurations of expire and schedule delivery and send this for topic/queue;
        /// </summary>
        /// <typeparam name="T">Message Type</typeparam>
        /// <param name="message">Message object for to send</param>
        /// <param name="expireMessage">Expiration date</param>
        /// <param name="scheduleDelivery">Schedule Delivery date (delivery with delay)</param>
        Task SendAsync<T>(T message, DateTime? expireMessage = null, DateTime? scheduleDelivery = null);

    }
}
