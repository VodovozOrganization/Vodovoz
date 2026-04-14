using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Documents
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public sealed class СвИнУчЭДОНеУчТип
	{
		/// <remarks/>
		[XmlAttribute("ИдСтат")]
		public СвИнУчЭДОНеУчТипИдСтат ИдСтат { get; set; }

		/// <remarks/>
		[XmlAttribute("СтрРег")]
		public string СтрРег { get; set; }

		/// <remarks/>
		[XmlAttribute("Наим")]
		public string Наим { get; set; }

		/// <remarks/>
		[XmlAttribute("КодНПРег")]
		public string КодНПРег { get; set; }

		/// <remarks/>
		[XmlAttribute("ИныеСвед")]
		public string ИныеСвед { get; set; }
	}
}
