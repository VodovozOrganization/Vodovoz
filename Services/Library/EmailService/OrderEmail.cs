using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Vodovoz.Domain.Orders.Documents;

namespace EmailService
{
	[DataContract]
	public class OrderEmail
	{
		[DataMember]
		public string Title { get; set; }

		[DataMember]
		public string Text { get; set; }

		public string HtmlTextBase64 => Base64Encode(HtmlText);

		/// <summary>
		/// Список файлов в виде словаря, где ключ - имя файла, значение - строка закодированная в base64
		/// </summary>
		[DataMember]
		public Dictionary<string, string> AttachmentsBinary { get; set; }

		[DataMember]
		public Dictionary<string, EmailAttachment> InlinedAttachments { get; set; }

		[DataMember]
		public string HtmlText { get; set; }

		[DataMember]
		public EmailContact Sender { get; set; }

		[DataMember]
		public EmailContact Recipient { get; set; }

		[DataMember]
		public List<EmailContact> HiddenCopyRecipients { get; set; }

		[DataMember]
		public OrderDocumentType OrderDocumentType { get; set; }

		[DataMember]
		public int Order { get; set; }

		[DataMember]
		public bool ManualSending { get; set; }

		[DataMember]
		public int AuthorId { get; set; }

		public int StoredEmailId { get; set; }

		public int SendAttemptsCount { get; set; }

		public string SenderStringValue {
			get { return Sender.ToString(); }
			set { Sender = EmailContact.Parse(value); }
		}

		public string RecipientStringValue {
			get { return Recipient.ToString(); }
			set { Recipient = EmailContact.Parse(value); }
		}

		public string HiddenCopyRecipientsStringValue {
			get { return GetEmailContactStrings(HiddenCopyRecipients); }
			set { HiddenCopyRecipients = ParseContacts(value); }
		}

		public void AddAttachment(string fileName, MemoryStream stream)
		{
			if(AttachmentsBinary == null) {
				AttachmentsBinary = new Dictionary<string, string>();
			}
			byte[] data = stream.ToArray();

			string dataStr = Convert.ToBase64String(data);

			new List<string> {
					"\\", "\"", "/", ":", "*",
					"?", "<", ">", "|", "+",
					"%", "!", "@", ","
			}.ForEach(m => fileName = fileName.Replace(m, ""));

			AttachmentsBinary.Add(fileName, dataStr);
		}

		public void AddInlinedAttachment(string id, string contentType, string fileName, string base64FileContent)
		{
			if(InlinedAttachments == null) {
				InlinedAttachments = new Dictionary<string, EmailAttachment>();
			}

			InlinedAttachments.Add(id, new EmailAttachment {
				ContentType = contentType,
				FileName = fileName,
				Base64String = base64FileContent
			});
		}

		private string GetEmailContactStrings(List<EmailContact> contacts)
		{
			string result = "";
			if(contacts == null) {
				return result;
			}
			foreach(var item in contacts) {
				if(!string.IsNullOrWhiteSpace(result)) {
					result += ",";
				}
				result += item.ToString();
			}
			return result;
		}

		private List<EmailContact> ParseContacts(string contacts)
		{
			if(string.IsNullOrWhiteSpace(contacts)) {
				return null;
			}
			var contactsArray = contacts.Split(',');
			if(contactsArray.Length == 0) {
				return null;
			}
			List<EmailContact> result = new List<EmailContact>();
			foreach(var item in contactsArray) {
				var ec = EmailContact.Parse(item);
				if(ec == null) {
					continue;
				}
				result.Add(ec);
			}
			return result;
		}

		private static string Base64Encode(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return Convert.ToBase64String(plainTextBytes);
		}

		private static string Base64Decode(string base64String)
		{
			var decodedBytes = Convert.FromBase64String(base64String);
			return System.Text.Encoding.UTF8.GetString(decodedBytes);
		}
	}
}
