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
    }
}
