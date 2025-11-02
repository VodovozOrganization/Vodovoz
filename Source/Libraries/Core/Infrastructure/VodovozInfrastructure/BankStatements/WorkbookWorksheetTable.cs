namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:spreadsheet")]
	public partial class WorkbookWorksheetTable
	{
		private WorkbookWorksheetTableColumn[] columnField;

		private WorkbookWorksheetTableRow[] rowField;

		private byte expandedColumnCountField;

		private uint expandedRowCountField;

		private byte fullColumnsField;

		private byte fullRowsField;

		private decimal defaultRowHeightField;

		private bool defaultRowHeightFieldSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Column")]
		public WorkbookWorksheetTableColumn[] Column
		{
			get
			{
				return this.columnField;
			}
			set
			{
				this.columnField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Row")]
		public WorkbookWorksheetTableRow[] Row
		{
			get
			{
				return this.rowField;
			}
			set
			{
				this.rowField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public byte ExpandedColumnCount
		{
			get
			{
				return this.expandedColumnCountField;
			}
			set
			{
				this.expandedColumnCountField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public uint ExpandedRowCount
		{
			get
			{
				return this.expandedRowCountField;
			}
			set
			{
				this.expandedRowCountField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "urn:schemas-microsoft-com:office:excel")]
		public byte FullColumns
		{
			get
			{
				return this.fullColumnsField;
			}
			set
			{
				this.fullColumnsField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "urn:schemas-microsoft-com:office:excel")]
		public byte FullRows
		{
			get
			{
				return this.fullRowsField;
			}
			set
			{
				this.fullRowsField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
		public decimal DefaultRowHeight
		{
			get
			{
				return this.defaultRowHeightField;
			}
			set
			{
				this.defaultRowHeightField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool DefaultRowHeightSpecified
		{
			get
			{
				return this.defaultRowHeightFieldSpecified;
			}
			set
			{
				this.defaultRowHeightFieldSpecified = value;
			}
		}
	}
}
