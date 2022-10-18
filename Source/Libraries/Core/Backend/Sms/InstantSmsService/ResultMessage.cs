using System.Runtime.Serialization;

namespace InstantSmsService
{
	[DataContract]
	public class ResultMessage
	{
		[DataMember]
		private string errorDescription;
		/// <summary>
		/// Gets or sets the error description.
		/// Set also changes <see cref="MessageStatus"/> to <see cref="SmsMessageStatus.Error"/>
		/// </summary>
		/// <value>The error description.</value>
		[DataMember]
		public string ErrorDescription { 
		get => errorDescription;
		set {
			MessageStatus = SmsMessageStatus.Error;
			errorDescription = value;
			}
		}

		[DataMember]
		public SmsMessageStatus MessageStatus { get; set; }
		
		public bool IsPaidStatus { get; set; }
	}

	public enum SmsMessageStatus
	{
		Ok,
		Error
	}
}
