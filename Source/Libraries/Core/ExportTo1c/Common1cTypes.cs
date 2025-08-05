using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library
{
	/// <summary>
	/// Типы 1С
	/// </summary>
	public static class Common1cTypes
	{
		public static readonly string String = "Строка";
		public static readonly string Boolean = "Булево";
		public static readonly string Numeric = "Число";
		public static readonly string Date = "Дата";
		public static readonly string SalesDocument = "ДокументСсылка.РеализацияТоваровУслуг";
		public static readonly string RetailDocument = "ДокументСсылка.ОтчетОРозничныхПродажах";
		public static readonly string InvoiceDocument = "ДокументСсылка.СчетФактураВыданный";
		public static readonly string EnumNaturalOrLegal = "ПеречислениеСсылка.ЮридическоеФизическоеЛицо";
		public static readonly string EnumWarehouseTypes = "ПеречислениеСсылка.ТипыСкладов";
		public static readonly string EnumNomenclatureTypes = "ПеречислениеСсылка.ТипыНоменклатуры";
		public static readonly string EnumInvoiceType = "ПеречислениеСсылка.ВидСчетаФактурыВыставленного";
		public static readonly string EnumVATTypes = "ПеречислениеСсылка.ВидыСтавокНДС";

		public static string Vat(Export1cMode export1CMode) => export1CMode == Export1cMode.ComplexAutomation
			? "СправочникСсылка.СтавкиНДС"
			: "ПеречислениеСсылка.СтавкиНДС";

		public static readonly string ReferenceCountry = "СправочникСсылка.СтраныМира";

		public static string EnumContractType(Export1cMode export1CMode) => export1CMode == Export1cMode.ComplexAutomation
			? "ПеречислениеСсылка.ТипыДоговоров"
			: "ПеречислениеСсылка.ВидыДоговоровКонтрагентов";

		public static string ReferenceAccount(Export1cMode export1CMode) => export1CMode == Export1cMode.ComplexAutomation
			? "СправочникСсылка.БанковскиеСчетаКонтрагентов"
			: "СправочникСсылка.БанковскиеСчета";

		public static string ReferenceBank(Export1cMode export1CMode) => export1CMode == Export1cMode.ComplexAutomation
			? "КлассификаторБанков"
			: "СправочникСсылка.Банки";

		public static readonly string ReferenceContract = "СправочникСсылка.ДоговорыКонтрагентов";
		public static readonly string ReferenceOrganization = "СправочникСсылка.Организации";
		public static readonly string ReferenceCounterparty = "СправочникСсылка.Контрагенты";
		public static readonly string ReferenceCurrency = "СправочникСсылка.Валюты";
		public static readonly string ReferenceNomenclature = "СправочникСсылка.Номенклатура";
		public static readonly string ReferencePriceType = "СправочникСсылка.ТипыЦенНоменклатуры";
		public static readonly string ReferenceWarehouse = "СправочникСсылка.Склады";
		public static readonly string ReferenceVat = "СправочникСсылка.СтавкиНДС";

		public static string ReferenceMeasurementUnit(Export1cMode export1CMode) => export1CMode == Export1cMode.ComplexAutomation
			? "СправочникСсылка.УпаковкиЕдиницыИзмерения"
			: "СправочникСсылка.КлассификаторЕдиницИзмерения";

		public static readonly string ReferenceNomenclatureType = "СправочникСсылка.ВидыНоменклатуры";
	}
}
