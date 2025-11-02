namespace Vodovoz.Controllers
{
	public class SentMovementsNotificationDetails
	{
		public bool NeedNotify { get; set; }
		public (bool Alert, string Message) Notification { get; set; }
	}
}
