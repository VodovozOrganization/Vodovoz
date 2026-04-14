using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Documents
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокументПодписант
	{
		/// <remarks/>
		[XmlElement(ElementName = "ФИО")]
		public FullName FullName { get; set; }

		/// <remarks/>
		[XmlAttribute(AttributeName = "Должность")]
		public string Должность { get; set; }
	}
}
