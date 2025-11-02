namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:spreadsheet")]
	public partial class WorkbookWorksheet
	{
		private WorkbookWorksheetTable tableField;

		private WorksheetOptions worksheetOptionsField;

		private string nameField;

		/// <remarks/>
		public WorkbookWorksheetTable Table
		{
			get
			{
				return this.tableField;
			}
			set
			{
				this.tableField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute(Namespace = "urn:schemas-microsoft-com:office:excel")]
		public WorksheetOptions WorksheetOptions
		{
			get
			{
				return this.worksheetOptionsField;
			}
			set
			{
				this.worksheetOptionsField = value;
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
