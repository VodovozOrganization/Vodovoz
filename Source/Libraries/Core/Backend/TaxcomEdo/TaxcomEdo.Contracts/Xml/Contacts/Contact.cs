namespace TaxcomEdo.Contracts.Xml.Contacts
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/contacts")]
	public class Contact
	{
		/// <remarks/>
		public ContactState State { get; set; }

		/// <remarks/>
		public string Name { get; set; }

		/// <remarks/>
		public string Inn { get; set; }

		/// <remarks/>
		public string Kpp { get; set; }

		/// <remarks/>
		public string Email { get; set; }

		/// <remarks/>
		public string Login { get; set; }

		/// <remarks/>
		public string EDXClientId { get; set; }

		/// <remarks/>
		public string ExternalContactId { get; set; }

		/// <remarks/>
		public string Comment { get; set; }

		/// <remarks/>
		public ContactAgreements Agreements { get; set; }

		/// <remarks/>
		public OrganizationStructureType OrganizationStructure { get; set; }
	}
}
