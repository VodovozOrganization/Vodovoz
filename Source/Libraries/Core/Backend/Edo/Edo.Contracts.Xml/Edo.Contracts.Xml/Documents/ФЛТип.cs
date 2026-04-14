using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Documents
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public class ФЛТип
	{
		/// <remarks/>
		[XmlAttribute(AttributeName = "ФИО")]
		public FullName FullName { get; set; }

		/// <remarks/>
		[XmlAttribute(AttributeName = "ИННФЛ")]
		public string Inn { get; set; }
	}
}
