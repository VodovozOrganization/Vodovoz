using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Xml.Serialization;
using TaxcomEdo.Contracts.XmlWrappers.Warrant;

namespace TaxcomEdo.Contracts.XmlWrappers
{
	[GeneratedCode("xsd", "4.6.1590.0")]
	[DesignerCategory("code")]
	[XmlRoot(ElementName = "Warrant", Namespace = "", IsNullable = false)]
	[Serializable]
	public class WarrantXml
	{
		private WarrantCard[] warrantCardField;

		[XmlElement("WarrantCard")]
		public WarrantCard[] WarrantCards
		{
			get => this.warrantCardField;
			set => this.warrantCardField = value;
		}
	}
}
