using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Transactions.PostDateConfirmations
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[XmlType(AnonymousType = true)]
	public sealed class ФайлДокументСведПодтв
	{
		/// <remarks/>
		[XmlElement("СведПолФайл")]
		public ФайлДокументСведПодтвСведПолФайл СведПолФайл { get; set; }

		/// <remarks/>
		[XmlAttribute("ДатаПол")]
		public string ДатаПол { get; set; }

		/// <remarks/>
		[XmlAttribute("ВремяПол")]
		public string ВремяПол { get; set; }
	}
}
