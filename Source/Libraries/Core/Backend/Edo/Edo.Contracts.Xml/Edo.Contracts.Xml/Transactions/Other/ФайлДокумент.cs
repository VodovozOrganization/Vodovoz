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
	public sealed class ФайлДокумент
	{
		/// <remarks/>
		[XmlElement("УчастЭДО")]
		public УчастЭДОТип УчастЭДО { get; set; }

		/// <remarks/>
		[XmlElement("СвИзвПолуч")]
		public ФайлДокументСвИзвПолуч СвИзвПолуч { get; set; }

		/// <remarks/>
		[XmlElement("ОтпрДок")]
		public ФайлДокументОтпрДок ОтпрДок { get; set; }

		/// <remarks/>
		[XmlElement("Подписант")]
		public ФайлДокументПодписант Подписант { get; set; }

		/// <remarks/>
		[XmlAttribute("КНД")]
		public FiscalDocumentClassifiers FiscalDocumentClassifiers { get; set; } = FiscalDocumentClassifiers.KND1115110;
	}
}
