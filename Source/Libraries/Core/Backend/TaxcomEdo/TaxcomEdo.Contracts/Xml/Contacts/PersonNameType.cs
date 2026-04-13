using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Xml.Contacts
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute]
	[System.Diagnostics.DebuggerStepThroughAttribute]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(Namespace = "http://api-invoice.taxcom.ru/contacts")]
	public class PersonNameType
	{
		/// <remarks/>
		[XmlAttribute]
		public string LastName { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string FirstName { get; set; }

		/// <remarks/>
		[XmlAttribute]
		public string MiddleName { get; set; }
	}
}
