using System.Xml.Serialization;
using Edo.Contracts.Xml.Transactions.PostDateConfirmations;

namespace Edo.Contracts.Xml.Documents
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class УчастЭДОТип
	{
		/// <remarks/>
		[XmlElement("ИП", typeof(ФЛТип))]
		[XmlElement("СвИнУчастНеУч", typeof(СвИнУчЭДОНеУчТип))]
		[XmlElement("ФЛ", typeof(ФЛТип))]
		[XmlElement("ЮЛ", typeof(ЮЛТип))]
		[XmlChoiceIdentifier("ItemElementName")]
		public object Item { get; set; }

		/// <remarks/>
		[XmlIgnore()]
		public ItemChoiceType ItemElementName { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string ИдУчастЭДО { get; set; }
	}
}
