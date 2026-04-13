namespace TaxcomEdo.Contracts.Xml.Container.Entities.Card
{
	/// <remarks/>
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(Card))]
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	//[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/card")]
	public class Definition
	{
		/// <remarks/>
		public DefinitionIdentifiers Identifiers { get; set; }

		/// <remarks/>
		public DefinitionType Type { get; set; }
	}
}
