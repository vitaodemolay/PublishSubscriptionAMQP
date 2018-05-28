using System;

namespace PSF.Interfaces
{
    public interface ISubscribe<IRequest, INotification>
    {
        void OnMessage(Action<object> callback);
    }
}
