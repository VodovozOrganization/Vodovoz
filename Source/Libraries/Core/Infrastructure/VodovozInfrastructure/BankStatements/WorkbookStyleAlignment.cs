namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:spreadsheet")]
	public partial class WorkbookStyleAlignment
	{
		private string verticalField;

		private string horizontalField;

		private byte wrapTextField;

		private bool wrapTextFieldSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public string Vertical
		{
			get
			{
				return this.verticalField;
			}
			set
			{
				this.verticalField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public string Horizontal
		{
			get
			{
				return this.horizontalField;
			}
			set
			{
				this.horizontalField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public byte WrapText
		{
			get
			{
				return this.wrapTextField;
			}
			set
			{
				this.wrapTextField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool WrapTextSpecified
		{
			get
			{
				return this.wrapTextFieldSpecified;
			}
			set
			{
				this.wrapTextFieldSpecified = value;
			}
		}
	}
}
