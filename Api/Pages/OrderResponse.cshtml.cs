using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Api.Pages
{
    public class OrderResponseModel : PageModel
    {
        [FromQuery(Name = "Vnp_TxnRef")]
        public string Vnp_TxnRef { get; set; }
        [FromQuery(Name = "Vnp_Amount")]
        public string Vnp_Amount { get; set; }
        [FromQuery(Name = "Vnp_ResponseCode")]
        public string Vnp_ResponseCode { get; set; }
        [FromQuery(Name = "Vnp_TransactionStatus")]
        public string Vnp_TransactionStatus { get; set; }
        [FromQuery(Name = "Vnp_SecureHash")]
        public string Vnp_SecureHash { get; set; }

        public string message;

        static HttpClient myAppHTTPClient = new HttpClient();
        public async Task OnGet()
        {
            string host = Request.Scheme + "://" + Request.Host;
            string pathname = Url.Action("GetPaymentForOrderResponse", "VNPay");

            string path = pathname + "?Vnp_TxnRef=" + Vnp_TxnRef + "&Vnp_Amount=" + Vnp_Amount + "&Vnp_ResponseCode=" + Vnp_ResponseCode + "&Vnp_TransactionStatus=" + Vnp_TransactionStatus + "&Vnp_SecureHash=" + Vnp_SecureHash;
            string requestUrl = host + path;

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();

            try
            {
                HttpResponseMessage responseMessage = await myAppHTTPClient.GetAsync(requestUrl);
                HttpContent content = responseMessage.Content;
                string message = await content.ReadAsStringAsync();
                Console.WriteLine("The output from thirdparty is: {0}", message);
                this.message = message;
                if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    this.message = "Something went wrong";
                }
                RedirectToPage();
            }
            catch (HttpRequestException exception)
            {
                Console.WriteLine("An HTTP request exception occurred. {0}", exception.Message);
                this.message = exception.Message;
            }
        }
    }
}
