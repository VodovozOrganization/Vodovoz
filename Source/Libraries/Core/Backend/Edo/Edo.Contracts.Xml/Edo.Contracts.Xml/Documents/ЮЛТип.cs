using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Documents
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public class ЮЛТип
	{
		/// <remarks/>
		[XmlAttribute(AttributeName = "НаимОрг")]
		public string OrganizationName { get; set; }

		/// <remarks/>
		[XmlAttribute(AttributeName = "ИННЮЛ")]
		public string Inn { get; set; }

		/// <remarks/>
		[XmlAttribute(AttributeName = "КПП")]
		public string Kpp { get; set; }
	}
}
