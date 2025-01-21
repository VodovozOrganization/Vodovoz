using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.XmlWrappers.Warrant
{
	[GeneratedCode("xsd", "4.6.1590.0")]
	[DesignerCategory("code")]
	[XmlRoot(Namespace = "", IsNullable = false)]
	[Serializable]
	public class WarrantCardAdditionalParameter
	{
		private string nameField;
		private string valueField;

		[XmlAttribute]
		public string Name
		{
			get => this.nameField;
			set => this.nameField = value;
		}

		[XmlAttribute]
		public string Value
		{
			get => this.valueField;
			set => this.valueField = value;
		}
	}
}
