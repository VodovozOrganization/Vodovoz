using System;
using System.Xml.Serialization;
using TaxcomEdo.Contracts.Counterparties;

namespace TaxcomEdo.Contracts.Contacts
{
	/// <summary>
	/// Информация о контакте по ЭДО
	/// </summary>
	[XmlRoot(ElementName = "Contact")]
	[Serializable]
	public class EdoContactInfo
	{
		public EdoContactInfo() { }

		private EdoContactInfo(string inn, string kpp)
		{
			Inn = inn;
			Kpp = kpp;
		}
		
		[XmlElement("ExternalContactId")]
		public string ExternalContactId { get; set; }
		/// <summary>
		/// Номер кабинета ЭДО
		/// </summary>
		[XmlElement(ElementName = "EDXClientId", IsNullable = false)]
		public string EdxClientId { get; set; }
		
		[XmlElement(ElementName = "Name", IsNullable = false)]
		public string Name { get; set; }
		/// <summary>
		/// ИНН клиента
		/// </summary>
		[XmlElement(ElementName = "Inn", IsNullable = false)]
		public string Inn { get; set; }
		
		[XmlElement(ElementName = "Kpp", IsNullable = false)]
		public string Kpp { get; set; }

		[XmlElement(ElementName = "Login", IsNullable = false)]
		public string Login { get; set; }
		
		[XmlElement(ElementName = "Email", IsNullable = false)]
		public string Email { get; set; }
		
		[XmlElement(ElementName = "SenderEmail", IsNullable = false)]
		public string SenderEmail { get; set; }

		[XmlElement(ElementName = "Comment", IsNullable = false)]
		public string Comment { get; set; }

		[XmlElement(ElementName = "RejectComment", IsNullable = false)]
		public string RejectComment { get; set; }
		
		[XmlArray(ElementName = "Agreements")]
		[XmlArrayItem(ElementName = "Agreement")]
		public ContactAgreement[] Agreements { get; set; }
		
		[XmlElement(ElementName = "OrganizationStructure", IsNullable = false)]
		public OrganizationStructure OrganizationStructure { get; set; }
		/// <summary>
		/// Статус
		/// </summary>
		[XmlElement("State")]
		public EdoContactState State { get; set; }
		
		[XmlElement(ElementName = "OperatorId", IsNullable = false)]
		public string OperatorId { get; set; }

		[XmlElement(ElementName = "ScanFilename", IsNullable = false)]
		public string ScanFilename { get; set; }

		[XmlElement(ElementName = "Scan", IsNullable = false)]
		public string Scan { get; set; }

		[XmlElement(ElementName = "Active", IsNullable = false)]
		public string Active { get; set; }

		public static EdoContactInfo CreateForCheckCounterparty(string inn, string kpp) => new EdoContactInfo(inn, kpp);
	}
}
