using System.Xml.Serialization;
using Edo.Contracts.Xml.Documents;

namespace Edo.Contracts.Xml.Transactions.CancellationOffers
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public sealed class ПодписантТип
	{
		/// <remarks/>
		[XmlElement("ФИО")]
		public FullName FullName { get; set; }

		/// <remarks/>
		[XmlAttribute("Должность")]
		public string JobPosition { get; set; }
	}
}
