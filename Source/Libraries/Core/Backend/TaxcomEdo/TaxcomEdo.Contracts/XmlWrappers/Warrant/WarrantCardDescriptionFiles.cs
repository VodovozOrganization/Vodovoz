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
	public class WarrantCardDescriptionFiles
	{
		private WarrantCardDescriptionFilesWarrantImage warrantImageField;
		private WarrantCardDescriptionFilesWarrantSignature[] warrantSignatureField;

		public WarrantCardDescriptionFilesWarrantImage WarrantImage
		{
			get => this.warrantImageField;
			set => this.warrantImageField = value;
		}

		[XmlElement("WarrantSignature")]
		public WarrantCardDescriptionFilesWarrantSignature[] WarrantSignature
		{
			get => this.warrantSignatureField;
			set => this.warrantSignatureField = value;
		}
	}
}
