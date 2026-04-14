using System.Xml.Serialization;
using Edo.Contracts.Xml.Documents;

namespace Edo.Contracts.Xml.Transactions.Other
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокументОтпрДок
	{
		/// <remarks/>
		[XmlElement("ИП", typeof(ФЛТип))]
		[XmlElement("ОперЭДО", typeof(EdoOperator))]
		[XmlElement("СвИнУчастНеУч", typeof(СвИнУчЭДОНеУчТип))]
		[XmlElement("ФЛ", typeof(ФЛТип))]
		[XmlElement("ЮЛ", typeof(ЮЛТип))]
		[XmlChoiceIdentifier("ItemElementName")]
		public object Item { get; set; }

		/// <remarks/>
		[XmlIgnore]
		public ItemChoiceType ItemElementName { get; set; }

		/// <remarks/>
		[XmlAttribute("ИдУчастЭДО")]
		public string ИдУчастЭДО { get; set; }
	}
}
