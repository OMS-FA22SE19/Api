using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IFirebaseMessagingService
    {
        //Message CreateNotification(string title, string notificationBody, string token);

        Task<string> SendNotification(string token, string title, string body);
        Task<int> SendNotificationToMultipleDevice(List<string> token, string title, string body);
        Task<string> SendNotificationToTopic(string topic, string title, string body);
        Task<int> SubcribeFromTopic(List<string> tokens, string topic);
        Task<int> UnsubcribeFromTopic(List<string> token, string topic);

    }
}
