using System.Xml.Serialization;
using Edo.Contracts.Xml.Transactions.PostDateConfirmations;

namespace Edo.Contracts.Xml.Transactions.SendConfirmations
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокументСведПодтв
	{
		/// <remarks/>
		[XmlElement("СведОтпрФайл")]
		public ФайлДокументСведПодтвСведОтпрФайл СведОтпрФайл { get; set; }

		/// <remarks/>
		[XmlAttribute("ДатаОтпр")]
		public string ДатаОтпр { get; set; }

		/// <remarks/>
		[XmlAttribute("ВремяОтпр")]
		public string ВремяОтпр { get; set; }
	}
}
