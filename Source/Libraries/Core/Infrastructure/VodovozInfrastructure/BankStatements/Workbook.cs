namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:spreadsheet")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:schemas-microsoft-com:office:spreadsheet", IsNullable = false)]
	public partial class Workbook
	{
		private DocumentProperties documentPropertiesField;

		private OfficeDocumentSettings officeDocumentSettingsField;

		private ExcelWorkbook excelWorkbookField;

		private WorkbookStyle[] stylesField;

		private WorkbookWorksheet[] worksheetField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute(Namespace = "urn:schemas-microsoft-com:office:office")]
		public DocumentProperties DocumentProperties
		{
			get
			{
				return this.documentPropertiesField;
			}
			set
			{
				this.documentPropertiesField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute(Namespace = "urn:schemas-microsoft-com:office:office")]
		public OfficeDocumentSettings OfficeDocumentSettings
		{
			get
			{
				return this.officeDocumentSettingsField;
			}
			set
			{
				this.officeDocumentSettingsField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute(Namespace = "urn:schemas-microsoft-com:office:excel")]
		public ExcelWorkbook ExcelWorkbook
		{
			get
			{
				return this.excelWorkbookField;
			}
			set
			{
				this.excelWorkbookField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("Style", IsNullable = false)]
		public WorkbookStyle[] Styles
		{
			get
			{
				return this.stylesField;
			}
			set
			{
				this.stylesField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Worksheet")]
		public WorkbookWorksheet[] Worksheet
		{
			get
			{
				return this.worksheetField;
			}
			set
			{
				this.worksheetField = value;
			}
		}
	}
}
