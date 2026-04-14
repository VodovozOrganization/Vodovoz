using System.Xml.Serialization;
using Edo.Contracts.Xml.Documents;

namespace Edo.Contracts.Xml.Transactions.CancellationOffers
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public sealed class УчастЭДОТип
	{
		/// <remarks/>
		[XmlElement("ИП", typeof(ФЛТип))]
		[XmlElement("ЮЛ", typeof(ЮЛТип))]
		public object Item { get; set; }

		/// <remarks/>
		[XmlAttribute("ИдУчастЭДО")]
		public string ИдУчастЭДО { get; set; }
	}
}
