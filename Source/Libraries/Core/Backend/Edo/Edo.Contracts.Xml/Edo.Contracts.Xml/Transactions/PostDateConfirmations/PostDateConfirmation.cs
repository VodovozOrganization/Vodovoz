using System.Xml.Serialization;
using Edo.Contracts.Xml.Documents;

namespace Edo.Contracts.Xml.Transactions.PostDateConfirmations
{
	/// <summary>
	/// Подтверждение получения документа
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(ElementName = "Файл", Namespace = "", IsNullable = false)]
	public sealed class PostDateConfirmation
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
		public FileVersionType ВерсФорм { get; set; } = FileVersionType.Version103;
	}
}
