namespace TaxcomEdo.Contracts.XmlWrappers.Transactions
{
	public class CustomerInformationXml
	{
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
		public partial class Файл
		{

			private ФайлСвУчДокОбор свУчДокОборField;

			private ФайлИнфПок инфПокField;

			private string идФайлField;

			private ФайлВерсФорм версФормField;

			private string версПрогField;

			/// <remarks/>
			public ФайлСвУчДокОбор СвУчДокОбор
			{
				get { return this.свУчДокОборField; }
				set { this.свУчДокОборField = value; }
			}

			/// <remarks/>
			public ФайлИнфПок ИнфПок
			{
				get { return this.инфПокField; }
				set { this.инфПокField = value; }
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
			public ФайлВерсФорм ВерсФорм
			{
				get { return this.версФормField; }
				set { this.версФормField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ВерсПрог
			{
				get { return this.версПрогField; }
				set { this.версПрогField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлСвУчДокОбор
		{

			private ФайлСвУчДокОборСвОЭДОтпр свОЭДОтпрField;

			private string идОтпрField;

			private string идПолField;

			/// <remarks/>
			public ФайлСвУчДокОборСвОЭДОтпр СвОЭДОтпр
			{
				get { return this.свОЭДОтпрField; }
				set { this.свОЭДОтпрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИдОтпр
			{
				get { return this.идОтпрField; }
				set { this.идОтпрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИдПол
			{
				get { return this.идПолField; }
				set { this.идПолField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлСвУчДокОборСвОЭДОтпр
		{

			private string наимОргField;

			private string иННЮЛField;

			private string идЭДОField;

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
			public string ИдЭДО
			{
				get { return this.идЭДОField; }
				set { this.идЭДОField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		public partial class СвИПТип
		{

			private ФИОТип фИОField;

			private string иННФЛField;

			private string свГосРегИПField;

			private string иныеСведField;

			/// <remarks/>
			public ФИОТип ФИО
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

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string СвГосРегИП
			{
				get { return this.свГосРегИПField; }
				set { this.свГосРегИПField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИныеСвед
			{
				get { return this.иныеСведField; }
				set { this.иныеСведField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		public partial class ФИОТип
		{

			private string фамилияField;

			private string имяField;

			private string отчествоField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Фамилия
			{
				get { return this.фамилияField; }
				set { this.фамилияField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Имя
			{
				get { return this.имяField; }
				set { this.имяField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Отчество
			{
				get { return this.отчествоField; }
				set { this.отчествоField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		public partial class СвФЛТип
		{

			private ФИОТип фИОField;

			private string иННФЛField;

			private string иныеСведField;

			/// <remarks/>
			public ФИОТип ФИО
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

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИныеСвед
			{
				get { return this.иныеСведField; }
				set { this.иныеСведField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПок
		{

			private ФайлИнфПокИдИнфПрод идИнфПродField;

			private ФайлИнфПокСодФХЖ4 содФХЖ4Field;

			private ФайлИнфПокИнфПокГосЗакКазн инфПокГосЗакКазнField;

			private ФайлИнфПокПодписант[] подписантField;

			private ФайлИнфПокКНД кНДField;

			private string датаИнфПокField;

			private string времИнфПокField;

			private string наимЭконСубСостField;

			private string оснДоверОргСостField;

			/// <remarks/>
			public ФайлИнфПокИдИнфПрод ИдИнфПрод
			{
				get { return this.идИнфПродField; }
				set { this.идИнфПродField = value; }
			}

			/// <remarks/>
			public ФайлИнфПокСодФХЖ4 СодФХЖ4
			{
				get { return this.содФХЖ4Field; }
				set { this.содФХЖ4Field = value; }
			}

			/// <remarks/>
			public ФайлИнфПокИнфПокГосЗакКазн ИнфПокГосЗакКазн
			{
				get { return this.инфПокГосЗакКазнField; }
				set { this.инфПокГосЗакКазнField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("Подписант")]
			public ФайлИнфПокПодписант[] Подписант
			{
				get { return this.подписантField; }
				set { this.подписантField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлИнфПокКНД КНД
			{
				get { return this.кНДField; }
				set { this.кНДField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаИнфПок
			{
				get { return this.датаИнфПокField; }
				set { this.датаИнфПокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ВремИнфПок
			{
				get { return this.времИнфПокField; }
				set { this.времИнфПокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимЭконСубСост
			{
				get { return this.наимЭконСубСостField; }
				set { this.наимЭконСубСостField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОснДоверОргСост
			{
				get { return this.оснДоверОргСостField; }
				set { this.оснДоверОргСостField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокИдИнфПрод
		{

			private string[] эпField;

			private string идФайлИнфПрField;

			private string датаФайлИнфПрField;

			private string времФайлИнфПрField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ЭП")]
			public string[] ЭП
			{
				get { return this.эпField; }
				set { this.эпField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИдФайлИнфПр
			{
				get { return this.идФайлИнфПрField; }
				set { this.идФайлИнфПрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаФайлИнфПр
			{
				get { return this.датаФайлИнфПрField; }
				set { this.датаФайлИнфПрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ВремФайлИнфПр
			{
				get { return this.времФайлИнфПрField; }
				set { this.времФайлИнфПрField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокСодФХЖ4
		{

			private ФайлИнфПокСодФХЖ4СвПрин свПринField;

			private ФайлИнфПокСодФХЖ4ИнфПолФХЖ4 инфПолФХЖ4Field;

			private string наимДокОпрПрField;

			private string функцияField;

			private string номСчФИнфПрField;

			private string датаСчФИнфПрField;

			private string видОперацииField;

			/// <remarks/>
			public ФайлИнфПокСодФХЖ4СвПрин СвПрин
			{
				get { return this.свПринField; }
				set { this.свПринField = value; }
			}

			/// <remarks/>
			public ФайлИнфПокСодФХЖ4ИнфПолФХЖ4 ИнфПолФХЖ4
			{
				get { return this.инфПолФХЖ4Field; }
				set { this.инфПолФХЖ4Field = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимДокОпрПр
			{
				get { return this.наимДокОпрПрField; }
				set { this.наимДокОпрПрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Функция
			{
				get { return this.функцияField; }
				set { this.функцияField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомСчФИнфПр
			{
				get { return this.номСчФИнфПрField; }
				set { this.номСчФИнфПрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаСчФИнфПр
			{
				get { return this.датаСчФИнфПрField; }
				set { this.датаСчФИнфПрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ВидОперации
			{
				get { return this.видОперацииField; }
				set { this.видОперацииField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокСодФХЖ4СвПрин
		{

			private ФайлИнфПокСодФХЖ4СвПринКодСодОпер кодСодОперField;

			private ФайлИнфПокСодФХЖ4СвПринСвЛицПрин свЛицПринField;

			private string содОперField;

			private string датаПринField;

			/// <remarks/>
			public ФайлИнфПокСодФХЖ4СвПринКодСодОпер КодСодОпер
			{
				get { return this.кодСодОперField; }
				set { this.кодСодОперField = value; }
			}

			/// <remarks/>
			public ФайлИнфПокСодФХЖ4СвПринСвЛицПрин СвЛицПрин
			{
				get { return this.свЛицПринField; }
				set { this.свЛицПринField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string СодОпер
			{
				get { return this.содОперField; }
				set { this.содОперField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаПрин
			{
				get { return this.датаПринField; }
				set { this.датаПринField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокСодФХЖ4СвПринКодСодОпер
		{

			private ФайлИнфПокСодФХЖ4СвПринКодСодОперКодИтога кодИтогаField;

			private string наимДокРасхField;

			private ФайлИнфПокСодФХЖ4СвПринКодСодОперВидДокРасх видДокРасхField;

			private bool видДокРасхFieldSpecified;

			private string номДокРасхField;

			private string датаДокРасхField;

			private string идФайлДокРасхField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлИнфПокСодФХЖ4СвПринКодСодОперКодИтога КодИтога
			{
				get { return this.кодИтогаField; }
				set { this.кодИтогаField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимДокРасх
			{
				get { return this.наимДокРасхField; }
				set { this.наимДокРасхField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлИнфПокСодФХЖ4СвПринКодСодОперВидДокРасх ВидДокРасх
			{
				get { return this.видДокРасхField; }
				set { this.видДокРасхField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ВидДокРасхSpecified
			{
				get { return this.видДокРасхFieldSpecified; }
				set { this.видДокРасхFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомДокРасх
			{
				get { return this.номДокРасхField; }
				set { this.номДокРасхField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаДокРасх
			{
				get { return this.датаДокРасхField; }
				set { this.датаДокРасхField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИдФайлДокРасх
			{
				get { return this.идФайлДокРасхField; }
				set { this.идФайлДокРасхField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлИнфПокСодФХЖ4СвПринКодСодОперКодИтога
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("1")]
			Item1,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("2")]
			Item2,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("3")]
			Item3,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлИнфПокСодФХЖ4СвПринКодСодОперВидДокРасх
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("2")]
			Item2,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("3")]
			Item3,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокСодФХЖ4СвПринСвЛицПрин
		{

			private object itemField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ИнЛицо", typeof(ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицо))]
			[System.Xml.Serialization.XmlElementAttribute("РабОргПок", typeof(ФайлИнфПокСодФХЖ4СвПринСвЛицПринРабОргПок))]
			public object Item
			{
				get { return this.itemField; }
				set { this.itemField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицо
		{

			private object itemField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ПредОргПрин", typeof(ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицоПредОргПрин))]
			[System.Xml.Serialization.XmlElementAttribute("ФЛПрин", typeof(ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицоФЛПрин))]
			public object Item
			{
				get { return this.itemField; }
				set { this.itemField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицоПредОргПрин
		{

			private ФИОТип фИОField;

			private string должностьField;

			private string иныеСведField;

			private string наимОргПринField;

			private string оснДоверОргПринField;

			private string оснПолнПредПринField;

			/// <remarks/>
			public ФИОТип ФИО
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

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИныеСвед
			{
				get { return this.иныеСведField; }
				set { this.иныеСведField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимОргПрин
			{
				get { return this.наимОргПринField; }
				set { this.наимОргПринField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОснДоверОргПрин
			{
				get { return this.оснДоверОргПринField; }
				set { this.оснДоверОргПринField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОснПолнПредПрин
			{
				get { return this.оснПолнПредПринField; }
				set { this.оснПолнПредПринField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокСодФХЖ4СвПринСвЛицПринИнЛицоФЛПрин
		{

			private ФИОТип фИОField;

			private string оснДоверФЛField;

			private string иныеСведField;

			/// <remarks/>
			public ФИОТип ФИО
			{
				get { return this.фИОField; }
				set { this.фИОField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОснДоверФЛ
			{
				get { return this.оснДоверФЛField; }
				set { this.оснДоверФЛField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИныеСвед
			{
				get { return this.иныеСведField; }
				set { this.иныеСведField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокСодФХЖ4СвПринСвЛицПринРабОргПок
		{

			private ФИОТип фИОField;

			private string должностьField;

			private string иныеСведField;

			private string оснПолнField;

			/// <remarks/>
			public ФИОТип ФИО
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

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИныеСвед
			{
				get { return this.иныеСведField; }
				set { this.иныеСведField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОснПолн
			{
				get { return this.оснПолнField; }
				set { this.оснПолнField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокСодФХЖ4ИнфПолФХЖ4
		{

			private ФайлИнфПокСодФХЖ4ИнфПолФХЖ4ТекстИнф[] текстИнфField;

			private string идФайлИнфПолField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ТекстИнф")]
			public ФайлИнфПокСодФХЖ4ИнфПолФХЖ4ТекстИнф[] ТекстИнф
			{
				get { return this.текстИнфField; }
				set { this.текстИнфField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИдФайлИнфПол
			{
				get { return this.идФайлИнфПолField; }
				set { this.идФайлИнфПолField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокСодФХЖ4ИнфПолФХЖ4ТекстИнф
		{

			private string идентифField;

			private string значенField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Идентиф
			{
				get { return this.идентифField; }
				set { this.идентифField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Значен
			{
				get { return this.значенField; }
				set { this.значенField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокИнфПокГосЗакКазн
		{

			private ФайлИнфПокИнфПокГосЗакКазнИнфСведДенОбяз[] инфСведДенОбязField;

			private string идКодЗакField;

			private string лицСчетПокField;

			private string наимФинОргПокField;

			private string номРеестрЗапПокField;

			private string учНомБюдОбязПокField;

			private string кодКазначПокField;

			private string наимКазначПокField;

			private string оКТМОПокField;

			private string оКТМОМесПостField;

			private string датаОплПредField;

			private string учНомДенОбязField;

			private string очерПлатField;

			private ФайлИнфПокИнфПокГосЗакКазнВидПлат видПлатField;

			private bool видПлатFieldSpecified;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ИнфСведДенОбяз")]
			public ФайлИнфПокИнфПокГосЗакКазнИнфСведДенОбяз[] ИнфСведДенОбяз
			{
				get { return this.инфСведДенОбязField; }
				set { this.инфСведДенОбязField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИдКодЗак
			{
				get { return this.идКодЗакField; }
				set { this.идКодЗакField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ЛицСчетПок
			{
				get { return this.лицСчетПокField; }
				set { this.лицСчетПокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимФинОргПок
			{
				get { return this.наимФинОргПокField; }
				set { this.наимФинОргПокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомРеестрЗапПок
			{
				get { return this.номРеестрЗапПокField; }
				set { this.номРеестрЗапПокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string УчНомБюдОбязПок
			{
				get { return this.учНомБюдОбязПокField; }
				set { this.учНомБюдОбязПокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодКазначПок
			{
				get { return this.кодКазначПокField; }
				set { this.кодКазначПокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимКазначПок
			{
				get { return this.наимКазначПокField; }
				set { this.наимКазначПокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОКТМОПок
			{
				get { return this.оКТМОПокField; }
				set { this.оКТМОПокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОКТМОМесПост
			{
				get { return this.оКТМОМесПостField; }
				set { this.оКТМОМесПостField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаОплПред
			{
				get { return this.датаОплПредField; }
				set { this.датаОплПредField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string УчНомДенОбяз
			{
				get { return this.учНомДенОбязField; }
				set { this.учНомДенОбязField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОчерПлат
			{
				get { return this.очерПлатField; }
				set { this.очерПлатField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлИнфПокИнфПокГосЗакКазнВидПлат ВидПлат
			{
				get { return this.видПлатField; }
				set { this.видПлатField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ВидПлатSpecified
			{
				get { return this.видПлатFieldSpecified; }
				set { this.видПлатFieldSpecified = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокИнфПокГосЗакКазнИнфСведДенОбяз
		{

			private string номСтрField;

			private string кодОбъектФАИПField;

			private ФайлИнфПокИнфПокГосЗакКазнИнфСведДенОбязВидСредств видСредствField;

			private string кодПокБюджКлассField;

			private string кодЦелиПокупField;

			private decimal сумАвансField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
			public string НомСтр
			{
				get { return this.номСтрField; }
				set { this.номСтрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодОбъектФАИП
			{
				get { return this.кодОбъектФАИПField; }
				set { this.кодОбъектФАИПField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлИнфПокИнфПокГосЗакКазнИнфСведДенОбязВидСредств ВидСредств
			{
				get { return this.видСредствField; }
				set { this.видСредствField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодПокБюджКласс
			{
				get { return this.кодПокБюджКлассField; }
				set { this.кодПокБюджКлассField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодЦелиПокуп
			{
				get { return this.кодЦелиПокупField; }
				set { this.кодЦелиПокупField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal СумАванс
			{
				get { return this.сумАвансField; }
				set { this.сумАвансField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлИнфПокИнфПокГосЗакКазнИнфСведДенОбязВидСредств
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("1")]
			Item1,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("2")]
			Item2,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("3")]
			Item3,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("4")]
			Item4,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("5")]
			Item5,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("6")]
			Item6,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлИнфПокИнфПокГосЗакКазнВидПлат
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("0")]
			Item0,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("4")]
			Item4,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокПодписант
		{

			private object itemField;

			private ФайлИнфПокПодписантОблПолн облПолнField;

			private ФайлИнфПокПодписантСтатус статусField;

			private string оснПолнField;

			private string оснПолнОргField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ИП", typeof(СвИПТип))]
			[System.Xml.Serialization.XmlElementAttribute("ФЛ", typeof(СвФЛТип))]
			[System.Xml.Serialization.XmlElementAttribute("ЮЛ", typeof(ФайлИнфПокПодписантЮЛ))]
			public object Item
			{
				get { return this.itemField; }
				set { this.itemField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлИнфПокПодписантОблПолн ОблПолн
			{
				get { return this.облПолнField; }
				set { this.облПолнField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлИнфПокПодписантСтатус Статус
			{
				get { return this.статусField; }
				set { this.статусField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОснПолн
			{
				get { return this.оснПолнField; }
				set { this.оснПолнField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОснПолнОрг
			{
				get { return this.оснПолнОргField; }
				set { this.оснПолнОргField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлИнфПокПодписантЮЛ
		{

			private ФИОТип фИОField;

			private string иННЮЛField;

			private string наимОргField;

			private string должнField;

			private string иныеСведField;

			/// <remarks/>
			public ФИОТип ФИО
			{
				get { return this.фИОField; }
				set { this.фИОField = value; }
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
			public string НаимОрг
			{
				get { return this.наимОргField; }
				set { this.наимОргField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Должн
			{
				get { return this.должнField; }
				set { this.должнField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИныеСвед
			{
				get { return this.иныеСведField; }
				set { this.иныеСведField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлИнфПокПодписантОблПолн
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("1")]
			Item1,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("2")]
			Item2,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("3")]
			Item3,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлИнфПокПодписантСтатус
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("3")]
			Item3,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("4")]
			Item4,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("5")]
			Item5,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("6")]
			Item6,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлИнфПокКНД
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("1115132")]
			Item1115132,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлВерсФорм
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("5.01")]
			Item501,
		}
	}
}
