namespace VodovozInfrastructure.BankStatements
{
	/// <remarks/>
	[System.SerializableAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:office:excel")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:schemas-microsoft-com:office:excel", IsNullable = false)]
	public partial class ExcelWorkbook
	{
		private ushort windowHeightField;

		private ushort windowWidthField;

		private byte windowTopXField;

		private byte windowTopYField;

		private object refModeR1C1Field;

		private string protectStructureField;

		private string protectWindowsField;

		/// <remarks/>
		public ushort WindowHeight
		{
			get
			{
				return this.windowHeightField;
			}
			set
			{
				this.windowHeightField = value;
			}
		}

		/// <remarks/>
		public ushort WindowWidth
		{
			get
			{
				return this.windowWidthField;
			}
			set
			{
				this.windowWidthField = value;
			}
		}

		/// <remarks/>
		public byte WindowTopX
		{
			get
			{
				return this.windowTopXField;
			}
			set
			{
				this.windowTopXField = value;
			}
		}

		/// <remarks/>
		public byte WindowTopY
		{
			get
			{
				return this.windowTopYField;
			}
			set
			{
				this.windowTopYField = value;
			}
		}

		/// <remarks/>
		public object RefModeR1C1
		{
			get
			{
				return this.refModeR1C1Field;
			}
			set
			{
				this.refModeR1C1Field = value;
			}
		}

		/// <remarks/>
		public string ProtectStructure
		{
			get
			{
				return this.protectStructureField;
			}
			set
			{
				this.protectStructureField = value;
			}
		}

		/// <remarks/>
		public string ProtectWindows
		{
			get
			{
				return this.protectWindowsField;
			}
			set
			{
				this.protectWindowsField = value;
			}
		}
	}
}
