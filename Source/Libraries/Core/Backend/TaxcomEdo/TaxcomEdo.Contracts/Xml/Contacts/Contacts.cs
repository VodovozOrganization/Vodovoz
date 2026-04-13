namespace TaxcomEdo.Contracts.Xml.Contacts
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/contacts")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://api-invoice.taxcom.ru/contacts", IsNullable = false)]
	public class Contacts
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Contact")]
		public Contact[] ContactsArray { get; set; }

		/// <remarks/>
		public string TemplateID { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute]
		public System.DateTime Asof { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute]
		public bool AsofSpecified { get; set; }
	}
}
