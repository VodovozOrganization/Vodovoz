using System;

namespace Vodovoz.ExportTo1c
{
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
		public static readonly string EnumInvoiceType = "ПеречислениеСсылка.ВидСчетаФактурыВыставленного";
		public static readonly string EnumVAT = "ПеречислениеСсылка.СтавкиНДС";
		public static readonly string EnumVATTypes = "ПеречислениеСсылка.ВидыСтавокНДС";
		public static readonly string EnumContractType = "ПеречислениеСсылка.ВидыДоговоровКонтрагентов";
		public static readonly string ReferenceAccount = "СправочникСсылка.БанковскиеСчета";
		public static readonly string ReferenceCountry = "СправочникСсылка.СтраныМира";
		public static readonly string ReferenceBank = "СправочникСсылка.Банки";
		public static readonly string ReferenceContract = "СправочникСсылка.ДоговорыКонтрагентов";
		public static readonly string ReferenceOrganization = "СправочникСсылка.Организации";
		public static readonly string ReferenceCounterparty = "СправочникСсылка.Контрагенты";
		public static readonly string ReferenceCurrency = "СправочникСсылка.Валюты";
		public static readonly string ReferenceNomenclature = "СправочникСсылка.Номенклатура";
		public static readonly string ReferencePriceType = "СправочникСсылка.ТипыЦенНоменклатуры";
		public static readonly string ReferenceWarehouse = "СправочникСсылка.Склады";
		public static readonly string ReferenceMeasurementUnit = "СправочникСсылка.КлассификаторЕдиницИзмерения";
		public static readonly string ReferenceNomenclatureType = "СправочникСсылка.ВидыНоменклатуры";
	}
}

