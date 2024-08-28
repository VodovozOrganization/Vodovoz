namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:spreadsheet")]
	public partial class WorkbookStyle
	{
		private WorkbookStyleAlignment alignmentField;

		private WorkbookStyleBorder[] bordersField;

		private WorkbookStyleFont fontField;

		private object interiorField;

		private WorkbookStyleNumberFormat numberFormatField;

		private object protectionField;

		private string idField;

		private string nameField;

		/// <remarks/>
		public WorkbookStyleAlignment Alignment
		{
			get
			{
				return this.alignmentField;
			}
			set
			{
				this.alignmentField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("Border", IsNullable = false)]
		public WorkbookStyleBorder[] Borders
		{
			get
			{
				return this.bordersField;
			}
			set
			{
				this.bordersField = value;
			}
		}

		/// <remarks/>
		public WorkbookStyleFont Font
		{
			get
			{
				return this.fontField;
			}
			set
			{
				this.fontField = value;
			}
		}

		/// <remarks/>
		public object Interior
		{
			get
			{
				return this.interiorField;
			}
			set
			{
				this.interiorField = value;
			}
		}

		/// <remarks/>
		public WorkbookStyleNumberFormat NumberFormat
		{
			get
			{
				return this.numberFormatField;
			}
			set
			{
				this.numberFormatField = value;
			}
		}

		/// <remarks/>
		public object Protection
		{
			get
			{
				return this.protectionField;
			}
			set
			{
				this.protectionField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public string ID
		{
			get
			{
				return this.idField;
			}
			set
			{
				this.idField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public string Name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}
	}
}
