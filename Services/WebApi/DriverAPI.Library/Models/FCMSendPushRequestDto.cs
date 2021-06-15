namespace DriverAPI.Library.Models
{
	public class FCMSendPushRequestDto
	{
		public string to { get; set; }
		public FCMSendPushMessageDto data { get; set; }
	}
}
