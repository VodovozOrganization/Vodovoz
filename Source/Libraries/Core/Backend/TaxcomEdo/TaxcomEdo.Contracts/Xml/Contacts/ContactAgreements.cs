namespace TaxcomEdo.Contracts.Xml.Contacts
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/contacts")]
	public class ContactAgreements
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Agreement")]
		public ContactAgreement[] Agreement { get; set; }
	}
}
