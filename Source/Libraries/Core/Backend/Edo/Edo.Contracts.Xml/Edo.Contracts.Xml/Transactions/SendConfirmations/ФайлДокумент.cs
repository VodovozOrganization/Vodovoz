using System.Xml.Serialization;
using Edo.Contracts.Xml.Documents;
using Edo.Contracts.Xml.Transactions.Other;
using ФайлДокументПодписант = Edo.Contracts.Xml.Documents.ФайлДокументПодписант;

namespace Edo.Contracts.Xml.Transactions.SendConfirmations
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокумент
	{
		/// <remarks/>
		[XmlElement("ОперЭДО")]
		public EdoOperator EdoOperator { get; set; }

		/// <remarks/>
		[XmlElement("СведПодтв")]
		public ФайлДокументСведПодтв СведПодтв { get; set; }

		/// <remarks/>
		[XmlElement("СвОтпрДок")]
		public УчастЭДОТип СвОтпрДок { get; set; }

		/// <remarks/>
		[XmlElement("СвПолДок")]
		public УчастЭДОТип СвПолДок { get; set; }

		/// <remarks/>
		[XmlElement("Подписант")]
		public ФайлДокументПодписант Подписант { get; set; }

		/// <remarks/>
		[XmlAttribute(AttributeName = "КНД")]
		public FiscalDocumentClassifiers FiscalDocumentClassifiers { get; set; } = FiscalDocumentClassifiers.KND1115111;
	}
}
