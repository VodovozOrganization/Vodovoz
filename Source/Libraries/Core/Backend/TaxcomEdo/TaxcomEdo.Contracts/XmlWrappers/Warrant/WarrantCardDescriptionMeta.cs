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
	public class WarrantCardDescriptionMeta
	{
		private string idField;
		private string issuerField;
		private string linkField;

		[XmlAttribute]
		public string ID
		{
			get => this.idField;
			set => this.idField = value;
		}

		[XmlAttribute]
		public string Issuer
		{
			get => this.issuerField;
			set => this.issuerField = value;
		}

		[XmlAttribute]
		public string Link
		{
			get => this.linkField;
			set => this.linkField = value;
		}
	}
}
