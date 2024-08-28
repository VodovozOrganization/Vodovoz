namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:spreadsheet")]
	public partial class WorkbookWorksheetTableRow
	{
		private WorkbookWorksheetTableRowCell[] cellField;

		private byte autoFitHeightField;

		private decimal heightField;

		private bool heightFieldSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Cell")]
		public WorkbookWorksheetTableRowCell[] Cell
		{
			get
			{
				return this.cellField;
			}
			set
			{
				this.cellField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public byte AutoFitHeight
		{
			get
			{
				return this.autoFitHeightField;
			}
			set
			{
				this.autoFitHeightField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public decimal Height
		{
			get
			{
				return this.heightField;
			}
			set
			{
				this.heightField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool HeightSpecified
		{
			get
			{
				return this.heightFieldSpecified;
			}
			set
			{
				this.heightFieldSpecified = value;
			}
		}
	}
}
