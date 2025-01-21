namespace TaxcomEdo.Contracts.XmlWrappers
{
	public class UniversalInvoiceDocumentXml
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

			private ФайлСвУчДокОбор свУчДокОборField;

			private ФайлДокумент документField;

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
		public partial class СумНДСТип
		{

			private object itemField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("БезНДС", typeof(СумНДСТипБезНДС))]
			[System.Xml.Serialization.XmlElementAttribute("ДефНДС", typeof(СумНДСТипДефНДС))]
			[System.Xml.Serialization.XmlElementAttribute("СумНал", typeof(decimal))]
			public object Item
			{
				get { return this.itemField; }
				set { this.itemField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum СумНДСТипБезНДС
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("без НДС")]
			безНДС,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum СумНДСТипДефНДС
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("-")]
			Item,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		public partial class СумАкцизТип
		{

			private object itemField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("БезАкциз", typeof(СумАкцизТипБезАкциз))]
			[System.Xml.Serialization.XmlElementAttribute("СумАкциз", typeof(decimal))]
			public object Item
			{
				get { return this.itemField; }
				set { this.itemField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum СумАкцизТипБезАкциз
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("без акциза")]
			безакциза,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		public partial class ТекстИнфТип
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
		public partial class ОснованиеТип
		{

			private string наимОснField;

			private string номОснField;

			private string датаОснField;

			private string допСвОснField;

			private string идентОснField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимОсн
			{
				get { return this.наимОснField; }
				set { this.наимОснField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомОсн
			{
				get { return this.номОснField; }
				set { this.номОснField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаОсн
			{
				get { return this.датаОснField; }
				set { this.датаОснField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДопСвОсн
			{
				get { return this.допСвОснField; }
				set { this.допСвОснField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИдентОсн
			{
				get { return this.идентОснField; }
				set { this.идентОснField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		public partial class КонтактТип
		{

			private string тлфField;

			private string элПочтаField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Тлф
			{
				get { return this.тлфField; }
				set { this.тлфField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ЭлПочта
			{
				get { return this.элПочтаField; }
				set { this.элПочтаField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		public partial class АдрИнфТип
		{

			private string кодСтрField;

			private string адрТекстField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодСтр
			{
				get { return this.кодСтрField; }
				set { this.кодСтрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string АдрТекст
			{
				get { return this.адрТекстField; }
				set { this.адрТекстField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		public partial class АдрРФТип
		{

			private string индексField;

			private string кодРегионField;

			private string районField;

			private string городField;

			private string населПунктField;

			private string улицаField;

			private string домField;

			private string корпусField;

			private string квартField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Индекс
			{
				get { return this.индексField; }
				set { this.индексField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодРегион
			{
				get { return this.кодРегионField; }
				set { this.кодРегионField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Район
			{
				get { return this.районField; }
				set { this.районField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Город
			{
				get { return this.городField; }
				set { this.городField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаселПункт
			{
				get { return this.населПунктField; }
				set { this.населПунктField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Улица
			{
				get { return this.улицаField; }
				set { this.улицаField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Дом
			{
				get { return this.домField; }
				set { this.домField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Корпус
			{
				get { return this.корпусField; }
				set { this.корпусField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Кварт
			{
				get { return this.квартField; }
				set { this.квартField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		public partial class АдресТип
		{

			private object itemField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("АдрИнф", typeof(АдрИнфТип))]
			[System.Xml.Serialization.XmlElementAttribute("АдрРФ", typeof(АдрРФТип))]
			[System.Xml.Serialization.XmlElementAttribute("КодГАР", typeof(string))]
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
		public partial class СвФЛТип
		{

			private ФИОТип фИОField;

			private string госРегИПВыдДовField;

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
			public string ГосРегИПВыдДов
			{
				get { return this.госРегИПВыдДовField; }
				set { this.госРегИПВыдДовField = value; }
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
		public partial class СвИПТип
		{

			private ФИОТип фИОField;

			private string иННФЛField;

			private СвИПТипДефИННФЛ дефИННФЛField;

			private bool дефИННФЛFieldSpecified;

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
			public СвИПТипДефИННФЛ ДефИННФЛ
			{
				get { return this.дефИННФЛField; }
				set { this.дефИННФЛField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ДефИННФЛSpecified
			{
				get { return this.дефИННФЛFieldSpecified; }
				set { this.дефИННФЛFieldSpecified = value; }
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
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum СвИПТипДефИННФЛ
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("-")]
			Item,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		public partial class УчастникТип
		{

			private УчастникТипИдСв идСвField;

			private АдресТип адресField;

			private КонтактТип контактField;

			private УчастникТипБанкРекв банкРеквField;

			private string оКПОField;

			private string структПодрField;

			private string инфДляУчастField;

			private string краткНазвField;

			/// <remarks/>
			public УчастникТипИдСв ИдСв
			{
				get { return this.идСвField; }
				set { this.идСвField = value; }
			}

			/// <remarks/>
			public АдресТип Адрес
			{
				get { return this.адресField; }
				set { this.адресField = value; }
			}

			/// <remarks/>
			public КонтактТип Контакт
			{
				get { return this.контактField; }
				set { this.контактField = value; }
			}

			/// <remarks/>
			public УчастникТипБанкРекв БанкРекв
			{
				get { return this.банкРеквField; }
				set { this.банкРеквField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОКПО
			{
				get { return this.оКПОField; }
				set { this.оКПОField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string СтруктПодр
			{
				get { return this.структПодрField; }
				set { this.структПодрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИнфДляУчаст
			{
				get { return this.инфДляУчастField; }
				set { this.инфДляУчастField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КраткНазв
			{
				get { return this.краткНазвField; }
				set { this.краткНазвField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class УчастникТипИдСв
		{

			private object itemField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("СвИП", typeof(СвИПТип))]
			[System.Xml.Serialization.XmlElementAttribute("СвИнНеУч", typeof(УчастникТипИдСвСвИнНеУч))]
			[System.Xml.Serialization.XmlElementAttribute("СвФЛУчастФХЖ", typeof(СвФЛТип))]
			[System.Xml.Serialization.XmlElementAttribute("СвЮЛУч", typeof(УчастникТипИдСвСвЮЛУч))]
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
		public partial class УчастникТипИдСвСвИнНеУч
		{

			private string наимОргField;

			private string идентифField;

			private string иныеСведField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимОрг
			{
				get { return this.наимОргField; }
				set { this.наимОргField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string Идентиф
			{
				get { return this.идентифField; }
				set { this.идентифField = value; }
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
		public partial class УчастникТипИдСвСвЮЛУч
		{

			private string наимОргField;

			private string иННЮЛField;

			private УчастникТипИдСвСвЮЛУчДефИННЮЛ дефИННЮЛField;

			private bool дефИННЮЛFieldSpecified;

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
			public УчастникТипИдСвСвЮЛУчДефИННЮЛ ДефИННЮЛ
			{
				get { return this.дефИННЮЛField; }
				set { this.дефИННЮЛField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ДефИННЮЛSpecified
			{
				get { return this.дефИННЮЛFieldSpecified; }
				set { this.дефИННЮЛFieldSpecified = value; }
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
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum УчастникТипИдСвСвЮЛУчДефИННЮЛ
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("-")]
			Item,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class УчастникТипБанкРекв
		{

			private УчастникТипБанкРеквСвБанк свБанкField;

			private string номерСчетаField;

			/// <remarks/>
			public УчастникТипБанкРеквСвБанк СвБанк
			{
				get { return this.свБанкField; }
				set { this.свБанкField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомерСчета
			{
				get { return this.номерСчетаField; }
				set { this.номерСчетаField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class УчастникТипБанкРеквСвБанк
		{

			private string наимБанкField;

			private string бИКField;

			private string корСчетField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимБанк
			{
				get { return this.наимБанкField; }
				set { this.наимБанкField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string БИК
			{
				get { return this.бИКField; }
				set { this.бИКField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КорСчет
			{
				get { return this.корСчетField; }
				set { this.корСчетField = value; }
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

			private ФайлДокументСвСчФакт свСчФактField;

			private ФайлДокументТаблСчФакт таблСчФактField;

			private ФайлДокументСвПродПер свПродПерField;

			private ФайлДокументПодписант[] подписантField;

			private ФайлДокументКНД кНДField;

			private ФайлДокументФункция функцияField;

			private string поФактХЖField;

			private string наимДокОпрField;

			private string датаИнфПрField;

			private string времИнфПрField;

			private string наимЭконСубСостField;

			private string оснДоверОргСостField;

			private string соглСтрДопИнфField;

			/// <remarks/>
			public ФайлДокументСвСчФакт СвСчФакт
			{
				get { return this.свСчФактField; }
				set { this.свСчФактField = value; }
			}

			/// <remarks/>
			public ФайлДокументТаблСчФакт ТаблСчФакт
			{
				get { return this.таблСчФактField; }
				set { this.таблСчФактField = value; }
			}

			/// <remarks/>
			public ФайлДокументСвПродПер СвПродПер
			{
				get { return this.свПродПерField; }
				set { this.свПродПерField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("Подписант")]
			public ФайлДокументПодписант[] Подписант
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

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументФункция Функция
			{
				get { return this.функцияField; }
				set { this.функцияField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ПоФактХЖ
			{
				get { return this.поФактХЖField; }
				set { this.поФактХЖField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимДокОпр
			{
				get { return this.наимДокОпрField; }
				set { this.наимДокОпрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаИнфПр
			{
				get { return this.датаИнфПрField; }
				set { this.датаИнфПрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ВремИнфПр
			{
				get { return this.времИнфПрField; }
				set { this.времИнфПрField = value; }
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

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string СоглСтрДопИнф
			{
				get { return this.соглСтрДопИнфField; }
				set { this.соглСтрДопИнфField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвСчФакт
		{

			private ФайлДокументСвСчФактИспрСчФ испрСчФField;

			private УчастникТип[] свПродField;

			private ФайлДокументСвСчФактГрузОт[] грузОтField;

			private УчастникТип[] грузПолучField;

			private ФайлДокументСвСчФактСвПРД[] свПРДField;

			private УчастникТип[] свПокупField;

			private ФайлДокументСвСчФактДопСвФХЖ1 допСвФХЖ1Field;

			private ФайлДокументСвСчФактДокПодтвОтгр[] докПодтвОтгрField;

			private ФайлДокументСвСчФактИнфПолФХЖ1 инфПолФХЖ1Field;

			private string номерСчФField;

			private string датаСчФField;

			private string кодОКВField;

			/// <remarks/>
			public ФайлДокументСвСчФактИспрСчФ ИспрСчФ
			{
				get { return this.испрСчФField; }
				set { this.испрСчФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("СвПрод")]
			public УчастникТип[] СвПрод
			{
				get { return this.свПродField; }
				set { this.свПродField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ГрузОт")]
			public ФайлДокументСвСчФактГрузОт[] ГрузОт
			{
				get { return this.грузОтField; }
				set { this.грузОтField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ГрузПолуч")]
			public УчастникТип[] ГрузПолуч
			{
				get { return this.грузПолучField; }
				set { this.грузПолучField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("СвПРД")]
			public ФайлДокументСвСчФактСвПРД[] СвПРД
			{
				get { return this.свПРДField; }
				set { this.свПРДField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("СвПокуп")]
			public УчастникТип[] СвПокуп
			{
				get { return this.свПокупField; }
				set { this.свПокупField = value; }
			}

			/// <remarks/>
			public ФайлДокументСвСчФактДопСвФХЖ1 ДопСвФХЖ1
			{
				get { return this.допСвФХЖ1Field; }
				set { this.допСвФХЖ1Field = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ДокПодтвОтгр")]
			public ФайлДокументСвСчФактДокПодтвОтгр[] ДокПодтвОтгр
			{
				get { return this.докПодтвОтгрField; }
				set { this.докПодтвОтгрField = value; }
			}

			/// <remarks/>
			public ФайлДокументСвСчФактИнфПолФХЖ1 ИнфПолФХЖ1
			{
				get { return this.инфПолФХЖ1Field; }
				set { this.инфПолФХЖ1Field = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомерСчФ
			{
				get { return this.номерСчФField; }
				set { this.номерСчФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаСчФ
			{
				get { return this.датаСчФField; }
				set { this.датаСчФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодОКВ
			{
				get { return this.кодОКВField; }
				set { this.кодОКВField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвСчФактИспрСчФ
		{

			private string номИспрСчФField;

			private ФайлДокументСвСчФактИспрСчФДефНомИспрСчФ дефНомИспрСчФField;

			private bool дефНомИспрСчФFieldSpecified;

			private string датаИспрСчФField;

			private ФайлДокументСвСчФактИспрСчФДефДатаИспрСчФ дефДатаИспрСчФField;

			private bool дефДатаИспрСчФFieldSpecified;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
			public string НомИспрСчФ
			{
				get { return this.номИспрСчФField; }
				set { this.номИспрСчФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументСвСчФактИспрСчФДефНомИспрСчФ ДефНомИспрСчФ
			{
				get { return this.дефНомИспрСчФField; }
				set { this.дефНомИспрСчФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ДефНомИспрСчФSpecified
			{
				get { return this.дефНомИспрСчФFieldSpecified; }
				set { this.дефНомИспрСчФFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаИспрСчФ
			{
				get { return this.датаИспрСчФField; }
				set { this.датаИспрСчФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументСвСчФактИспрСчФДефДатаИспрСчФ ДефДатаИспрСчФ
			{
				get { return this.дефДатаИспрСчФField; }
				set { this.дефДатаИспрСчФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ДефДатаИспрСчФSpecified
			{
				get { return this.дефДатаИспрСчФFieldSpecified; }
				set { this.дефДатаИспрСчФFieldSpecified = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументСвСчФактИспрСчФДефНомИспрСчФ
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("-")]
			Item,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументСвСчФактИспрСчФДефДатаИспрСчФ
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("-")]
			Item,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвСчФактГрузОт
		{

			private object itemField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ГрузОтпр", typeof(УчастникТип))]
			[System.Xml.Serialization.XmlElementAttribute("ОнЖе", typeof(ФайлДокументСвСчФактГрузОтОнЖе))]
			public object Item
			{
				get { return this.itemField; }
				set { this.itemField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументСвСчФактГрузОтОнЖе
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("он же")]
			онже,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвСчФактСвПРД
		{

			private string номерПРДField;

			private string датаПРДField;

			private decimal суммаПРДField;

			private bool суммаПРДFieldSpecified;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомерПРД
			{
				get { return this.номерПРДField; }
				set { this.номерПРДField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаПРД
			{
				get { return this.датаПРДField; }
				set { this.датаПРДField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal СуммаПРД
			{
				get { return this.суммаПРДField; }
				set { this.суммаПРДField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool СуммаПРДSpecified
			{
				get { return this.суммаПРДFieldSpecified; }
				set { this.суммаПРДFieldSpecified = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвСчФактДопСвФХЖ1
		{

			private ФайлДокументСвСчФактДопСвФХЖ1ИнфПродГосЗакКазн инфПродГосЗакКазнField;

			private УчастникТип свФакторField;

			private ОснованиеТип оснУстДенТребField;

			private string идГосКонField;

			private string наимОКВField;

			private decimal курсВалField;

			private bool курсВалFieldSpecified;

			private ФайлДокументСвСчФактДопСвФХЖ1ОбстФормСЧФ обстФормСЧФField;

			private bool обстФормСЧФFieldSpecified;

			/// <remarks/>
			public ФайлДокументСвСчФактДопСвФХЖ1ИнфПродГосЗакКазн ИнфПродГосЗакКазн
			{
				get { return this.инфПродГосЗакКазнField; }
				set { this.инфПродГосЗакКазнField = value; }
			}

			/// <remarks/>
			public УчастникТип СвФактор
			{
				get { return this.свФакторField; }
				set { this.свФакторField = value; }
			}

			/// <remarks/>
			public ОснованиеТип ОснУстДенТреб
			{
				get { return this.оснУстДенТребField; }
				set { this.оснУстДенТребField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИдГосКон
			{
				get { return this.идГосКонField; }
				set { this.идГосКонField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимОКВ
			{
				get { return this.наимОКВField; }
				set { this.наимОКВField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal КурсВал
			{
				get { return this.курсВалField; }
				set { this.курсВалField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool КурсВалSpecified
			{
				get { return this.курсВалFieldSpecified; }
				set { this.курсВалFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументСвСчФактДопСвФХЖ1ОбстФормСЧФ ОбстФормСЧФ
			{
				get { return this.обстФормСЧФField; }
				set { this.обстФормСЧФField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ОбстФормСЧФSpecified
			{
				get { return this.обстФормСЧФFieldSpecified; }
				set { this.обстФормСЧФFieldSpecified = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвСчФактДопСвФХЖ1ИнфПродГосЗакКазн
		{

			private string датаГосКонтField;

			private string номерГосКонтField;

			private string лицСчетПродField;

			private string кодПродБюджКлассField;

			private string кодЦелиПродField;

			private string кодКазначПродField;

			private string наимКазначПродField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаГосКонт
			{
				get { return this.датаГосКонтField; }
				set { this.датаГосКонтField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомерГосКонт
			{
				get { return this.номерГосКонтField; }
				set { this.номерГосКонтField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ЛицСчетПрод
			{
				get { return this.лицСчетПродField; }
				set { this.лицСчетПродField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодПродБюджКласс
			{
				get { return this.кодПродБюджКлассField; }
				set { this.кодПродБюджКлассField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодЦелиПрод
			{
				get { return this.кодЦелиПродField; }
				set { this.кодЦелиПродField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодКазначПрод
			{
				get { return this.кодКазначПродField; }
				set { this.кодКазначПродField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимКазначПрод
			{
				get { return this.наимКазначПродField; }
				set { this.наимКазначПродField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументСвСчФактДопСвФХЖ1ОбстФормСЧФ
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

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("7")]
			Item7,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("8")]
			Item8,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвСчФактДокПодтвОтгр
		{

			private string наимДокОтгрField;

			private string номДокОтгрField;

			private string датаДокОтгрField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимДокОтгр
			{
				get { return this.наимДокОтгрField; }
				set { this.наимДокОтгрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомДокОтгр
			{
				get { return this.номДокОтгрField; }
				set { this.номДокОтгрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаДокОтгр
			{
				get { return this.датаДокОтгрField; }
				set { this.датаДокОтгрField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвСчФактИнфПолФХЖ1
		{

			private ТекстИнфТип[] текстИнфField;

			private string идФайлИнфПолField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ТекстИнф")]
			public ТекстИнфТип[] ТекстИнф
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
		public partial class ФайлДокументТаблСчФакт
		{

			private ФайлДокументТаблСчФактСведТов[] сведТовField;

			private ФайлДокументТаблСчФактВсегоОпл всегоОплField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("СведТов")]
			public ФайлДокументТаблСчФактСведТов[] СведТов
			{
				get { return this.сведТовField; }
				set { this.сведТовField = value; }
			}

			/// <remarks/>
			public ФайлДокументТаблСчФактВсегоОпл ВсегоОпл
			{
				get { return this.всегоОплField; }
				set { this.всегоОплField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументТаблСчФактСведТов
		{

			private СумАкцизТип акцизField;

			private СумНДСТип сумНалField;

			private ФайлДокументТаблСчФактСведТовСвТД[] свТДField;

			private ФайлДокументТаблСчФактСведТовДопСведТов допСведТовField;

			private ТекстИнфТип[] инфПолФХЖ2Field;

			private string номСтрField;

			private string наимТовField;

			private string оКЕИ_ТовField;

			private ФайлДокументТаблСчФактСведТовДефОКЕИ_Тов дефОКЕИ_ТовField;

			private bool дефОКЕИ_ТовFieldSpecified;

			private decimal колТовField;

			private bool колТовFieldSpecified;

			private decimal ценаТовField;

			private bool ценаТовFieldSpecified;

			private decimal стТовБезНДСField;

			private bool стТовБезНДСFieldSpecified;

			private ФайлДокументТаблСчФактСведТовНалСт налСтField;

			private decimal стТовУчНалField;

			private bool стТовУчНалFieldSpecified;

			private ФайлДокументТаблСчФактСведТовДефСтТовУчНал дефСтТовУчНалField;

			private bool дефСтТовУчНалFieldSpecified;

			/// <remarks/>
			public СумАкцизТип Акциз
			{
				get { return this.акцизField; }
				set { this.акцизField = value; }
			}

			/// <remarks/>
			public СумНДСТип СумНал
			{
				get { return this.сумНалField; }
				set { this.сумНалField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("СвТД")]
			public ФайлДокументТаблСчФактСведТовСвТД[] СвТД
			{
				get { return this.свТДField; }
				set { this.свТДField = value; }
			}

			/// <remarks/>
			public ФайлДокументТаблСчФактСведТовДопСведТов ДопСведТов
			{
				get { return this.допСведТовField; }
				set { this.допСведТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ИнфПолФХЖ2")]
			public ТекстИнфТип[] ИнфПолФХЖ2
			{
				get { return this.инфПолФХЖ2Field; }
				set { this.инфПолФХЖ2Field = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
			public string НомСтр
			{
				get { return this.номСтрField; }
				set { this.номСтрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимТов
			{
				get { return this.наимТовField; }
				set { this.наимТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОКЕИ_Тов
			{
				get { return this.оКЕИ_ТовField; }
				set { this.оКЕИ_ТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументТаблСчФактСведТовДефОКЕИ_Тов ДефОКЕИ_Тов
			{
				get { return this.дефОКЕИ_ТовField; }
				set { this.дефОКЕИ_ТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ДефОКЕИ_ТовSpecified
			{
				get { return this.дефОКЕИ_ТовFieldSpecified; }
				set { this.дефОКЕИ_ТовFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal КолТов
			{
				get { return this.колТовField; }
				set { this.колТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool КолТовSpecified
			{
				get { return this.колТовFieldSpecified; }
				set { this.колТовFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal ЦенаТов
			{
				get { return this.ценаТовField; }
				set { this.ценаТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ЦенаТовSpecified
			{
				get { return this.ценаТовFieldSpecified; }
				set { this.ценаТовFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal СтТовБезНДС
			{
				get { return this.стТовБезНДСField; }
				set { this.стТовБезНДСField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool СтТовБезНДСSpecified
			{
				get { return this.стТовБезНДСFieldSpecified; }
				set { this.стТовБезНДСFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументТаблСчФактСведТовНалСт НалСт
			{
				get { return this.налСтField; }
				set { this.налСтField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal СтТовУчНал
			{
				get { return this.стТовУчНалField; }
				set { this.стТовУчНалField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool СтТовУчНалSpecified
			{
				get { return this.стТовУчНалFieldSpecified; }
				set { this.стТовУчНалFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументТаблСчФактСведТовДефСтТовУчНал ДефСтТовУчНал
			{
				get { return this.дефСтТовУчНалField; }
				set { this.дефСтТовУчНалField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ДефСтТовУчНалSpecified
			{
				get { return this.дефСтТовУчНалFieldSpecified; }
				set { this.дефСтТовУчНалFieldSpecified = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументТаблСчФактСведТовСвТД
		{

			private string кодПроисхField;

			private ФайлДокументТаблСчФактСведТовСвТДДефКодПроисх дефКодПроисхField;

			private bool дефКодПроисхFieldSpecified;

			private string номерТДField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодПроисх
			{
				get { return this.кодПроисхField; }
				set { this.кодПроисхField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументТаблСчФактСведТовСвТДДефКодПроисх ДефКодПроисх
			{
				get { return this.дефКодПроисхField; }
				set { this.дефКодПроисхField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ДефКодПроисхSpecified
			{
				get { return this.дефКодПроисхFieldSpecified; }
				set { this.дефКодПроисхFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомерТД
			{
				get { return this.номерТДField; }
				set { this.номерТДField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументТаблСчФактСведТовСвТДДефКодПроисх
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("-")]
			Item,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументТаблСчФактСведТовДопСведТов
		{

			private ФайлДокументТаблСчФактСведТовДопСведТовСведПрослеж[] сведПрослежField;

			private ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов[] номСредИдентТовField;

			private ФайлДокументТаблСчФактСведТовДопСведТовПрТовРаб прТовРабField;

			private bool прТовРабFieldSpecified;

			private string допПризнField;

			private string наимЕдИзмField;

			private string крНаимСтрПрField;

			private decimal надлОтпField;

			private bool надлОтпFieldSpecified;

			private string характерТовField;

			private string сортТовField;

			private string артикулТовField;

			private string кодТовField;

			private string кодКатField;

			private string кодВидТовField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("СведПрослеж")]
			public ФайлДокументТаблСчФактСведТовДопСведТовСведПрослеж[] СведПрослеж
			{
				get { return this.сведПрослежField; }
				set { this.сведПрослежField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("НомСредИдентТов")]
			public ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов[] НомСредИдентТов
			{
				get { return this.номСредИдентТовField; }
				set { this.номСредИдентТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументТаблСчФактСведТовДопСведТовПрТовРаб ПрТовРаб
			{
				get { return this.прТовРабField; }
				set { this.прТовРабField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ПрТовРабSpecified
			{
				get { return this.прТовРабFieldSpecified; }
				set { this.прТовРабFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДопПризн
			{
				get { return this.допПризнField; }
				set { this.допПризнField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимЕдИзм
			{
				get { return this.наимЕдИзмField; }
				set { this.наимЕдИзмField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КрНаимСтрПр
			{
				get { return this.крНаимСтрПрField; }
				set { this.крНаимСтрПрField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal НадлОтп
			{
				get { return this.надлОтпField; }
				set { this.надлОтпField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool НадлОтпSpecified
			{
				get { return this.надлОтпFieldSpecified; }
				set { this.надлОтпFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ХарактерТов
			{
				get { return this.характерТовField; }
				set { this.характерТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string СортТов
			{
				get { return this.сортТовField; }
				set { this.сортТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string АртикулТов
			{
				get { return this.артикулТовField; }
				set { this.артикулТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодТов
			{
				get { return this.кодТовField; }
				set { this.кодТовField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодКат
			{
				get { return this.кодКатField; }
				set { this.кодКатField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string КодВидТов
			{
				get { return this.кодВидТовField; }
				set { this.кодВидТовField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументТаблСчФактСведТовДопСведТовСведПрослеж
		{

			private string номТовПрослежField;

			private string едИзмПрослежField;

			private string наимЕдИзмПрослежField;

			private decimal колВЕдПрослежField;

			private string допПрослежField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомТовПрослеж
			{
				get { return this.номТовПрослежField; }
				set { this.номТовПрослежField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ЕдИзмПрослеж
			{
				get { return this.едИзмПрослежField; }
				set { this.едИзмПрослежField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НаимЕдИзмПрослеж
			{
				get { return this.наимЕдИзмПрослежField; }
				set { this.наимЕдИзмПрослежField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal КолВЕдПрослеж
			{
				get { return this.колВЕдПрослежField; }
				set { this.колВЕдПрослежField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДопПрослеж
			{
				get { return this.допПрослежField; }
				set { this.допПрослежField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов
		{

			private string[] itemsField;

			private ItemsChoiceType[] itemsElementNameField;

			private string идентТрансУпакField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("КИЗ", typeof(string))]
			[System.Xml.Serialization.XmlElementAttribute("НомУпак", typeof(string))]
			[System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
			public string[] Items
			{
				get { return this.itemsField; }
				set { this.itemsField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public ItemsChoiceType[] ItemsElementName
			{
				get { return this.itemsElementNameField; }
				set { this.itemsElementNameField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ИдентТрансУпак
			{
				get { return this.идентТрансУпакField; }
				set { this.идентТрансУпакField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema = false)]
		public enum ItemsChoiceType
		{

			/// <remarks/>
			КИЗ,

			/// <remarks/>
			НомУпак,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументТаблСчФактСведТовДопСведТовПрТовРаб
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
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументТаблСчФактСведТовДефОКЕИ_Тов
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("-")]
			Item,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументТаблСчФактСведТовНалСт
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("0%")]
			Item0,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("10%")]
			Item10,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("18%")]
			Item18,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("20%")]
			Item20,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("10/110")]
			Item10110,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("18/118")]
			Item18118,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("20/120")]
			Item20120,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("без НДС")]
			безНДС,

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("НДС исчисляется налоговым агентом")]
			НДСисчисляетсяналоговымагентом,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументТаблСчФактСведТовДефСтТовУчНал
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("-")]
			Item,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументТаблСчФактВсегоОпл
		{

			private СумНДСТип сумНалВсегоField;

			private decimal колНеттоВсField;

			private bool колНеттоВсFieldSpecified;

			private decimal стТовБезНДСВсегоField;

			private bool стТовБезНДСВсегоFieldSpecified;

			private decimal стТовУчНалВсегоField;

			private bool стТовУчНалВсегоFieldSpecified;

			private ФайлДокументТаблСчФактВсегоОплДефСтТовУчНалВсего дефСтТовУчНалВсегоField;

			private bool дефСтТовУчНалВсегоFieldSpecified;

			/// <remarks/>
			public СумНДСТип СумНалВсего
			{
				get { return this.сумНалВсегоField; }
				set { this.сумНалВсегоField = value; }
			}

			/// <remarks/>
			public decimal КолНеттоВс
			{
				get { return this.колНеттоВсField; }
				set { this.колНеттоВсField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool КолНеттоВсSpecified
			{
				get { return this.колНеттоВсFieldSpecified; }
				set { this.колНеттоВсFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal СтТовБезНДСВсего
			{
				get { return this.стТовБезНДСВсегоField; }
				set { this.стТовБезНДСВсегоField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool СтТовБезНДСВсегоSpecified
			{
				get { return this.стТовБезНДСВсегоFieldSpecified; }
				set { this.стТовБезНДСВсегоFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public decimal СтТовУчНалВсего
			{
				get { return this.стТовУчНалВсегоField; }
				set { this.стТовУчНалВсегоField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool СтТовУчНалВсегоSpecified
			{
				get { return this.стТовУчНалВсегоFieldSpecified; }
				set { this.стТовУчНалВсегоFieldSpecified = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументТаблСчФактВсегоОплДефСтТовУчНалВсего ДефСтТовУчНалВсего
			{
				get { return this.дефСтТовУчНалВсегоField; }
				set { this.дефСтТовУчНалВсегоField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlIgnoreAttribute()]
			public bool ДефСтТовУчНалВсегоSpecified
			{
				get { return this.дефСтТовУчНалВсегоFieldSpecified; }
				set { this.дефСтТовУчНалВсегоFieldSpecified = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументТаблСчФактВсегоОплДефСтТовУчНалВсего
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("-")]
			Item,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвПродПер
		{

			private ФайлДокументСвПродПерСвПер свПерField;

			private ФайлДокументСвПродПерИнфПолФХЖ3 инфПолФХЖ3Field;

			/// <remarks/>
			public ФайлДокументСвПродПерСвПер СвПер
			{
				get { return this.свПерField; }
				set { this.свПерField = value; }
			}

			/// <remarks/>
			public ФайлДокументСвПродПерИнфПолФХЖ3 ИнфПолФХЖ3
			{
				get { return this.инфПолФХЖ3Field; }
				set { this.инфПолФХЖ3Field = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвПродПерСвПер
		{

			private ОснованиеТип[] оснПерField;

			private ФайлДокументСвПродПерСвПерСвЛицПер свЛицПерField;

			private ФайлДокументСвПродПерСвПерТранГруз транГрузField;

			private ФайлДокументСвПродПерСвПерСвПерВещи свПерВещиField;

			private string содОперField;

			private string видОперField;

			private string датаПерField;

			private string датаНачField;

			private string датаОконField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ОснПер")]
			public ОснованиеТип[] ОснПер
			{
				get { return this.оснПерField; }
				set { this.оснПерField = value; }
			}

			/// <remarks/>
			public ФайлДокументСвПродПерСвПерСвЛицПер СвЛицПер
			{
				get { return this.свЛицПерField; }
				set { this.свЛицПерField = value; }
			}

			/// <remarks/>
			public ФайлДокументСвПродПерСвПерТранГруз ТранГруз
			{
				get { return this.транГрузField; }
				set { this.транГрузField = value; }
			}

			/// <remarks/>
			public ФайлДокументСвПродПерСвПерСвПерВещи СвПерВещи
			{
				get { return this.свПерВещиField; }
				set { this.свПерВещиField = value; }
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
			public string ВидОпер
			{
				get { return this.видОперField; }
				set { this.видОперField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаПер
			{
				get { return this.датаПерField; }
				set { this.датаПерField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаНач
			{
				get { return this.датаНачField; }
				set { this.датаНачField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаОкон
			{
				get { return this.датаОконField; }
				set { this.датаОконField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвПродПерСвПерСвЛицПер
		{

			private object itemField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ИнЛицо", typeof(ФайлДокументСвПродПерСвПерСвЛицПерИнЛицо))]
			[System.Xml.Serialization.XmlElementAttribute("РабОргПрод", typeof(ФайлДокументСвПродПерСвПерСвЛицПерРабОргПрод))]
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
		public partial class ФайлДокументСвПродПерСвПерСвЛицПерИнЛицо
		{

			private object itemField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ПредОргПер", typeof(ФайлДокументСвПродПерСвПерСвЛицПерИнЛицоПредОргПер))]
			[System.Xml.Serialization.XmlElementAttribute("ФЛПер", typeof(ФайлДокументСвПродПерСвПерСвЛицПерИнЛицоФЛПер))]
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
		public partial class ФайлДокументСвПродПерСвПерСвЛицПерИнЛицоПредОргПер
		{

			private ФИОТип фИОField;

			private string должностьField;

			private string иныеСведField;

			private string наимОргПерField;

			private string оснДоверОргПерField;

			private string оснПолнПредПерField;

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
			public string НаимОргПер
			{
				get { return this.наимОргПерField; }
				set { this.наимОргПерField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОснДоверОргПер
			{
				get { return this.оснДоверОргПерField; }
				set { this.оснДоверОргПерField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ОснПолнПредПер
			{
				get { return this.оснПолнПредПерField; }
				set { this.оснПолнПредПерField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвПродПерСвПерСвЛицПерИнЛицоФЛПер
		{

			private ФИОТип фИОField;

			private string иныеСведField;

			private string оснДоверФЛField;

			/// <remarks/>
			public ФИОТип ФИО
			{
				get { return this.фИОField; }
				set { this.фИОField = value; }
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
			public string ОснДоверФЛ
			{
				get { return this.оснДоверФЛField; }
				set { this.оснДоверФЛField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвПродПерСвПерСвЛицПерРабОргПрод
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
		public partial class ФайлДокументСвПродПерСвПерТранГруз
		{

			private ФайлДокументСвПродПерСвПерТранГрузТранНакл[] транНаклField;

			private УчастникТип перевозчикField;

			private string свТранГрузField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ТранНакл")]
			public ФайлДокументСвПродПерСвПерТранГрузТранНакл[] ТранНакл
			{
				get { return this.транНаклField; }
				set { this.транНаклField = value; }
			}

			/// <remarks/>
			public УчастникТип Перевозчик
			{
				get { return this.перевозчикField; }
				set { this.перевозчикField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string СвТранГруз
			{
				get { return this.свТранГрузField; }
				set { this.свТранГрузField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвПродПерСвПерТранГрузТранНакл
		{

			private string номТранНаклField;

			private string датаТранНаклField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string НомТранНакл
			{
				get { return this.номТранНаклField; }
				set { this.номТранНаклField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаТранНакл
			{
				get { return this.датаТранНаклField; }
				set { this.датаТранНаклField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвПродПерСвПерСвПерВещи
		{

			private string датаПерВещField;

			private string свПерВещField;

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string ДатаПерВещ
			{
				get { return this.датаПерВещField; }
				set { this.датаПерВещField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public string СвПерВещ
			{
				get { return this.свПерВещField; }
				set { this.свПерВещField = value; }
			}
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Diagnostics.DebuggerStepThroughAttribute()]
		[System.ComponentModel.DesignerCategoryAttribute("code")]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public partial class ФайлДокументСвПродПерИнфПолФХЖ3
		{

			private ТекстИнфТип[] текстИнфField;

			private string идФайлИнфПолField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ТекстИнф")]
			public ТекстИнфТип[] ТекстИнф
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
		public partial class ФайлДокументПодписант
		{

			private object itemField;

			private ФайлДокументПодписантОблПолн облПолнField;

			private ФайлДокументПодписантСтатус статусField;

			private string оснПолнField;

			private string оснПолнОргField;

			/// <remarks/>
			[System.Xml.Serialization.XmlElementAttribute("ИП", typeof(СвИПТип))]
			[System.Xml.Serialization.XmlElementAttribute("ФЛ", typeof(СвФЛТип))]
			[System.Xml.Serialization.XmlElementAttribute("ЮЛ", typeof(ФайлДокументПодписантЮЛ))]
			public object Item
			{
				get { return this.itemField; }
				set { this.itemField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументПодписантОблПолн ОблПолн
			{
				get { return this.облПолнField; }
				set { this.облПолнField = value; }
			}

			/// <remarks/>
			[System.Xml.Serialization.XmlAttributeAttribute()]
			public ФайлДокументПодписантСтатус Статус
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
		public partial class ФайлДокументПодписантЮЛ
		{

			private ФИОТип фИОField;

			private string госРегИПВыдДовField;

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
			public string ГосРегИПВыдДов
			{
				get { return this.госРегИПВыдДовField; }
				set { this.госРегИПВыдДовField = value; }
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
		public enum ФайлДокументПодписантОблПолн
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("0")]
			Item0,

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
		public enum ФайлДокументПодписантСтатус
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
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументКНД
		{

			/// <remarks/>
			[System.Xml.Serialization.XmlEnumAttribute("1115131")]
			Item1115131,
		}

		/// <remarks/>
		[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
		[System.SerializableAttribute()]
		[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
		public enum ФайлДокументФункция
		{

			/// <remarks/>
			СЧФ,

			/// <remarks/>
			СЧФДОП,

			/// <remarks/>
			ДОП,

			/// <remarks/>
			СвРК,

			/// <remarks/>
			СвЗК,
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
