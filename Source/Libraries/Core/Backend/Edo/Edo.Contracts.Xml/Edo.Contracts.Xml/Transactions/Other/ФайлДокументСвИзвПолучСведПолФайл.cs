using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Transactions.Other
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокументСвИзвПолучСведПолФайл
	{
		/// <remarks/>
		[XmlElement("ЭППолФайл")]
		public string[] ЭППолФайл { get; set; }

		/// <remarks/>
		[XmlAttribute("ИмяПолФайла")]
		public string ИмяПолФайла { get; set; }
	}
}
