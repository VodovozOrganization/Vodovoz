using System;
using System.Net;

namespace SmsPaymentService
{
    public struct SendResponse
    {
        public HttpStatusCode? HttpStatusCode { get; set; }
        
        /// <summary>
        /// ID платежа во внешней базе
        /// </summary>
        public int? ExternalId { get; set; }
    }
}