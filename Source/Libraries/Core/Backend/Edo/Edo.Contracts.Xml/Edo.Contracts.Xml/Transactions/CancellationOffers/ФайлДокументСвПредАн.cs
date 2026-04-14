using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Transactions.CancellationOffers
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокументСвПредАн
	{
		/// <remarks/>
		[XmlElement("СведАнФайл")]
		public ФайлДокументСвПредАнСведАнФайл СведАнФайл { get; set; }

		/// <remarks/>
		[XmlAttribute("ТекстПредАн")]
		public string ТекстПредАн { get; set; }
	}
}
