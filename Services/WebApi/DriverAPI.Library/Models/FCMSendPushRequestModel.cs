namespace DriverAPI.Library.Models
{
	public class FCMSendPushRequestModel
	{
		public string to { get; set; }
		public FCMSendPushMessageModel data { get; set; }
	}
}
