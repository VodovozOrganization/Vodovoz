using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Transactions.Other
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокументПодписантСвДоверЭл
	{
		/// <remarks/>
		[XmlAttribute("НомДовер")]
		public string НомДовер { get; set; }

		/// <remarks/>
		[XmlAttribute("ДатаВыдДовер")]
		public string ДатаВыдДовер { get; set; }

		/// <remarks/>
		[XmlAttribute("ВнНомДовер")]
		public string ВнНомДовер { get; set; }

		/// <remarks/>
		[XmlAttribute("ДатаВнРегДовер")]
		public string ДатаВнРегДовер { get; set; }

		/// <remarks/>
		[XmlAttribute("СпособПредставл")]
		public ФайлДокументПодписантСвДоверЭлСпособПредставл СпособПредставл { get; set; }

		/// <remarks/>
		[XmlAttribute("ИдСистХран")]
		public string ИдСистХран { get; set; }
	}
}
