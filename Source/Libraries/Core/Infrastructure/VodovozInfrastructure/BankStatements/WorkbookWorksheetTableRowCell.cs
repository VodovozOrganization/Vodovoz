namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:spreadsheet")]
	public partial class WorkbookWorksheetTableRowCell
	{
		private WorkbookWorksheetTableRowCellData dataField;

		private string styleIDField;

		/// <remarks/>
		public WorkbookWorksheetTableRowCellData Data
		{
			get
			{
				return this.dataField;
			}
			set
			{
				this.dataField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public string StyleID
		{
			get
			{
				return this.styleIDField;
			}
			set
			{
				this.styleIDField = value;
			}
		}
	}
}
