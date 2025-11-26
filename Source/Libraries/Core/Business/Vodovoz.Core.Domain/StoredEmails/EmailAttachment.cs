namespace Vodovoz.Core.Domain.StoredEmails
{
	/// <summary>
	/// Приложение к электронному письму
	/// </summary>
	public class EmailAttachment
	{
		public string MIMEType { get; set; }
		public string FileName { get; set; }
		public string Base64Content { get; set; }
	}
}
