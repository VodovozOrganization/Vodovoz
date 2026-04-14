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
	public sealed class ФайлДокументПодписантСвДоверБум
	{
		/// <remarks/>
		[XmlElement("ФИО")]
		public FullName FullName { get; set; }

		/// <remarks/>
		[XmlAttribute("ДатаДовер")]
		public string ДатаДовер { get; set; }

		/// <remarks/>
		[XmlAttribute("СвДовер")]
		public string ВнНомДовер { get; set; }

		/// <remarks/>
		[XmlAttribute("СвДовер")]
		public string СвДовер { get; set; }
	}
}
