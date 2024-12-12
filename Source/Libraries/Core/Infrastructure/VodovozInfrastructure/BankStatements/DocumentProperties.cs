namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:office")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:schemas-microsoft-com:office:office", IsNullable = false)]
	public partial class DocumentProperties
	{
		private string authorField;

		private string lastAuthorField;

		private System.DateTime createdField;

		private System.DateTime lastSavedField;

		private string companyField;

		private decimal versionField;

		/// <remarks/>
		public string Author
		{
			get
			{
				return this.authorField;
			}
			set
			{
				this.authorField = value;
			}
		}

		/// <remarks/>
		public string LastAuthor
		{
			get
			{
				return this.lastAuthorField;
			}
			set
			{
				this.lastAuthorField = value;
			}
		}

		/// <remarks/>
		public System.DateTime Created
		{
			get
			{
				return this.createdField;
			}
			set
			{
				this.createdField = value;
			}
		}

		/// <remarks/>
		public System.DateTime LastSaved
		{
			get
			{
				return this.lastSavedField;
			}
			set
			{
				this.lastSavedField = value;
			}
		}

		/// <remarks/>
		public string Company
		{
			get
			{
				return this.companyField;
			}
			set
			{
				this.companyField = value;
			}
		}

		/// <remarks/>
		public decimal Version
		{
			get
			{
				return this.versionField;
			}
			set
			{
				this.versionField = value;
			}
		}
	}
}
