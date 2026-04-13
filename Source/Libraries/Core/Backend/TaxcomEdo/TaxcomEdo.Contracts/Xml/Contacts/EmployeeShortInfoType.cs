namespace TaxcomEdo.Contracts.Xml.Contacts
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/contacts")]
	public class EmployeeShortInfoType
	{
		/// <remarks/>
		public PersonNameType Name { get; set; }

		/// <remarks/>
		public string Position { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute]
		public string ID { get; set; }
	}
}
