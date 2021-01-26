using System.Runtime.Serialization;

namespace EmailService
{
	[DataContract]
	public class EmailContact
	{
		[DataMember]
		public string Title { get; set; }
		[DataMember]
		public string EmailAddress { get; set; }

		public EmailContact()
		{
		}

		public EmailContact(string title, string email)
		{
			Title = title;
			EmailAddress = email;
		}

		/// <summary>
		/// Parse the specified emailContact. Format: title<Email>
		/// </summary>
		public static EmailContact Parse(string emailContact)
		{
			string[] values = emailContact.Split('<');
			if(values.Length != 2) {
				return null;
			}
			var title = values[0];
			var email = values[1].Substring(0, values[1].Length - 1);
			return new EmailContact(title, email);
		}

		public override string ToString()
		{
			return string.Format("{0}<{1}>", Title, EmailAddress);
		}
	}
}
