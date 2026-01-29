namespace CustomerAppNotifier.Options
{
	public abstract class SourceEventsOptions
	{
		public CounterpartyAssignNotificationOptions CounterpartyAssignNotification { get; set; }
		public LogoutLegalAccountEventOptions LogoutLegalAccountEvent { get; set; }
	}
}
