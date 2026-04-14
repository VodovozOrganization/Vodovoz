using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Transactions.Other
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокументСвИзвПолуч
	{
		/// <remarks/>
		[XmlElement("СведПолФайл")]
		public ФайлДокументСвИзвПолучСведПолФайл СведПолФайл { get; set; }

		/// <remarks/>
		[XmlAttribute("ДатаПол")]
		public string ДатаПол { get; set; }

		/// <remarks/>
		[XmlAttribute("ВремяПол")]
		public string ВремяПол { get; set; }
	}
}
