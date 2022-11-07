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
using Firebase.Auth;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using static Google.Apis.Requests.BatchRequest;

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

        private MulticastMessage CreateNotificationToMultipleDevice(string title, string notificationBody, List<string> tokens)
        {
            return new MulticastMessage()
            {
                Tokens = tokens,
                Notification = new Notification()
                {
                    Body = notificationBody,
                    Title = title
                }
            };
        }

        public async Task<int> SendNotificationToMultipleDevice(List<string> tokens, string title, string body)
        {
            var result = await messaging.SendMulticastAsync(CreateNotificationToMultipleDevice(title, body, tokens));
            //do something with result
            Console.WriteLine("success: " + result);
            return result.SuccessCount;
        }

        private Message CreateNotificationToTopic(string title, string notificationBody, string topic)
        {
            return new Message()
            {
                Topic = topic,
                Notification = new Notification()
                {
                    Body = notificationBody,
                    Title = title
                }
            };
        }
        public async Task<string> SendNotificationToTopic(string topic, string title, string body)
        {
            var result = await messaging.SendAsync(CreateNotificationToTopic(title, body, topic));
            Console.WriteLine("success: " + result);
            return result;
        }

        public async Task<int> SubcribeFromTopic(List<string> tokens, string topic)
        {
            var result = await messaging.SubscribeToTopicAsync(tokens, topic);
            Console.WriteLine("success: " + result);
            return result.SuccessCount;
        }
        public async Task<int> UnsubcribeFromTopic(List<string> tokens, string topic)
        {
            var result = await messaging.UnsubscribeFromTopicAsync(tokens, topic);
            Console.WriteLine("success: " + result);
            return result.SuccessCount;
        }
    }
}
