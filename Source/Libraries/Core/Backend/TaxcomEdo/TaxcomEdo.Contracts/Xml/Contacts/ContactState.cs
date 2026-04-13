namespace TaxcomEdo.Contracts.Xml.Contacts
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/contacts")]
	public class ContactState
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute]
		public ContactStateCode Code { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute]
		public string ErrorCode { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute]
		public System.DateTime Changed { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlTextAttribute]
		public string Value { get; set; }
	}
}
