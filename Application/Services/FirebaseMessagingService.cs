using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Text;
using Application.Common.Interfaces;
using Newtonsoft.Json;

namespace Application.Services
{
    public class FirebaseMessagingService : IFirebaseMessagingService
    {
        private readonly FirebaseMessaging messaging;

        public FirebaseMessagingService()
        {
            var parameters = new JsonCredentialParameters
            {

                Type = JsonCredentialParameters.ServiceAccountCredentialType,
                ProjectId = "fa22se19-oms",
                PrivateKeyId = "69c3c6e73d81a700bf674f36c0fa1c46625d1602",
                PrivateKey = "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCta7TejeUs2PWH\n4hAfKeTz773Bp7kgiMZ4lBNL5k5YQNbYGM2Ua9y3jJ8tZiQr9qIpJvYyRv1MXnuB\ndvKgCzDpXQyYS/QpyzOKwBZBgy8ez4AWLqqNmQgSCNLtecf/aAimYkmN4rl++ZJt\nuy8BlWSGRM5Bw7s/RTKHNvXV1DnAVyrhyJ2Fd/fp9PWK7WDVAEU00F0LEHYm0WDa\nCCedq3tOIMs8XvUcScLx/SUCBzHQyOJ8CdwwXbA6Ge51G7kypfjqhT5XHyGrVRVx\nNJVhqrTumahuPNQIs8nxGIRBWumwBJYzT6ILrK3R0X1q+bymHI4qC7quvdq7XB+j\n9BsQcRQnAgMBAAECggEAHp0By7u+tgECrt8QO4AYw5sRo0rpWj3q00RId8CLGfcw\nNeuC9u1nhBtVyhzay8v46kzPbyaq9j1DjiIiSHBRTRC49XWK/4m4aojoJN50hfrZ\nQJEyWFaoXw6htXwm+wYxaLW3CE36sth5qdc+QAhQO4eP5XoY5t1fQxFHqlyRaUtv\ngawChizAxEQ1obMbdPY1XTWuHJHPIgWutw/Sq+3fPKZQOXYs3GuFqWHXZjz3+rfg\njO9W/g+wwdT6wI6FzN4icQuByc4PCilXfgmGcdMiFtzu7/FmcIHZzKr2Xthzyk5h\nkzAVgBsJxv5dVIo6GXoks8F/9r7uqx0keJOPuykdoQKBgQDY2Q4Ib1V2GfJXqLgk\nyYnK1koqMMBcnN/CJIuwV6sZgnkHUy8Pg7jXIMqVZB40PpV3yMyZwLzdWwy8VUcK\ncN879117SX12wxj8u12ZWq/xqdsNZldNFsbQRmEbUFGTvMKX10nhNXfdSvuFyx6p\nHi70G6xVBCWEfmq+99wl6vwTFwKBgQDMu2beD5QohnT6HZVbBciFlDS87TZIaO7L\nGoLd6LgyTA1yWggUzd09OoQhSMwbyGr5eY3DXhwad112r/1B93A7hMJlk8aDq07C\nF7Cai5SZZvnpz0Nof7GqSCnEYQ2GqNZSJV+k/tJTMUNn3DskcYHKKjSQNpoTaQEN\nsrWMEOzxcQKBgQCPELUnb3yszHWMy+2hp63XOwX3S+69q2odBkt67nwd+myrHoB/\n9eGvXR0K7uDWiyvFuZ7zGawBRp7iakjSZKGDSLJOMrAp+JG1vFAGa0SFZhD1iD20\nyIAmzSgoAxevJuwjjZBamxIX1+6V2OrDnPxRZSiPdoriHR2EHLcL9TqVxQKBgARy\nAT7LGc1zdfDEWazbSEpWOHrtr6MM3Dp+vd6xoeY9roEQIWLKQYaF0b6jRQNJa8E0\n+XYHgha1Bst/8MDQ7ZftPwdTmRjczF38g7InW7ek+8Iu5OjM9P+ch4OjE9cbrWOl\nOhlsCwcRZ8RGjOYyrL3GAtQqy1emjQZOHhEHgQ2RAoGAHSuNDCQqnkJKby39y9py\n+rD/JkXosAU9TUIpR3cO75MKc2AojsefRWwlHuyk1xU0A4PGbL442T6xrbr+mu2V\nBXbEWKwiRc5fvULaFONQEHRzemkWXnKB3rt1dDYrOHm0a1OHT/U2hXRm8BaZ6uH5\nfnKrIxS39KFaJy9uWUsY+3A=\n-----END PRIVATE KEY-----\n",
                ClientEmail = "firebase-adminsdk-p7w5i@fa22se19-oms.iam.gserviceaccount.com",
                ClientId = "114190627581569215145",
                ClientSecret = ""
            };
            string json = JsonConvert.SerializeObject(parameters);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var app = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromStream(stream).CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
            });
            //var app = FirebaseApp.Create(new AppOptions() { 
            //    Credential = GoogleCredential.FromFile("private_key.json").CreateScoped("https://www.googleapis.com/auth/firebase.messaging") 
            //});
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
            try
            {
                var result = await messaging.SendAsync(CreateNotification(title, body, token));
                
                return result;
            }
            catch (FirebaseException ex)
            {
                var errorMessage = $"error code: {ex.ErrorCode} was found please fix";
                return null;
            }
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
