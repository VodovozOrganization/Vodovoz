using System;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Contacts
{
	/// <summary>
	/// Список контактов по ЭДО
	/// </summary>
	[XmlRoot(ElementName = "Contacts", Namespace = XmlNamespace)]
	[Serializable]
	public class EdoContactList
	{
		[XmlIgnore]
		public const string XmlNamespace = "http://api-invoice.taxcom.ru/contacts";
		
		public EdoContactList() { }

		private EdoContactList(EdoContactInfo[] contacts)
		{
			Contacts = contacts;
		}
		
		/// <summary>
		/// Информация о контакте по ЭДО
		/// </summary>
		[XmlElement(ElementName = "Contact")]
		public EdoContactInfo[] Contacts { get; set; }
		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute(AttributeName = "Asof")]
		public DateTime Asof { get; set; }
		/// <summary>
		/// 
		/// </summary>
		[XmlElement(ElementName = "TemplateID")]
		public Guid TemplateId { get; set; }
		
		public static EdoContactList CreateForCheckCounterparty(string inn, string kpp)
		{
			var contactInfo = EdoContactInfo.CreateForCheckCounterparty(inn, kpp);
			return new EdoContactList(new[] { contactInfo });
		}

		public static EdoContactList Create(string inn, string kpp, string email, string edxClientId, string comment = null)
		{
			var contactInfo = EdoContactInfo.Create(inn, kpp, email, edxClientId, comment);
			return new EdoContactList(new[] { contactInfo });
		}
		
		public static EdoContactList Create(
			string inn,
			string kpp,
			string organizationName,
			string operatorId,
			string email,
			string scanFileName,
			string scanFile,
			string comment)
		{
			var contactInfo = EdoContactInfo.Create(inn, kpp, organizationName, operatorId, email, scanFileName, scanFile, comment);
			return new EdoContactList(new[] { contactInfo });
		}
	}
}
