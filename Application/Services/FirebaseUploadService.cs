using Application.Common.Interfaces;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class FirebaseUploadService : IUploadService
    {

        private static string API_KEY = "AIzaSyBPgFj2G8bipS5N7nRSYNdxa8MuPrhMKaM";
        private static string BUCKET = "fa22se19-oms.appspot.com";
        private static string AUTH_EMAIL = "administrator@oms.com";
        private static string AUTH_PASSWORD = "~d[3f6mz)yxx'D=y";
        public async Task<string> UploadAsync(IFormFile file, string folder = "files")
        {
            string link = string.Empty;
            try
            {
                using (var stream = file.OpenReadStream())
                {

                    var auth = new FirebaseAuthProvider(new FirebaseConfig(API_KEY));
                    var signIn = await auth.SignInWithEmailAndPasswordAsync(AUTH_EMAIL, AUTH_PASSWORD);

                    var cancellation = new CancellationTokenSource();

                    var task = new FirebaseStorage(
                        BUCKET,
                        new FirebaseStorageOptions
                        {

                            AuthTokenAsyncFactory = () => Task.FromResult(signIn.FirebaseToken),
                            ThrowOnCancel = true
                        })
                        .Child(folder)
                        .Child(Path.GetRandomFileName() + Path.GetExtension(file.FileName))
                        .PutAsync(stream);
                    link = await task;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return link;
        }
    }
}
