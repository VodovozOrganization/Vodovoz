namespace TaxcomEdo.Contracts.Xml.Container.Entities.Card
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/card")]
	public class DefinitionType
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public DefinitionTypeName Name { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool NameSpecified { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public bool ResignRequired { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool ResignRequiredSpecified { get; set; }
	}
}
