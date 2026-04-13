namespace TaxcomEdo.Contracts.Xml.Container.Entities.Card
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://api-invoice.taxcom.ru/card")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://api-invoice.taxcom.ru/card", IsNullable = false)]
	public class Card : Definition
	{
		public const string FileName = "card.xml";
		
		/// <remarks/>
		public Description Description { get; set; }

		/// <remarks/>
		public Participant Sender { get; set; }

		/// <remarks/>
		public Participant Receiver { get; set; }

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
		public Signer[] Signers { get; set; }
		
		public void SetDocumentTypeName(DefinitionTypeName value, bool resignRequired)
		{
			SetDocumentTypeName(value);
			Type.ResignRequired = resignRequired;
			Type.ResignRequiredSpecified = true;
		}

		private void SetDocumentTypeName(DefinitionTypeName value)
		{
			if(Type == null)
			{
				Type = new DefinitionType();
			}

			Type.Name = value;
			Type.NameSpecified = true;
		}
	}
}
