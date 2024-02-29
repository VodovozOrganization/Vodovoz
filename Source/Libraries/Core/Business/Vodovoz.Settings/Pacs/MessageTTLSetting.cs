namespace Vodovoz.Settings.Pacs
{
	public class MessageTTLSetting
	{
		/// <summary>
		/// Message class name with namespace
		/// </summary>
		public string ClassFullName { get; set; }

		/// <summary>
		/// Time to live in seconds
		/// </summary>
		public int TTL { get; set; }
	}
}
