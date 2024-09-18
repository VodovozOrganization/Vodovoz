namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:excel")]
	public partial class WorksheetOptionsPageSetup
	{
		private WorksheetOptionsPageSetupPageMargins pageMarginsField;

		/// <remarks/>
		public WorksheetOptionsPageSetupPageMargins PageMargins
		{
			get
			{
				return this.pageMarginsField;
			}
			set
			{
				this.pageMarginsField = value;
			}
		}
	}
}
