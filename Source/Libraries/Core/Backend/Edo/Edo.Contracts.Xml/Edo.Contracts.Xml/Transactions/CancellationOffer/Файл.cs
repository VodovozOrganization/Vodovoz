using Edo.Contracts.Xml.Other;

namespace Edo.Contracts.Xml.Transactions.CancellationOffer
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class Файл
	{
		private ФайлДокумент документField;

		private string идФайлField;

		private string версПрогField;

		private ФайлВерсФорм версФормField;

		/// <remarks/>
		public ФайлДокумент Документ
		{
			get { return this.документField; }
			set { this.документField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ИдФайл
		{
			get { return this.идФайлField; }
			set { this.идФайлField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ВерсПрог
		{
			get { return this.версПрогField; }
			set { this.версПрогField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public ФайлВерсФорм ВерсФорм
		{
			get { return this.версФормField; }
			set { this.версФормField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class ФайлДокумент
	{
		private УчастЭДОТип участЭДОField;

		private ФайлДокументСвПредАн свПредАнField;

		private УчастЭДОТип напрПредАнField;

		private ПодписантТип подписантField;

		/// <remarks/>
		public УчастЭДОТип УчастЭДО
		{
			get { return this.участЭДОField; }
			set { this.участЭДОField = value; }
		}

		/// <remarks/>
		public ФайлДокументСвПредАн СвПредАн
		{
			get { return this.свПредАнField; }
			set { this.свПредАнField = value; }
		}

		/// <remarks/>
		public УчастЭДОТип НапрПредАн
		{
			get { return this.напрПредАнField; }
			set { this.напрПредАнField = value; }
		}

		/// <remarks/>
		public ПодписантТип Подписант
		{
			get { return this.подписантField; }
			set { this.подписантField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class УчастЭДОТип
	{

		private object itemField;

		private string идУчастЭДОField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("ИП", typeof(ФЛТип))]
		[System.Xml.Serialization.XmlElementAttribute("ЮЛ", typeof(ЮЛТип))]
		public object Item
		{
			get { return this.itemField; }
			set { this.itemField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ИдУчастЭДО
		{
			get { return this.идУчастЭДОField; }
			set { this.идУчастЭДОField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class ФЛТип
	{

		private FullName фИОField;

		private string иННФЛField;

		/// <remarks/>
		public FullName ФИО
		{
			get { return this.фИОField; }
			set { this.фИОField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ИННФЛ
		{
			get { return this.иННФЛField; }
			set { this.иННФЛField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class ПодписантТип
	{

		private FullName фИОField;

		private string должностьField;

		/// <remarks/>
		public FullName ФИО
		{
			get { return this.фИОField; }
			set { this.фИОField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Должность
		{
			get { return this.должностьField; }
			set { this.должностьField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class ЮЛТип
	{

		private string наимОргField;

		private string иННЮЛField;

		private string кППField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string НаимОрг
		{
			get { return this.наимОргField; }
			set { this.наимОргField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ИННЮЛ
		{
			get { return this.иННЮЛField; }
			set { this.иННЮЛField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string КПП
		{
			get { return this.кППField; }
			set { this.кППField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class ФайлДокументСвПредАн
	{

		private ФайлДокументСвПредАнСведАнФайл сведАнФайлField;

		private string текстПредАнField;

		/// <remarks/>
		public ФайлДокументСвПредАнСведАнФайл СведАнФайл
		{
			get { return this.сведАнФайлField; }
			set { this.сведАнФайлField = value; }
		}

		/// <remarks/>
		public string ТекстПредАн
		{
			get { return this.текстПредАнField; }
			set { this.текстПредАнField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public partial class ФайлДокументСвПредАнСведАнФайл
	{

		private string[] эЦПАнФайлField;

		private string имяАнФайлаField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("ЭЦПАнФайл")]
		public string[] ЭЦПАнФайл
		{
			get { return this.эЦПАнФайлField; }
			set { this.эЦПАнФайлField = value; }
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ИмяАнФайла
		{
			get { return this.имяАнФайлаField; }
			set { this.имяАнФайлаField = value; }
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	public enum ФайлВерсФорм
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlEnumAttribute("1.02")]
		Item102,
	}
}
