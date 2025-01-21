namespace TaxcomEdo.Contracts.XmlWrappers
{
	public class ReceiveNotice
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

			private ФайлДокументУчастЭДО участЭДОField;

			private ФайлДокументСвИзвПолуч свИзвПолучField;

			private ФайлДокументОтпрДок отпрДокField;

			private ФайлДокументПодписант подписантField;

			private ФайлДокументКНД кНДField;

			/// <remarks/>
			public ФайлДокументУчастЭДО УчастЭДО
			{
				get { return this.участЭДОField; }
				set { this.участЭДОField = value; }
			}

			/// <remarks/>
			public ФайлДокументСвИзвПолуч СвИзвПолуч
			{
				get { return this.свИзвПолучField; }
				set { this.свИзвПолучField = value; }
			}

			/// <remarks/>
			public ФайлДокументОтпрДок ОтпрДок
			{
				get { return this.отпрДокField; }
				set { this.отпрДокField = value; }
			}

			/// <remarks/>
			public ФайлДокументПодписант Подписант
			{
				get { return this.подписантField; }
				set { this.подписантField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументКНД КНД
			{
				get { return this.кНДField; }
				set { this.кНДField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументУчастЭДО
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

			private ФИОТип фИОField;

			private string иННФЛField;

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
		public partial class ФайлДокументСвИзвПолуч
		{

			private ФайлДокументСвИзвПолучСведПолФайл сведПолФайлField;

			private string датаПолField;

			private string времяПолField;

			/// <remarks/>
			public ФайлДокументСвИзвПолучСведПолФайл СведПолФайл
			{
				get { return this.сведПолФайлField; }
				set { this.сведПолФайлField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаПол
			{
				get { return this.датаПолField; }
				set { this.датаПолField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ВремяПол
			{
				get { return this.времяПолField; }
				set { this.времяПолField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвИзвПолучСведПолФайл
		{

			private string[] эЦППолФайлField;

			private ФайлДокументСвИзвПолучСведПолФайлДанПолучДок данПолучДокField;

			private string имяПостФайлаField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ЭЦППолФайл")]
			public string[] ЭЦППолФайл
			{
				get { return this.эЦППолФайлField; }
				set { this.эЦППолФайлField = value; }
			}

			/// <remarks/>
			public ФайлДокументСвИзвПолучСведПолФайлДанПолучДок ДанПолучДок
			{
				get { return this.данПолучДокField; }
				set { this.данПолучДокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИмяПостФайла
			{
				get { return this.имяПостФайлаField; }
				set { this.имяПостФайлаField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвИзвПолучСведПолФайлДанПолучДок
		{

			private string наимДокField;

			private string номДокField;

			private string датаДокField;

			private string номИспрСФField;

			private string датаИспрСФField;

			private string номКСФField;

			private string датаКСФField;

			private string номИспрКСФField;

			private string датаИспрКСФField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимДок
			{
				get { return this.наимДокField; }
				set { this.наимДокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомДок
			{
				get { return this.номДокField; }
				set { this.номДокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаДок
			{
				get { return this.датаДокField; }
				set { this.датаДокField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомИспрСФ
			{
				get { return this.номИспрСФField; }
				set { this.номИспрСФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаИспрСФ
			{
				get { return this.датаИспрСФField; }
				set { this.датаИспрСФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомКСФ
			{
				get { return this.номКСФField; }
				set { this.номКСФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаКСФ
			{
				get { return this.датаКСФField; }
				set { this.датаКСФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомИспрКСФ
			{
				get { return this.номИспрКСФField; }
				set { this.номИспрКСФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаИспрКСФ
			{
				get { return this.датаИспрКСФField; }
				set { this.датаИспрКСФField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументОтпрДок
		{

			private object itemField;

			private string идУчастЭДОField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ИП", typeof(ФЛТип))]
			[System.Xml.Serialization.XmlElementAttribute("ОперЭДО", typeof(ФайлДокументОтпрДокОперЭДО))]
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
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументОтпрДокОперЭДО
		{

			private string наимОргField;

			private string иННЮЛField;

			private string идОперЭДОField;

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
			public string ИдОперЭДО
			{
				get { return this.идОперЭДОField; }
				set { this.идОперЭДОField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументПодписант
		{

			private ФИОТип фИОField;

			private string должностьField;

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
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументКНД
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("1115110")]
			Item1115110,
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
}
