using System;

namespace PSF.AMQP.Interfaces
{
    /// <summary>
    /// The Subscribe is the Receiver object. It create a listener for when to receive a message on an open topic/queue, it start a callback method
    /// </summary>
    /// <typeparam name="IRequest">Here you set a Request interface what is on your code or in other library</typeparam>
    /// <typeparam name="INotification">Here you set a Notification interface what is on your code or in other library</typeparam>
    public interface ISubscribe<IRequest, INotification>
    {

        /// <summary>
        /// This method create a listener and define a callback method for when to receive a message
        /// </summary>
        /// <param name="callback">Callback method delegate</param>
        void OnMessage(Action<object> callback);
    }
}
