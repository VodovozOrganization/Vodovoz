using Edo.Contracts.Xml.Documents;
using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Transactions.CancellationOffers
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(ElementName = "Файл", Namespace = "", IsNullable = false)]
	public sealed class CancellationOffer
	{
		/// <remarks/>
		[XmlElement("Документ")]
		public ФайлДокумент Документ { get; set; }

		/// <remarks/>
		[XmlAttribute("ИдФайл")]
		public string ИдФайл { get; set; }

		/// <remarks/>
		[XmlAttribute("ВерсПрог")]
		public string ВерсПрог { get; set; }

		/// <remarks/>
		[XmlAttribute("ВерсФорм")]
		public FileVersionType ВерсФорм { get; set; } = FileVersionType.Version102;
	}
}
