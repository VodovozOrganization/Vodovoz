using System.Net;

namespace SmsPaymentService
{
    public struct StatusCode
    {
        public StatusCode(HttpStatusCode code)
        {
            status = (int)code;
        }
        public int status { get; set; }
    }
}