using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Transactions.SendConfirmations
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокументСведПодтвСведОтпрФайл
	{
		/// <remarks/>
		[XmlElement("ЭПОтпрФайл")]
		public string[] ЭПОтпрФайл { get; set; }

		/// <remarks/>
		[XmlAttribute("ИмяОтпрФайла")]
		public string ИмяОтпрФайла { get; set; }
	}
}
