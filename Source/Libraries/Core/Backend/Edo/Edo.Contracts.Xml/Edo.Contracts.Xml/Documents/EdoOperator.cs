using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Transactions.Other
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class EdoOperator
	{
		/// <remarks/>
		[XmlAttribute("НаимОрг")]
		public string НаимОрг { get; set; }

		/// <remarks/>
		[XmlAttribute("ИННЮЛ")]
		public string ИННЮЛ { get; set; }

		/// <remarks/>
		[XmlAttribute("ИдОперЭДО")]
		public string ИдОперЭДО { get; set; }
	}
}
