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
	public class WarrantCardDescriptionFilesWarrantImage
	{
		private string pathField;

		[XmlAttribute]
		public string Path
		{
			get => this.pathField;
			set => this.pathField = value;
		}
	}
}
