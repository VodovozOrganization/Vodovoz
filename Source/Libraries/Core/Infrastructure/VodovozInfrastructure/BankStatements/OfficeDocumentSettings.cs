namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:office")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:schemas-microsoft-com:office:office", IsNullable = false)]
	public partial class OfficeDocumentSettings
	{
		private object allowPNGField;

		/// <remarks/>
		public object AllowPNG
		{
			get
			{
				return this.allowPNGField;
			}
			set
			{
				this.allowPNGField = value;
			}
		}
	}
}
