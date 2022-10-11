using System;
using SmsSendInterface;
using SmsBlissAPI.Model;
namespace SmsBlissSendService
{
	public class SmsBlissSentResult : ISmsSendResult
	{
		public SmsSentStatus Status { get; set; }

		public string ServerId { get; set; }

		public string LocalId { get; set; }

		public string Description { get; set; }

		public SmsBlissSentResult(SmsSentStatus status)
		{
			Status = status;
		}

		public SmsBlissSentResult(MessageResponse messageResponse)
		{
			switch(messageResponse.Status) {
			case MessageResponseStatus.Accepted:
				Status = SmsSentStatus.Accepted;
				break;
			case MessageResponseStatus.InvalidMobilePhone:
				Status = SmsSentStatus.InvalidMobilePhone;
				break;
			case MessageResponseStatus.TextIsEmpty:
				Status = SmsSentStatus.TextIsEmpty;
				break;
			case MessageResponseStatus.SenderAddressInvalid:
				Status = SmsSentStatus.SenderAddressInvalid;
				break;
			case MessageResponseStatus.NotEnoughBalance:
				Status = SmsSentStatus.NotEnoughBalance;
				break;
			//не используются
			case MessageResponseStatus.InvalidStatusQueueName:
			case MessageResponseStatus.InvalidScheduleTimeFormat:
			case MessageResponseStatus.WapUrlInvalid:
			default:
				Status = SmsSentStatus.UnknownError;
				Description = "Неизвестная ошибка";
				break;
			}

			ServerId = messageResponse.SmscId;
			LocalId = messageResponse.ClientId;
		}
	}
}
