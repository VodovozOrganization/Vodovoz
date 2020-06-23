using System.Runtime.Serialization;
using Vodovoz.Domain;

namespace SmsPaymentService
{
	[DataContract]
	public class PaymentResult
	{
		public enum MessageStatus
		{
			Ok,
			Error
		}

		[DataMember]
		public MessageStatus Status { get; set; }

		private string errorDescription;
		[DataMember]
		public string ErrorDescription {
			get => errorDescription;
			set {
				Status = MessageStatus.Error;
				PaymentStatus = null;
				errorDescription = value;
			}
		}

		[DataMember]
		public SmsPaymentStatus? PaymentStatus { get; set; }

		/// <summary>
		/// Создает результат со статусом Ok без информации о статусе платежа
		/// </summary>
		public PaymentResult()
		{
			PaymentStatus = null;
		}

		/// <summary>
		/// Создает результат со статусом Ok
		/// </summary>
		public PaymentResult(SmsPaymentStatus smsPaymentStatus)
		{
			PaymentStatus = smsPaymentStatus;
		}

		/// <summary>
		/// Создает результат со статусом Error
		/// </summary>
		public PaymentResult(string errorDescription)
		{
			ErrorDescription = errorDescription;
			PaymentStatus = null;
		}
	}
	
}
