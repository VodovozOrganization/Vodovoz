namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:spreadsheet")]
	public partial class WorkbookStyleFont
	{
		private string fontNameField;

		private byte charSetField;

		private string familyField;

		private byte sizeField;

		private bool sizeFieldSpecified;

		private byte boldField;

		private bool boldFieldSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public string FontName
		{
			get
			{
				return this.fontNameField;
			}
			set
			{
				this.fontNameField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "urn:schemas-microsoft-com:office:excel")]
		public byte CharSet
		{
			get
			{
				return this.charSetField;
			}
			set
			{
				this.charSetField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "urn:schemas-microsoft-com:office:excel")]
		public string Family
		{
			get
			{
				return this.familyField;
			}
			set
			{
				this.familyField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public byte Size
		{
			get
			{
				return this.sizeField;
			}
			set
			{
				this.sizeField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool SizeSpecified
		{
			get
			{
				return this.sizeFieldSpecified;
			}
			set
			{
				this.sizeFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public byte Bold
		{
			get
			{
				return this.boldField;
			}
			set
			{
				this.boldField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool BoldSpecified
		{
			get
			{
				return this.boldFieldSpecified;
			}
			set
			{
				this.boldFieldSpecified = value;
			}
		}
	}
}
