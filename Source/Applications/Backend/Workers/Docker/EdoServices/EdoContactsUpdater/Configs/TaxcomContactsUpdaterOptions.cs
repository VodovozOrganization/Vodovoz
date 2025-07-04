namespace EdoContactsUpdater.Configs
{
	public class TaxcomContactsUpdaterOptions
	{
		public static string Path => "ContactsUpdaterOptions";
		
		public int DelayBetweenContactsProcessingInSeconds { get; set; }
		public string EdoAccount { get; set; }
	}
}
