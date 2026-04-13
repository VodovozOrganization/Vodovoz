namespace TaxcomEdo.Contracts.Xml.Container.Entities.Warrant
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://api-invoice.taxcom.ru/warrant")]
	public class WarrantCardAdditionalParameter
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Name { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Value { get; set; }

		public static WarrantCardAdditionalParameter Create(string name, string value)
		{
			return new WarrantCardAdditionalParameter
			{
				Name = name,
				Value = value
			};
		}
	}
}
