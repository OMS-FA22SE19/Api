using System.Net;

namespace Application.Models
{
    public sealed class Response<T>
    {
        public T Data { get; set; }
        public bool Succeeded { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;
        public string Message { get; set; }
        public Response()
        {

        }

        public Response(T data)
        {
            Data = data;
            Succeeded = true;
            StatusCode = HttpStatusCode.OK;
        }

        public Response(string error)
        {
            Succeeded = false;
            Message = error;
            Data = default;
        }
    }
}
