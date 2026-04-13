namespace TaxcomEdo.Contracts.Xml.Container.Entities.Warrant
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/warrant")]
	public class WarrantCardDescription
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Files", typeof(WarrantCardDescriptionFiles))]
		[System.Xml.Serialization.XmlElementAttribute("Meta", typeof(WarrantCardDescriptionMeta))]
		public object Item { get; set; }
	}
}
