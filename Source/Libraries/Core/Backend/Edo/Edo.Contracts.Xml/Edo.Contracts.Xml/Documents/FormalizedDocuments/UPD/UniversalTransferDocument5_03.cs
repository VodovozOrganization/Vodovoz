using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Edo.Contracts.Xml.Documents.FormalizedDocuments.UPD
{
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	[XmlRoot(ElementName = "Файл", Namespace = "", IsNullable = false)]
	public class UniversalTransferDocument5_03
	{
		/// <remarks/>
		[XmlElement("Документ")]
		public UniversalTransferDocument UniversalTransferDocument { get; set; }

		/// <remarks/>
		[XmlAttribute("ИдФайл")]
		public string Id { get; set; }

		/// <summary>
		/// Формат документа
		/// </summary>
		[XmlAttribute("ВерсФорм")]
		public Format Format { get; set; }

		/// <summary>
		/// Версия программы, с помощью которой сформирован файл
		/// </summary>
		[XmlAttribute("ВерсПрог")]
		public string ProgramVersion { get; set; }
	}

	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public class UniversalTransferDocument
	{
		[XmlElement("СвСчФакт")]
		public ФайлДокументСвСчФакт СвСчФакт { get; set; }

		/// <remarks/>
		[XmlElement("ТаблСчФакт")]
		public ФайлДокументТаблСчФакт ТаблСчФакт { get; set; }

		/// <remarks/>
		[XmlElement("СвПродПер")]
		public ФайлДокументСвПродПер СвПродПер { get; set; }

		/// <remarks/>
		[XmlElement("Подписант")]
		public ФайлДокументПодписант[] Подписант { get; set; }

		/// <remarks/>
		[XmlElement("ОснДоверОргСост")]
		public РеквДокТип ОснДоверОргСост { get; set; }

		/// <remarks/>
		[XmlAttribute("КНД")]
		public FiscalDocumentClassifiers FiscalDocumentClassifiers { get; set; } = FiscalDocumentClassifiers.KND1115131;

		/// <remarks/>
		[XmlAttribute("УИД")]
		public string УИД { get; set; }

		/// <remarks/>
		[XmlAttribute("Функция")]
		public ФайлДокументФункция Функция { get; set; }

		/// <remarks/>
		[XmlAttribute("ПоФактХЖ")]
		public string ПоФактХЖ { get; set; }

		/// <remarks/>
		[XmlAttribute("НаимДокОпр")]
		public string НаимДокОпр { get; set; }

		/// <remarks/>
		[XmlAttribute("ДатаИнфПр")]
		public string ДатаИнфПр { get; set; }

		/// <remarks/>
		[XmlAttribute("ВремИнфПр")]
		public string ВремИнфПр { get; set; }

		/// <remarks/>
		[XmlAttribute("НаимЭконСубСост")]
		public string НаимЭконСубСост { get; set; }

		/// <remarks/>
		[XmlAttribute("СоглСтрДопИнф")]
		public string СоглСтрДопИнф { get; set; }
	}

	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public class ФайлДокументСвСчФакт
	{
		/// <remarks/>
		[XmlElement("ИспрДок")]
		public ФайлДокументСвСчФактИспрДок ИспрДок { get; set; }

		/// <remarks/>
		[XmlElement("СвПрод")]
		public УчастникТип[] СвПрод { get; set; }

		/// <remarks/>
		[XmlElement("ГрузОт")]
		public ФайлДокументСвСчФактГрузОт[] ГрузОт { get; set; }

		/// <remarks/>
		[XmlElement("ГрузПолуч")]
		public УчастникТип[] ГрузПолуч { get; set; }

		/// <remarks/>
		[XmlElement("СвПРД")]
		public ФайлДокументСвСчФактСвПРД[] СвПРД { get; set; }

		/// <remarks/>
		[XmlElement("ДокПодтвОтгрНом")]
		public РеквДокТип[] ДокПодтвОтгрНом { get; set; }

		/// <remarks/>
		[XmlElement("СвПокуп")]
		public УчастникТип[] СвПокуп { get; set; }

		/// <remarks/>
		[XmlElement("ДенИзм")]
		public ФайлДокументСвСчФактДенИзм ДенИзм { get; set; }

		/// <remarks/>
		[XmlElement("ДопСвФХЖ1")]
		public ФайлДокументСвСчФактДопСвФХЖ1 ДопСвФХЖ1 { get; set; }

		/// <remarks/>
		[XmlElement("ИнфПолФХЖ1")]
		public ФайлДокументСвСчФактИнфПолФХЖ1 ИнфПолФХЖ1 { get; set; }

		/// <remarks/>
		[XmlAttribute("НомерДок")]
		public string НомерДок { get; set; }

		/// <remarks/>
		[XmlAttribute("ДатаДок")]
		public string ДатаДок { get; set; }

		/// <remarks/>
		[XmlAttribute("ИмяФайлИспрПрод")]
		public string ИмяФайлИспрПрод { get; set; }

		/// <remarks/>
		[XmlAttribute("ИмяФайлИспрПок")]
		public string ИмяФайлИспрПок { get; set; }
	}

	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public class ФайлДокументСвСчФактИспрДок
	{
		/// <remarks/>
		[XmlAttribute(AttributeName = "НомИспр", DataType = "integer")]
		public string НомИспр { get; set; }

		/// <remarks/>
		[XmlAttribute("ДатаИспр")]
		public string ДатаИспр { get; set; }
	}

	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class УчастникТип
	{
		/// <remarks/>
		[XmlElement("ИдСв")]
		public УчастникТипИдСв ИдСв { get; set; }

		/// <remarks/>
		[XmlElement("Адрес")]
		public АдресТип Адрес { get; set; }

		/// <remarks/>
		[XmlElement("БанкРекв")]
		public УчастникТипБанкРекв БанкРекв { get; set; }

		/// <remarks/>
		[XmlElement("Контакт")]
		public КонтактТип Контакт { get; set; }

		/// <remarks/>
		[XmlAttribute("ОКПО")]
		public string ОКПО { get; set; }

		/// <remarks/>
		[XmlAttribute("КодОПФ")]
		public string КодОПФ { get; set; }

		/// <remarks/>
		[XmlAttribute("ПолнНаимОПФ")]
		public string ПолнНаимОПФ { get; set; }

		/// <remarks/>
		[XmlAttribute("СтруктПодр")]
		public string СтруктПодр { get; set; }

		/// <remarks/>
		[XmlAttribute("ИнфДляУчаст")]
		public string ИнфДляУчаст { get; set; }

		/// <remarks/>
		[XmlAttribute("СокрНаим")]
		public string СокрНаим { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public class УчастникТипИдСв
	{
		/// <remarks/>
		[XmlElement("СвИП", typeof(УчастникТипИдСвСвИП))]
		[XmlElement("СвИнНеУч", typeof(СвИнНеУчТип))]
		[XmlElement("СвФЛУч", typeof(УчастникТипИдСвСвФЛУч))]
		[XmlElement("СвЮЛУч", typeof(УчастникТипИдСвСвЮЛУч))]
		public object Item { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class АдресТип
	{
		/// <remarks/>
		[XmlElement("АдрГАР", typeof(АдрГАРТип))]
		[XmlElement("АдрИнф", typeof(АдрИнфТип))]
		[XmlElement("АдрРФ", typeof(АдрРФТип))]
		public object Item { get; set; }

		/// <remarks/>
		[XmlAttribute("ГЛНМеста")]
		public string ГЛНМеста { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public class УчастникТипБанкРекв
	{
		/// <remarks/>
		[XmlElement("СвБанк")]
		public УчастникТипБанкРеквСвБанк СвБанк { get; set; }

		/// <remarks/>
		[XmlAttribute("НомерСчета")]
		public string НомерСчета { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class КонтактТип
	{
		/// <remarks/>
		[XmlElement("Тлф")]
		public string[] Тлф { get; set; }

		/// <remarks/>
		[XmlElement("ЭлПочта")]
		public string[] ЭлПочта { get; set; }

		/// <remarks/>
		[XmlAttribute("ИнКонт")]
		public string ИнКонт { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public class СумАкцизТип
	{
		/// <remarks/>
		[XmlElement("БезАкциз", typeof(СумАкцизТипБезАкциз))]
		[XmlElement("СумАкциз", typeof(decimal))]
		public object Item { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum СумАкцизТипБезАкциз
	{
		/// <remarks/>
		[XmlEnum("без акциза")]
		безакциза,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public class СумНДСТип
	{
		/// <remarks/>
		[XmlElement("БезНДС", typeof(СумНДСТипБезНДС))]
		[XmlElement("СумНал", typeof(decimal))]
		public object Item { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum СумНДСТипБезНДС
	{
		/// <remarks/>
		[XmlEnum("без НДС")]
		безНДС,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public partial class ИдРекСостТип
	{
		/// <remarks/>
		[XmlElement("ДаннИно", typeof(СвИнНеУчТип))]
		[XmlElement("ИННФЛ", typeof(string))]
		[XmlElement("ИННЮЛ", typeof(string))]
		[XmlElement("НаимОИВ", typeof(string))]
		[XmlChoiceIdentifier("ItemElementName")]
		public object Item { get; set; }

		/// <remarks/>
		[XmlIgnore]
		public ItemChoiceType ItemElementName { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public partial class СвИнНеУчТип
	{
		/// <remarks/>
		[XmlAttribute("ИдСтат")]
		public СвИнНеУчТипИдСтат ИдСтат { get; set; }

		/// <remarks/>
		[XmlAttribute("КодСтр")]
		public string КодСтр { get; set; }

		/// <remarks/>
		[XmlAttribute("НаимСтран")]
		public string НаимСтран { get; set; }

		/// <remarks/>
		[XmlAttribute("Наим")]
		public string Наим { get; set; }

		/// <remarks/>
		[XmlAttribute("Идентиф")]
		public string Идентиф { get; set; }

		/// <remarks/>
		[XmlAttribute("ИныеСвед")]
		public string ИныеСвед { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum СвИнНеУчТипИдСтат
	{
		/// <remarks/>
		ИО,
		/// <remarks/>
		ИГ,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemChoiceType
	{
		/// <remarks/>
		ДаннИно,
		/// <remarks/>
		ИННФЛ,
		/// <remarks/>
		ИННЮЛ,
		/// <remarks/>
		НаимОИВ,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public partial class РеквДокТип
	{
		/// <remarks/>
		[XmlElement("РеквИдРекСост")]
		public ИдРекСостТип[] РеквИдРекСост { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string РеквНаимДок { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string РеквНомерДок { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string РеквДатаДок { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string РеквИдФайлДок { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string РеквИдДок { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string РИдСистХранД { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string РеквУРЛСистДок { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string РеквДопСведДок { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public partial class АдрИнфТип
	{

		private string кодСтрField;

		private string наимСтранField;

		private string адрТекстField;

		/// <remarks/>
		[XmlAttribute()]
		public string КодСтр
		{
			get => кодСтрField;
			set => кодСтрField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаимСтран
		{
			get => наимСтранField;
			set => наимСтранField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string АдрТекст
		{
			get => адрТекстField;
			set => адрТекстField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public partial class НомерТип
	{

		private string типField;

		private string номерField;

		/// <remarks/>
		[XmlAttribute()]
		public string Тип
		{
			get => типField;
			set => типField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Номер
		{
			get => номерField;
			set => номерField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public partial class ТипНаимТип
	{

		private string типField;

		private string наимField;

		/// <remarks/>
		[XmlAttribute()]
		public string Тип
		{
			get => типField;
			set => типField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Наим
		{
			get => наимField;
			set => наимField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public partial class ВидНаимТип
	{

		private string видField;

		private string наимField;

		/// <remarks/>
		[XmlAttribute()]
		public string Вид
		{
			get => видField;
			set => видField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Наим
		{
			get => наимField;
			set => наимField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public partial class ВидНаимКодТип
	{

		private string видКодField;

		private string наимField;

		/// <remarks/>
		[XmlAttribute()]
		public string ВидКод
		{
			get => видКодField;
			set => видКодField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Наим
		{
			get => наимField;
			set => наимField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public partial class АдрГАРТип
	{

		private string регионField;

		private string наимРегионField;

		private ВидНаимКодТип муниципРайонField;

		private ВидНаимКодТип городСелПоселенField;

		private ВидНаимТип населенПунктField;

		private ТипНаимТип элПланСтруктурField;

		private ТипНаимТип элУлДорСетиField;

		private string земелУчастокField;

		private НомерТип[] зданиеField;

		private НомерТип помещЗданияField;

		private НомерТип помещКвартирыField;

		private string идНомField;

		private string индексField;

		/// <remarks/>
		public string Регион
		{
			get => регионField;
			set => регионField = value;
		}

		/// <remarks/>
		public string НаимРегион
		{
			get => наимРегионField;
			set => наимРегионField = value;
		}

		/// <remarks/>
		public ВидНаимКодТип МуниципРайон
		{
			get => муниципРайонField;
			set => муниципРайонField = value;
		}

		/// <remarks/>
		public ВидНаимКодТип ГородСелПоселен
		{
			get => городСелПоселенField;
			set => городСелПоселенField = value;
		}

		/// <remarks/>
		public ВидНаимТип НаселенПункт
		{
			get => населенПунктField;
			set => населенПунктField = value;
		}

		/// <remarks/>
		public ТипНаимТип ЭлПланСтруктур
		{
			get => элПланСтруктурField;
			set => элПланСтруктурField = value;
		}

		/// <remarks/>
		public ТипНаимТип ЭлУлДорСети
		{
			get => элУлДорСетиField;
			set => элУлДорСетиField = value;
		}

		/// <remarks/>
		public string ЗемелУчасток
		{
			get => земелУчастокField;
			set => земелУчастокField = value;
		}

		/// <remarks/>
		[XmlElement("Здание")]
		public НомерТип[] Здание
		{
			get => зданиеField;
			set => зданиеField = value;
		}

		/// <remarks/>
		public НомерТип ПомещЗдания
		{
			get => помещЗданияField;
			set => помещЗданияField = value;
		}

		/// <remarks/>
		public НомерТип ПомещКвартиры
		{
			get => помещКвартирыField;
			set => помещКвартирыField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИдНом
		{
			get => идНомField;
			set => идНомField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Индекс
		{
			get => индексField;
			set => индексField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	public partial class АдрРФТип
	{

		private string индексField;

		private string кодРегионField;

		private string наимРегионField;

		private string районField;

		private string городField;

		private string населПунктField;

		private string улицаField;

		private string домField;

		private string корпусField;

		private string квартField;

		private string иныеСведField;

		/// <remarks/>
		[XmlAttribute()]
		public string Индекс
		{
			get => индексField;
			set => индексField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КодРегион
		{
			get => кодРегионField;
			set => кодРегионField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаимРегион
		{
			get => наимРегионField;
			set => наимРегионField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Район
		{
			get => районField;
			set => районField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Город
		{
			get => городField;
			set => городField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаселПункт
		{
			get => населПунктField;
			set => населПунктField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Улица
		{
			get => улицаField;
			set => улицаField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Дом
		{
			get => домField;
			set => домField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Корпус
		{
			get => корпусField;
			set => корпусField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Кварт
		{
			get => квартField;
			set => квартField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИныеСвед
		{
			get => иныеСведField;
			set => иныеСведField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class УчастникТипИдСвСвИП
	{
		/// <remarks/>
		public FullName ФИО { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string ИННФЛ { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string СвГосРегИП { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string ОГРНИП { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаОГРНИП { get; set; }

		/// <remarks/>
		[XmlAttribute()]
		public string ИныеСвед { get; set; }
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class УчастникТипИдСвСвФЛУч
	{

		private FullName фИОField;

		private string иННФЛField;

		private УчастникТипИдСвСвФЛУчИдСтатЛ идСтатЛField;

		private bool идСтатЛFieldSpecified;

		private string иныеСведField;

		/// <remarks/>
		public FullName FullName
		{
			get => фИОField;
			set => фИОField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИННФЛ
		{
			get => иННФЛField;
			set => иННФЛField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public УчастникТипИдСвСвФЛУчИдСтатЛ ИдСтатЛ
		{
			get => идСтатЛField;
			set => идСтатЛField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool ИдСтатЛSpecified
		{
			get => идСтатЛFieldSpecified;
			set => идСтатЛFieldSpecified = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИныеСвед
		{
			get => иныеСведField;
			set => иныеСведField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum УчастникТипИдСвСвФЛУчИдСтатЛ
	{

		/// <remarks/>
		[XmlEnum("0")]
		Item0,

		/// <remarks/>
		[XmlEnum("1")]
		Item1,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class УчастникТипИдСвСвЮЛУч
	{

		private string наимОргField;

		private string иННЮЛField;

		private string кППField;

		/// <remarks/>
		[XmlAttribute()]
		public string НаимОрг
		{
			get => наимОргField;
			set => наимОргField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИННЮЛ
		{
			get => иННЮЛField;
			set => иННЮЛField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КПП
		{
			get => кППField;
			set => кППField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class УчастникТипБанкРеквСвБанк
	{

		private string наимБанкField;

		private string бИКField;

		private string корСчетField;

		/// <remarks/>
		[XmlAttribute()]
		public string НаимБанк
		{
			get => наимБанкField;
			set => наимБанкField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string БИК
		{
			get => бИКField;
			set => бИКField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КорСчет
		{
			get => корСчетField;
			set => корСчетField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвСчФактГрузОт
	{

		private object itemField;

		/// <remarks/>
		[XmlElement("ГрузОтпр", typeof(УчастникТип))]
		[XmlElement("ОнЖе", typeof(ФайлДокументСвСчФактГрузОтОнЖе))]
		public object Item
		{
			get => itemField;
			set => itemField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ФайлДокументСвСчФактГрузОтОнЖе
	{

		/// <remarks/>
		[XmlEnum("он же")]
		онже,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвСчФактСвПРД
	{

		private string номерПРДField;

		private string датаПРДField;

		private decimal суммаПРДField;

		private bool суммаПРДFieldSpecified;

		/// <remarks/>
		[XmlAttribute()]
		public string НомерПРД
		{
			get => номерПРДField;
			set => номерПРДField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаПРД
		{
			get => датаПРДField;
			set => датаПРДField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal СуммаПРД
		{
			get => суммаПРДField;
			set => суммаПРДField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool СуммаПРДSpecified
		{
			get => суммаПРДFieldSpecified;
			set => суммаПРДFieldSpecified = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвСчФактДенИзм
	{

		private string кодОКВField;

		private string наимОКВField;

		private decimal курсВалField;

		private bool курсВалFieldSpecified;

		/// <remarks/>
		[XmlAttribute()]
		public string КодОКВ
		{
			get => кодОКВField;
			set => кодОКВField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаимОКВ
		{
			get => наимОКВField;
			set => наимОКВField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal КурсВал
		{
			get => курсВалField;
			set => курсВалField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool КурсВалSpecified
		{
			get => курсВалFieldSpecified;
			set => курсВалFieldSpecified = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвСчФактДопСвФХЖ1
	{

		private ФайлДокументСвСчФактДопСвФХЖ1ВидОбяз[] видОбязField;

		private ФайлДокументСвСчФактДопСвФХЖ1ИнфПродЗаГоскКазн инфПродЗаГоскКазнField;

		private УчастникТип свФакторField;

		private РеквДокТип оснУстДенТребField;

		private РеквДокТип[] сопрДокФХЖField;

		private string идГосКонField;

		private ФайлДокументСвСчФактДопСвФХЖ1СпОбстФСЧФ спОбстФСЧФField;

		private bool спОбстФСЧФFieldSpecified;

		private string спОбстФСЧФДОПField;

		private string спОбстФДОПField;

		/// <remarks/>
		[XmlElement("ВидОбяз")]
		public ФайлДокументСвСчФактДопСвФХЖ1ВидОбяз[] ВидОбяз
		{
			get => видОбязField;
			set => видОбязField = value;
		}

		/// <remarks/>
		public ФайлДокументСвСчФактДопСвФХЖ1ИнфПродЗаГоскКазн ИнфПродЗаГоскКазн
		{
			get => инфПродЗаГоскКазнField;
			set => инфПродЗаГоскКазнField = value;
		}

		/// <remarks/>
		public УчастникТип СвФактор
		{
			get => свФакторField;
			set => свФакторField = value;
		}

		/// <remarks/>
		public РеквДокТип ОснУстДенТреб
		{
			get => оснУстДенТребField;
			set => оснУстДенТребField = value;
		}

		/// <remarks/>
		[XmlElement("СопрДокФХЖ")]
		public РеквДокТип[] СопрДокФХЖ
		{
			get => сопрДокФХЖField;
			set => сопрДокФХЖField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИдГосКон
		{
			get => идГосКонField;
			set => идГосКонField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public ФайлДокументСвСчФактДопСвФХЖ1СпОбстФСЧФ СпОбстФСЧФ
		{
			get => спОбстФСЧФField;
			set => спОбстФСЧФField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool СпОбстФСЧФSpecified
		{
			get => спОбстФСЧФFieldSpecified;
			set => спОбстФСЧФFieldSpecified = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string СпОбстФСЧФДОП
		{
			get => спОбстФСЧФДОПField;
			set => спОбстФСЧФДОПField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string СпОбстФДОП
		{
			get => спОбстФДОПField;
			set => спОбстФДОПField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвСчФактДопСвФХЖ1ВидОбяз
	{

		private string кодВидОбязField;

		private string наимВидОбязField;

		/// <remarks/>
		[XmlAttribute()]
		public string КодВидОбяз
		{
			get => кодВидОбязField;
			set => кодВидОбязField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаимВидОбяз
		{
			get => наимВидОбязField;
			set => наимВидОбязField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвСчФактДопСвФХЖ1ИнфПродЗаГоскКазн
	{

		private string датаГосКонтField;

		private string номерГосКонтField;

		private string лицСчетПродField;

		private string кодПродБюджКлассField;

		private string кодЦелиПродField;

		private string кодКазначПродField;

		private string наимКазначПродField;

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаГосКонт
		{
			get => датаГосКонтField;
			set => датаГосКонтField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НомерГосКонт
		{
			get => номерГосКонтField;
			set => номерГосКонтField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ЛицСчетПрод
		{
			get => лицСчетПродField;
			set => лицСчетПродField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КодПродБюджКласс
		{
			get => кодПродБюджКлассField;
			set => кодПродБюджКлассField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КодЦелиПрод
		{
			get => кодЦелиПродField;
			set => кодЦелиПродField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КодКазначПрод
		{
			get => кодКазначПродField;
			set => кодКазначПродField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаимКазначПрод
		{
			get => наимКазначПродField;
			set => наимКазначПродField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ФайлДокументСвСчФактДопСвФХЖ1СпОбстФСЧФ
	{

		/// <remarks/>
		[XmlEnum("1")]
		Item1,

		/// <remarks/>
		[XmlEnum("2")]
		Item2,

		/// <remarks/>
		[XmlEnum("3")]
		Item3,

		/// <remarks/>
		[XmlEnum("4")]
		Item4,

		/// <remarks/>
		[XmlEnum("5")]
		Item5,

		/// <remarks/>
		[XmlEnum("6")]
		Item6,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвСчФактИнфПолФХЖ1
	{

		private TextInformation[] текстИнфField;

		private string идФайлИнфПолField;

		/// <remarks/>
		[XmlElement("ТекстИнф")]
		public TextInformation[] ТекстИнф
		{
			get => текстИнфField;
			set => текстИнфField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИдФайлИнфПол
		{
			get => идФайлИнфПолField;
			set => идФайлИнфПолField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументТаблСчФакт
	{

		private ФайлДокументТаблСчФактСведТов[] сведТовField;

		private ФайлДокументТаблСчФактВсегоОпл всегоОплField;

		/// <remarks/>
		[XmlElement("СведТов")]
		public ФайлДокументТаблСчФактСведТов[] СведТов
		{
			get => сведТовField;
			set => сведТовField = value;
		}

		/// <remarks/>
		public ФайлДокументТаблСчФактВсегоОпл ВсегоОпл
		{
			get => всегоОплField;
			set => всегоОплField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументТаблСчФактСведТов
	{

		private ФайлДокументТаблСчФактСведТовСвДТ[] свДТField;

		private ФайлДокументТаблСчФактСведТовДопСведТов допСведТовField;

		private СумАкцизТип акцизField;

		private СумНДСТип сумНалField;

		private TextInformation[] инфПолФХЖ2Field;

		private string номСтрField;

		private string наимТовField;

		private string оКЕИ_ТовField;

		private string наимЕдИзмField;

		private decimal колТовField;

		private bool колТовFieldSpecified;

		private decimal ценаТовField;

		private bool ценаТовFieldSpecified;

		private decimal стТовБезНДСField;

		private bool стТовБезНДСFieldSpecified;

		private ФайлДокументТаблСчФактСведТовНалСт налСтField;

		private decimal стТовУчНалField;

		private bool стТовУчНалFieldSpecified;

		/// <remarks/>
		[XmlElement("СвДТ")]
		public ФайлДокументТаблСчФактСведТовСвДТ[] СвДТ
		{
			get => свДТField;
			set => свДТField = value;
		}

		/// <remarks/>
		public ФайлДокументТаблСчФактСведТовДопСведТов ДопСведТов
		{
			get => допСведТовField;
			set => допСведТовField = value;
		}

		/// <remarks/>
		public СумАкцизТип Акциз
		{
			get => акцизField;
			set => акцизField = value;
		}

		/// <remarks/>
		public СумНДСТип СумНал
		{
			get => сумНалField;
			set => сумНалField = value;
		}

		/// <remarks/>
		[XmlElement("ИнфПолФХЖ2")]
		public TextInformation[] ИнфПолФХЖ2
		{
			get => инфПолФХЖ2Field;
			set => инфПолФХЖ2Field = value;
		}

		/// <remarks/>
		[XmlAttribute(DataType = "integer")]
		public string НомСтр
		{
			get => номСтрField;
			set => номСтрField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаимТов
		{
			get => наимТовField;
			set => наимТовField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ОКЕИ_Тов
		{
			get => оКЕИ_ТовField;
			set => оКЕИ_ТовField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаимЕдИзм
		{
			get => наимЕдИзмField;
			set => наимЕдИзмField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal КолТов
		{
			get => колТовField;
			set => колТовField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool КолТовSpecified
		{
			get => колТовFieldSpecified;
			set => колТовFieldSpecified = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal ЦенаТов
		{
			get => ценаТовField;
			set => ценаТовField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool ЦенаТовSpecified
		{
			get => ценаТовFieldSpecified;
			set => ценаТовFieldSpecified = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal СтТовБезНДС
		{
			get => стТовБезНДСField;
			set => стТовБезНДСField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool СтТовБезНДСSpecified
		{
			get => стТовБезНДСFieldSpecified;
			set => стТовБезНДСFieldSpecified = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public ФайлДокументТаблСчФактСведТовНалСт НалСт
		{
			get => налСтField;
			set => налСтField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal СтТовУчНал
		{
			get => стТовУчНалField;
			set => стТовУчНалField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool СтТовУчНалSpecified
		{
			get => стТовУчНалFieldSpecified;
			set => стТовУчНалFieldSpecified = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументТаблСчФактСведТовСвДТ
	{

		private string кодПроисхField;

		private string номерДТField;

		/// <remarks/>
		[XmlAttribute()]
		public string КодПроисх
		{
			get => кодПроисхField;
			set => кодПроисхField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НомерДТ
		{
			get => номерДТField;
			set => номерДТField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументТаблСчФактСведТовДопСведТов
	{

		private string[] крНаимСтрПрField;

		private РеквДокТип[] сопрДокТовField;

		private ФайлДокументТаблСчФактСведТовДопСведТовНалУчАморт налУчАмортField;

		private СумНДСТип сумНалВосстField;

		private ФайлДокументТаблСчФактСведТовДопСведТовСведПрослеж[] сведПрослежField;

		private ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов[] номСредИдентТовField;

		private ФайлДокументТаблСчФактСведТовДопСведТовСвГосСист[] свГосСистField;

		private ФайлДокументТаблСчФактСведТовДопСведТовПрТовРаб прТовРабField;

		private bool прТовРабFieldSpecified;

		private string допПризнField;

		private decimal надлОтпField;

		private bool надлОтпFieldSpecified;

		private string характерТовField;

		private string сортТовField;

		private string серияТовField;

		private string артикулТовField;

		private string кодТовField;

		private string гТИНField;

		private string кодКатField;

		private string кодВидТовField;

		private string кодВидПрField;

		private string кодТовОКПД2Field;

		private string допИнфПВидОField;

		/// <remarks/>
		[XmlElement("КрНаимСтрПр")]
		public string[] КрНаимСтрПр
		{
			get => крНаимСтрПрField;
			set => крНаимСтрПрField = value;
		}

		/// <remarks/>
		[XmlElement("СопрДокТов")]
		public РеквДокТип[] СопрДокТов
		{
			get => сопрДокТовField;
			set => сопрДокТовField = value;
		}

		/// <remarks/>
		public ФайлДокументТаблСчФактСведТовДопСведТовНалУчАморт НалУчАморт
		{
			get => налУчАмортField;
			set => налУчАмортField = value;
		}

		/// <remarks/>
		public СумНДСТип СумНалВосст
		{
			get => сумНалВосстField;
			set => сумНалВосстField = value;
		}

		/// <remarks/>
		[XmlElement("СведПрослеж")]
		public ФайлДокументТаблСчФактСведТовДопСведТовСведПрослеж[] СведПрослеж
		{
			get => сведПрослежField;
			set => сведПрослежField = value;
		}

		/// <remarks/>
		[XmlElement("НомСредИдентТов")]
		public ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов[] НомСредИдентТов
		{
			get => номСредИдентТовField;
			set => номСредИдентТовField = value;
		}

		/// <remarks/>
		[XmlElement("СвГосСист")]
		public ФайлДокументТаблСчФактСведТовДопСведТовСвГосСист[] СвГосСист
		{
			get => свГосСистField;
			set => свГосСистField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public ФайлДокументТаблСчФактСведТовДопСведТовПрТовРаб ПрТовРаб
		{
			get => прТовРабField;
			set => прТовРабField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool ПрТовРабSpecified
		{
			get => прТовРабFieldSpecified;
			set => прТовРабFieldSpecified = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДопПризн
		{
			get => допПризнField;
			set => допПризнField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal НадлОтп
		{
			get => надлОтпField;
			set => надлОтпField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool НадлОтпSpecified
		{
			get => надлОтпFieldSpecified;
			set => надлОтпFieldSpecified = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ХарактерТов
		{
			get => характерТовField;
			set => характерТовField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string СортТов
		{
			get => сортТовField;
			set => сортТовField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string СерияТов
		{
			get => серияТовField;
			set => серияТовField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string АртикулТов
		{
			get => артикулТовField;
			set => артикулТовField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КодТов
		{
			get => кодТовField;
			set => кодТовField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ГТИН
		{
			get => гТИНField;
			set => гТИНField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КодКат
		{
			get => кодКатField;
			set => кодКатField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КодВидТов
		{
			get => кодВидТовField;
			set => кодВидТовField = value;
		}

		/// <remarks/>
		[XmlAttribute(DataType = "integer")]
		public string КодВидПр
		{
			get => кодВидПрField;
			set => кодВидПрField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КодТовОКПД2
		{
			get => кодТовОКПД2Field;
			set => кодТовОКПД2Field = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДопИнфПВидО
		{
			get => допИнфПВидОField;
			set => допИнфПВидОField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументТаблСчФактСведТовДопСведТовНалУчАморт
	{

		private string амГруппаField;

		private string кодОКОФField;

		private string срПолИспОСField;

		private string фактСрокИспField;

		/// <remarks/>
		[XmlAttribute()]
		public string АмГруппа
		{
			get => амГруппаField;
			set => амГруппаField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string КодОКОФ
		{
			get => кодОКОФField;
			set => кодОКОФField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string СрПолИспОС
		{
			get => срПолИспОСField;
			set => срПолИспОСField = value;
		}

		/// <remarks/>
		[XmlAttribute(DataType = "integer")]
		public string ФактСрокИсп
		{
			get => фактСрокИспField;
			set => фактСрокИспField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументТаблСчФактСведТовДопСведТовСведПрослеж
	{

		private string номТовПрослежField;

		private string едИзмПрослежField;

		private string наимЕдИзмПрослежField;

		private decimal колВЕдПрослежField;

		private decimal стТовБезНДСПрослежField;

		private string допИнфПрослежField;

		/// <remarks/>
		[XmlAttribute()]
		public string НомТовПрослеж
		{
			get => номТовПрослежField;
			set => номТовПрослежField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ЕдИзмПрослеж
		{
			get => едИзмПрослежField;
			set => едИзмПрослежField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаимЕдИзмПрослеж
		{
			get => наимЕдИзмПрослежField;
			set => наимЕдИзмПрослежField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal КолВЕдПрослеж
		{
			get => колВЕдПрослежField;
			set => колВЕдПрослежField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal СтТовБезНДСПрослеж
		{
			get => стТовБезНДСПрослежField;
			set => стТовБезНДСПрослежField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДопИнфПрослеж
		{
			get => допИнфПрослежField;
			set => допИнфПрослежField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументТаблСчФактСведТовДопСведТовНомСредИдентТов
	{

		private string[] itemsField;

		private ItemsChoiceType[] itemsElementNameField;

		private string идентТрансУпакField;

		private string колВедМаркField;

		private string прПартМаркField;

		/// <remarks/>
		[XmlElement("КИЗ", typeof(string))]
		[XmlElement("НомУпак", typeof(string))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public string[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		/// <remarks/>
		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИдентТрансУпак
		{
			get => идентТрансУпакField;
			set => идентТрансУпакField = value;
		}

		/// <remarks/>
		[XmlAttribute(DataType = "integer")]
		public string КолВедМарк
		{
			get => колВедМаркField;
			set => колВедМаркField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ПрПартМарк
		{
			get => прПартМаркField;
			set => прПартМаркField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType
	{

		/// <remarks/>
		КИЗ,

		/// <remarks/>
		НомУпак,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументТаблСчФактСведТовДопСведТовСвГосСист
	{

		private string[] идНомУчетЕдField;

		private string наимГосСистField;

		private string учетЕдField;

		private string инаяИнфField;

		/// <remarks/>
		[XmlElement("ИдНомУчетЕд")]
		public string[] ИдНомУчетЕд
		{
			get => идНомУчетЕдField;
			set => идНомУчетЕдField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаимГосСист
		{
			get => наимГосСистField;
			set => наимГосСистField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string УчетЕд
		{
			get => учетЕдField;
			set => учетЕдField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИнаяИнф
		{
			get => инаяИнфField;
			set => инаяИнфField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ФайлДокументТаблСчФактСведТовДопСведТовПрТовРаб
	{

		/// <remarks/>
		[XmlEnum("1")]
		Item1,

		/// <remarks/>
		[XmlEnum("2")]
		Item2,

		/// <remarks/>
		[XmlEnum("3")]
		Item3,

		/// <remarks/>
		[XmlEnum("4")]
		Item4,

		/// <remarks/>
		[XmlEnum("5")]
		Item5,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ФайлДокументТаблСчФактСведТовНалСт
	{
		/// <remarks/>
		[XmlEnum("0%")]
		Item0,

		/// <remarks/>
		[XmlEnum("5%")]
		Item5,

		/// <remarks/>
		[XmlEnum("7%")]
		Item7,

		/// <remarks/>
		[XmlEnum("9,09%")]
		Item909,

		/// <remarks/>
		[XmlEnum("10%")]
		Item10,

		/// <remarks/>
		[XmlEnum("16,67%")]
		Item1667,

		/// <remarks/>
		[XmlEnum("18%")]
		Item18,

		/// <remarks/>
		[XmlEnum("18,03%")]
		Item1803,

		/// <remarks/>
		[XmlEnum("20%")]
		Item20,

		/// <remarks/>
		[XmlEnum("22%")]
		Item22,

		/// <remarks/>
		[XmlEnum("5/105")]
		Item5105,

		/// <remarks/>
		[XmlEnum("7/107")]
		Item7107,

		/// <remarks/>
		[XmlEnum("10/110")]
		Item10110,

		/// <remarks/>
		[XmlEnum("18/118")]
		Item18118,

		/// <remarks/>
		[XmlEnum("20/120")]
		Item20120,

		/// <remarks/>
		[XmlEnum("22/122")]
		Item22122,

		/// <remarks/>
		[XmlEnum("без НДС")]
		безНДС,

		/// <remarks/>
		[XmlEnum("НДС исчисляется налоговым агентом")]
		НДСисчисляетсяналоговымагентом,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументТаблСчФактВсегоОпл
	{

		private СумНДСТип сумНалВсегоField;

		private decimal стТовБезНДСВсегоField;

		private bool стТовБезНДСВсегоFieldSpecified;

		private decimal стТовУчНалВсегоField;

		private bool стТовУчНалВсегоFieldSpecified;

		private decimal колНеттоВсField;

		private bool колНеттоВсFieldSpecified;

		/// <remarks/>
		public СумНДСТип СумНалВсего
		{
			get => сумНалВсегоField;
			set => сумНалВсегоField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal СтТовБезНДСВсего
		{
			get => стТовБезНДСВсегоField;
			set => стТовБезНДСВсегоField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool СтТовБезНДСВсегоSpecified
		{
			get => стТовБезНДСВсегоFieldSpecified;
			set => стТовБезНДСВсегоFieldSpecified = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal СтТовУчНалВсего
		{
			get => стТовУчНалВсегоField;
			set => стТовУчНалВсегоField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool СтТовУчНалВсегоSpecified
		{
			get => стТовУчНалВсегоFieldSpecified;
			set => стТовУчНалВсегоFieldSpecified = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public decimal КолНеттоВс
		{
			get => колНеттоВсField;
			set => колНеттоВсField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool КолНеттоВсSpecified
		{
			get => колНеттоВсFieldSpecified;
			set => колНеттоВсFieldSpecified = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвПродПер
	{

		private ФайлДокументСвПродПерСвПер свПерField;

		private ФайлДокументСвПродПерИнфПолФХЖ3 инфПолФХЖ3Field;

		/// <remarks/>
		public ФайлДокументСвПродПерСвПер СвПер
		{
			get => свПерField;
			set => свПерField = value;
		}

		/// <remarks/>
		public ФайлДокументСвПродПерИнфПолФХЖ3 ИнфПолФХЖ3
		{
			get => инфПолФХЖ3Field;
			set => инфПолФХЖ3Field = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвПродПерСвПер
	{

		private object[] itemsField;

		private ФайлДокументСвПродПерСвПерСвЛицПер свЛицПерField;

		private ФайлДокументСвПродПерСвПерТран транField;

		private ФайлДокументСвПродПерСвПерСвПерВещи свПерВещиField;

		private string содОперField;

		private string видОперField;

		private string датаПерField;

		private string датаНачПерField;

		private string датаОконПерField;

		/// <remarks/>
		[XmlElement("БезДокОснПер", typeof(ФайлДокументСвПродПерСвПерБезДокОснПер))]
		[XmlElement("ОснПер", typeof(РеквДокТип))]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		/// <remarks/>
		public ФайлДокументСвПродПерСвПерСвЛицПер СвЛицПер
		{
			get => свЛицПерField;
			set => свЛицПерField = value;
		}

		/// <remarks/>
		public ФайлДокументСвПродПерСвПерТран Тран
		{
			get => транField;
			set => транField = value;
		}

		/// <remarks/>
		public ФайлДокументСвПродПерСвПерСвПерВещи СвПерВещи
		{
			get => свПерВещиField;
			set => свПерВещиField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string СодОпер
		{
			get => содОперField;
			set => содОперField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ВидОпер
		{
			get => видОперField;
			set => видОперField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаПер
		{
			get => датаПерField;
			set => датаПерField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаНачПер
		{
			get => датаНачПерField;
			set => датаНачПерField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаОконПер
		{
			get => датаОконПерField;
			set => датаОконПерField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ФайлДокументСвПродПерСвПерБезДокОснПер
	{

		/// <remarks/>
		[XmlEnum("1")]
		Item1,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвПродПерСвПерСвЛицПер
	{

		private object itemField;

		/// <remarks/>
		[XmlElement("ИнЛицо", typeof(ФайлДокументСвПродПерСвПерСвЛицПерИнЛицо))]
		[XmlElement("РабОргПрод", typeof(ФайлДокументСвПродПерСвПерСвЛицПерРабОргПрод))]
		public object Item
		{
			get => itemField;
			set => itemField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвПродПерСвПерСвЛицПерИнЛицо
	{

		private object itemField;

		/// <remarks/>
		[XmlElement("ПредОргПер", typeof(ФайлДокументСвПродПерСвПерСвЛицПерИнЛицоПредОргПер))]
		[XmlElement("ФЛПер", typeof(ФайлДокументСвПродПерСвПерСвЛицПерИнЛицоФЛПер))]
		public object Item
		{
			get => itemField;
			set => itemField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвПродПерСвПерСвЛицПерИнЛицоПредОргПер
	{

		private РеквДокТип оснДоверОргПерField;

		private РеквДокТип оснПолнПредПерField;

		private FullName фИОField;

		private string должностьField;

		private string иныеСведField;

		private string наимОргПерField;

		private string иННЮЛПерField;

		/// <remarks/>
		public РеквДокТип ОснДоверОргПер
		{
			get => оснДоверОргПерField;
			set => оснДоверОргПерField = value;
		}

		/// <remarks/>
		public РеквДокТип ОснПолнПредПер
		{
			get => оснПолнПредПерField;
			set => оснПолнПредПерField = value;
		}

		/// <remarks/>
		public FullName ФИО
		{
			get => фИОField;
			set => фИОField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Должность
		{
			get => должностьField;
			set => должностьField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИныеСвед
		{
			get => иныеСведField;
			set => иныеСведField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string НаимОргПер
		{
			get => наимОргПерField;
			set => наимОргПерField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИННЮЛПер
		{
			get => иННЮЛПерField;
			set => иННЮЛПерField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвПродПерСвПерСвЛицПерИнЛицоФЛПер
	{

		private РеквДокТип оснДоверФЛField;

		private FullName фИОField;

		private string иННФЛПерField;

		private string иныеСведField;

		/// <remarks/>
		public РеквДокТип ОснДоверФЛ
		{
			get => оснДоверФЛField;
			set => оснДоверФЛField = value;
		}

		/// <remarks/>
		public FullName ФИО
		{
			get => фИОField;
			set => фИОField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИННФЛПер
		{
			get => иННФЛПерField;
			set => иННФЛПерField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИныеСвед
		{
			get => иныеСведField;
			set => иныеСведField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвПродПерСвПерСвЛицПерРабОргПрод
	{

		private FullName фИОField;

		private string должностьField;

		private string иныеСведField;

		/// <remarks/>
		public FullName ФИО
		{
			get => фИОField;
			set => фИОField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Должность
		{
			get => должностьField;
			set => должностьField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИныеСвед
		{
			get => иныеСведField;
			set => иныеСведField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвПродПерСвПерТран
	{

		private string свТранField;

		private string инкотермсField;

		private string верИнкотермсField;

		/// <remarks/>
		[XmlAttribute()]
		public string СвТран
		{
			get => свТранField;
			set => свТранField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Инкотермс
		{
			get => инкотермсField;
			set => инкотермсField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ВерИнкотермс
		{
			get => верИнкотермсField;
			set => верИнкотермсField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвПродПерСвПерСвПерВещи
	{

		private РеквДокТип докПерВещField;

		private string датаПерВещField;

		private string свПерВещField;

		/// <remarks/>
		public РеквДокТип ДокПерВещ
		{
			get => докПерВещField;
			set => докПерВещField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаПерВещ
		{
			get => датаПерВещField;
			set => датаПерВещField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string СвПерВещ
		{
			get => свПерВещField;
			set => свПерВещField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументСвПродПерИнфПолФХЖ3
	{

		private TextInformation[] текстИнфField;

		private string идФайлИнфПолField;

		/// <remarks/>
		[XmlElement("ТекстИнф")]
		public TextInformation[] ТекстИнф
		{
			get => текстИнфField;
			set => текстИнфField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИдФайлИнфПол
		{
			get => идФайлИнфПолField;
			set => идФайлИнфПолField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументПодписант
	{
		private FullName фИОField;

		private object[] itemsField;

		private string должнField;

		private ФайлДокументПодписантТипПодпис типПодписField;

		private bool типПодписFieldSpecified;

		private string датаПодДокField;

		private ФайлДокументПодписантСпосПодтПолном спосПодтПолномField;

		private string допСведПодпField;

		/// <remarks/>
		public FullName ФИО
		{
			get => фИОField;
			set => фИОField = value;
		}

		/// <remarks/>
		[XmlElement("СвДоверБум", typeof(ФайлДокументПодписантСвДоверБум))]
		[XmlElement("СвДоверЭл", typeof(ФайлДокументПодписантСвДоверЭл))]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string Должн
		{
			get => должнField;
			set => должнField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public ФайлДокументПодписантТипПодпис ТипПодпис
		{
			get => типПодписField;
			set => типПодписField = value;
		}

		/// <remarks/>
		[XmlIgnore()]
		public bool ТипПодписSpecified
		{
			get => типПодписFieldSpecified;
			set => типПодписFieldSpecified = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаПодДок
		{
			get => датаПодДокField;
			set => датаПодДокField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public ФайлДокументПодписантСпосПодтПолном СпосПодтПолном
		{
			get => спосПодтПолномField;
			set => спосПодтПолномField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДопСведПодп
		{
			get => допСведПодпField;
			set => допСведПодпField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументПодписантСвДоверБум
	{

		private FullName фИОField;

		private string датаВыдДоверField;

		private string внНомДоверField;

		private string свИдДоверField;

		/// <remarks/>
		public FullName ФИО
		{
			get => фИОField;
			set => фИОField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаВыдДовер
		{
			get => датаВыдДоверField;
			set => датаВыдДоверField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ВнНомДовер
		{
			get => внНомДоверField;
			set => внНомДоверField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string СвИдДовер
		{
			get => свИдДоверField;
			set => свИдДоверField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[DebuggerStepThrough()]
	[DesignerCategory("code")]
	[XmlType(AnonymousType = true)]
	public partial class ФайлДокументПодписантСвДоверЭл
	{

		private string номДоверField;

		private string датаВыдДоверField;

		private string внНомДоверField;

		private string датаВнРегДоверField;

		private string идСистХранField;

		private string уРЛСистField;

		/// <remarks/>
		[XmlAttribute()]
		public string НомДовер
		{
			get => номДоверField;
			set => номДоверField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаВыдДовер
		{
			get => датаВыдДоверField;
			set => датаВыдДоверField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ВнНомДовер
		{
			get => внНомДоверField;
			set => внНомДоверField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ДатаВнРегДовер
		{
			get => датаВнРегДоверField;
			set => датаВнРегДоверField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string ИдСистХран
		{
			get => идСистХранField;
			set => идСистХранField = value;
		}

		/// <remarks/>
		[XmlAttribute()]
		public string УРЛСист
		{
			get => уРЛСистField;
			set => уРЛСистField = value;
		}
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ФайлДокументПодписантТипПодпис
	{

		/// <remarks/>
		[XmlEnum("1")]
		Item1,

		/// <remarks/>
		[XmlEnum("2")]
		Item2,

		/// <remarks/>
		[XmlEnum("3")]
		Item3,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ФайлДокументПодписантСпосПодтПолном
	{

		/// <remarks/>
		[XmlEnum("1")]
		Item1,

		/// <remarks/>
		[XmlEnum("2")]
		Item2,

		/// <remarks/>
		[XmlEnum("3")]
		Item3,

		/// <remarks/>
		[XmlEnum("4")]
		Item4,

		/// <remarks/>
		[XmlEnum("5")]
		Item5,

		/// <remarks/>
		[XmlEnum("6")]
		Item6,
	}

	/// <remarks/>
	[GeneratedCode("xsd", "4.8.3928.0")]
	[Serializable()]
	[XmlType(AnonymousType = true)]
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
}
