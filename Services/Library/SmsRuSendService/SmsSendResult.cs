using SmsSendInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmsRuSendService
{
    public class SmsSendResult : ISmsSendResult
    {
        public SmsSentStatus Status { get; set; }

        public string ServerId { get; set; }

        public string LocalId { get; set; }

        public string Description { get; set; }

        public SmsSendResult(SmsSentStatus status)
        {
            Status = status;
        }
    }
}
