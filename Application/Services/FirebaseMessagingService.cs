using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Models;

namespace Application.Services
{
    public class FirebaseMessagingService : IFirebaseMessagingService
    {
        private readonly FirebaseMessaging messaging;

        public FirebaseMessagingService()
        {
            var app = FirebaseApp.Create(new AppOptions() { 
                Credential = GoogleCredential.FromFile("private_key.json").CreateScoped("https://www.googleapis.com/auth/firebase.messaging") 
            });
            messaging = FirebaseMessaging.GetMessaging(app);
        }

        private Message CreateNotification(string title, string notificationBody, string token)
        {
            return new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Body = notificationBody,
                    Title = title
                }
            };
        }

        public async Task<string> SendNotification(string token, string title, string body)
        {
            var result = await messaging.SendAsync(CreateNotification(title, body, token));
            //do something with result
            Console.WriteLine("success: " + result);
            return result;
        }
    }
}
