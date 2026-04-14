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
	public sealed class ФайлДокументПодписант
	{
		/// <remarks/>
		[XmlElement("ФИО")]
		public FullName FullName { get; set; }

		/// <remarks/>
		[XmlElement("СвДоверЭл")]
		public ФайлДокументПодписантСвДоверЭл СвДоверЭл { get; set; }

		/// <remarks/>
		[XmlElement("СвДоверБум")]
		public ФайлДокументПодписантСвДоверБум СвДоверБум { get; set; }

		/// <remarks/>
		[XmlAttribute("СтатПодп")]
		public ФайлДокументПодписантСтатПодп СтатПодп { get; set; }

		/// <remarks/>
		[XmlIgnore()]
		public bool СтатПодпSpecified { get; set; }

		/// <remarks/>
		[XmlAttribute("ТипПодпис")]
		public ФайлДокументПодписантТипПодпис ТипПодпис { get; set; }

		/// <remarks/>
		[XmlIgnore()]
		public bool ТипПодписSpecified { get; set; }

		/// <remarks/>
		[XmlAttribute("Должн")]
		public string Должн { get; set; }
	}
}
