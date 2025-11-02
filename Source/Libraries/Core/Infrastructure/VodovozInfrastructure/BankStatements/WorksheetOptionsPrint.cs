namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:excel")]
	public partial class WorksheetOptionsPrint
	{
		private object validPrinterInfoField;

		private byte paperSizeIndexField;

		private ushort horizontalResolutionField;

		private ushort verticalResolutionField;

		/// <remarks/>
		public object ValidPrinterInfo
		{
			get { return this.validPrinterInfoField; }
			set { this.validPrinterInfoField = value; }
		}

		/// <remarks/>
		public byte PaperSizeIndex
		{
			get { return this.paperSizeIndexField; }
			set { this.paperSizeIndexField = value; }
		}

		/// <remarks/>
		public ushort HorizontalResolution
		{
			get { return this.horizontalResolutionField; }
			set { this.horizontalResolutionField = value; }
		}

		/// <remarks/>
		public ushort VerticalResolution
		{
			get { return this.verticalResolutionField; }
			set { this.verticalResolutionField = value; }
		}
	}
}
