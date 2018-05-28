using System;
using System.Threading.Tasks;

namespace PSF.Interfaces
{
    public interface IPublish : IDisposable
    {
        void Send<T>(T message, DateTime? expireMessage = null, DateTime? scheduleDelivery = null);
        Task SendAsync<T>(T message, DateTime? expireMessage = null, DateTime? scheduleDelivery = null);

    }
}
