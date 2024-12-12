namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:excel")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:schemas-microsoft-com:office:excel", IsNullable = false)]
	public partial class WorksheetOptions
	{
		private WorksheetOptionsPageSetup pageSetupField;

		private object unsyncedField;

		private WorksheetOptionsPrint printField;

		private object selectedField;

		private string protectObjectsField;

		private string protectScenariosField;

		/// <remarks/>
		public WorksheetOptionsPageSetup PageSetup
		{
			get
			{
				return this.pageSetupField;
			}
			set
			{
				this.pageSetupField = value;
			}
		}

		/// <remarks/>
		public object Unsynced
		{
			get
			{
				return this.unsyncedField;
			}
			set
			{
				this.unsyncedField = value;
			}
		}

		/// <remarks/>
		public WorksheetOptionsPrint Print
		{
			get
			{
				return this.printField;
			}
			set
			{
				this.printField = value;
			}
		}

		/// <remarks/>
		public object Selected
		{
			get
			{
				return this.selectedField;
			}
			set
			{
				this.selectedField = value;
			}
		}

		/// <remarks/>
		public string ProtectObjects
		{
			get
			{
				return this.protectObjectsField;
			}
			set
			{
				this.protectObjectsField = value;
			}
		}

		/// <remarks/>
		public string ProtectScenarios
		{
			get
			{
				return this.protectScenariosField;
			}
			set
			{
				this.protectScenariosField = value;
			}
		}
	}
}
