using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Contacts
{
	/// <summary>
	/// Список контактов по ЭДО
	/// </summary>
	[XmlRoot(ElementName = "Contacts", Namespace = "http://api-invoice.taxcom.ru/contacts")]
	[Serializable]
	public class EdoContactList
	{
		public EdoContactList() { }

		private EdoContactList(EdoContactInfo[] contacts)
		{
			Contacts = contacts;
		}
		
		[XmlIgnore]
		public const string XmlNamespace = "http://api-invoice.taxcom.ru/contacts";
		
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

		public static EdoContactList CreateForCheckContragent(string inn, string kpp)
		{
			var contactInfo = EdoContactInfo.CreateForCheckContragent(inn, kpp);
			return new EdoContactList(new[] { contactInfo });
		}
	}
}
