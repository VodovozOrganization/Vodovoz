using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Transactions.CancellationOffers
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокументСвПредАнСведАнФайл
	{
		/// <remarks/>
		[XmlElement("ЭЦПАнФайл")]
		public string[] ЭЦПАнФайл { get; set; }

		/// <remarks/>
		[XmlAttribute("ИмяАнФайла")]
		public string ИмяАнФайла { get; set; }
	}
}
