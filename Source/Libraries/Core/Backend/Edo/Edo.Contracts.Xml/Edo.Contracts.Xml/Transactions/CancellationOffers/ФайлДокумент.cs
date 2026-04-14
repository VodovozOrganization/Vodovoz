using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Transactions.CancellationOffers
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
		public УчастЭДОТип УчастЭДО { get; set; }

		/// <remarks/>
		public ФайлДокументСвПредАн СвПредАн { get; set; }

		/// <remarks/>
		public УчастЭДОТип НапрПредАн { get; set; }

		/// <remarks/>
		public ПодписантТип Подписант { get; set; }
	}
}
