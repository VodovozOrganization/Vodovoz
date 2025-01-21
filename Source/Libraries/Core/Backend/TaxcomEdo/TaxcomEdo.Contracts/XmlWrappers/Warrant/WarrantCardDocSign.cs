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
	public class WarrantCardDocSign
	{
		private string fileField;

		[XmlAttribute]
		public string file
		{
			get => this.fileField;
			set => this.fileField = value;
		}
	}
}
