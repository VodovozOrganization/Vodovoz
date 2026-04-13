namespace TaxcomEdo.Contracts.Xml.Container.Entities.Warrant
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/warrant")]
	public class WarrantCard
	{
		/// <remarks/>
		public WarrantCardDescription Description { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("DocSign", IsNullable = false)]
		public WarrantCardDocSign[] ToSign { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("AdditionalParameter", IsNullable = false)]
		public WarrantCardAdditionalParameter[] AdditionalData { get; set; }
	}
}
