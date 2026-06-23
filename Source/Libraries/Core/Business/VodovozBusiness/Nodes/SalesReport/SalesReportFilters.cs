using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Nodes.SalesReport
{
	public class SalesReportFilters
	{
		/// <summary>
		/// Включаемые номенклатуры
		/// </summary>
		public int[] NomenclatureInclude { get; set; }

		/// <summary>
		/// Исключаемые номенклатуры
		/// </summary>
		public int[] NomenclatureExclude { get; set; }

		/// <summary>
		/// Включаемые категории номенклатуры
		/// </summary>
		public NomenclatureCategory[] NomenclatureCategoryInclude { get; set; }

		/// <summary>
		/// Исключаемые категории номенклатуры
		/// </summary>
		public NomenclatureCategory[] NomenclatureCategoryExclude { get; set; }

		/// <summary>
		/// Включаемые контрагенты
		/// </summary>
		public int[] CounterpartyInclude { get; set; }

		/// <summary>
		/// Исключаемые контрагенты
		/// </summary>
		public int[] CounterpartyExclude { get; set; }

		/// <summary>
		/// Включаемые организации
		/// </summary>
		public int[] OrganizationInclude { get; set; }

		/// <summary>
		/// Исключаемые организации
		/// </summary>
		public int[] OrganizationExclude { get; set; }

		/// <summary>
		/// Включаемые авторы заказа
		/// </summary>
		public int[] OrderAuthorInclude { get; set; }

		/// <summary>
		/// Исключаемые авторы заказа
		/// </summary>
		public int[] OrderAuthorExclude { get; set; }

		/// <summary>
		/// Включаемые менеджеры по продажам
		/// </summary>
		public int[] SalesManagerInclude { get; set; }

		/// <summary>
		/// Исключаемые менеджеры по продажам
		/// </summary>
		public int[] SalesManagerExclude { get; set; }

		/// <summary>
		/// Включаемые подразделения автора
		/// </summary>
		public int[] SubdivisionInclude { get; set; }

		/// <summary>
		/// Исключаемые подразделения автора
		/// </summary>
		public int[] SubdivisionExclude { get; set; }

		/// <summary>
		/// Включаемые гео-группы
		/// </summary>
		public int[] GeoGroupInclude { get; set; }

		/// <summary>
		/// Исключаемые гео-группы
		/// </summary>
		public int[] GeoGroupExclude { get; set; }

		/// <summary>
		/// Включаемые типы оплаты
		/// </summary>
		public PaymentType[] PaymentTypeInclude { get; set; }

		/// <summary>
		/// Исключаемые типы оплаты
		/// </summary>
		public PaymentType[] PaymentTypeExclude { get; set; }

		/// <summary>
		/// Включаемые промонаборы
		/// </summary>
		public int[] PromotionalSetInclude { get; set; }

		/// <summary>
		/// Исключаемые промонаборы
		/// </summary>
		public int[] PromotionalSetExclude { get; set; }

		/// <summary>
		/// Включаемые группы товаров
		/// </summary>
		public int[] ProductGroupInclude { get; set; }

		/// <summary>
		/// Исключаемые группы товаров
		/// </summary>
		public int[] ProductGroupExclude { get; set; }

		/// <summary>
		/// Включаемые источники оплаты по терминалу
		/// </summary>
		public PaymentByTerminalSource[] PaymentByTerminalSourceInclude { get; set; }

		/// <summary>
		/// Исключаемые источники оплаты по терминалу
		/// </summary>
		public PaymentByTerminalSource[] PaymentByTerminalSourceExclude { get; set; }

		/// <summary>
		/// Включаемые источники онлайн оплаты
		/// </summary>
		public int[] PaymentFromInclude { get; set; }

		/// <summary>
		/// Исключаемые источники онлайн оплаты
		/// </summary>
		public int[] PaymentFromExclude { get; set; }

		/// <summary>
		/// Включаемые статусы заказа
		/// </summary>
		public OrderStatus[] OrderStatusInclude { get; set; }

		/// <summary>
		/// Исключаемые статусы заказа
		/// </summary>
		public OrderStatus[] OrderStatusExclude { get; set; }

		/// <summary>
		/// Включаемые типы контрагента
		/// </summary>
		public CounterpartyType[] CounterpartyTypeInclude { get; set; }

		/// <summary>
		/// Исключаемые типы контрагента
		/// </summary>
		public CounterpartyType[] CounterpartyTypeExclude { get; set; }

		/// <summary>
		/// Включаемые подтипы контрагента (для Клиентов РО)
		/// </summary>
		public int[] CounterpartySubtypeInclude { get; set; }

		/// <summary>
		/// Исключаемые подтипы контрагента
		/// </summary>
		public int[] CounterpartySubtypeExclude { get; set; }

		/// <summary>
		/// Включаемые композитные классификации контрагентов
		/// </summary>
		public CounterpartyCompositeClassification[] CounterpartyCompositeClassificationInclude { get; set; }

		/// <summary>
		/// Исключаемые композитные классификации контрагентов
		/// </summary>
		public CounterpartyCompositeClassification[] CounterpartyCompositeClassificationExclude { get; set; }

		/// <summary>
		/// Включаемые причины скидок
		/// </summary>
		public int[] DiscountReasonInclude { get; set; }

		/// <summary>
		/// Исключаемые причины скидок
		/// </summary>
		public int[] DiscountReasonExclude { get; set; }

		/// <summary>
		/// Фильтр по самовывозу (true - только самовывоз, false - только доставка, null - все)
		/// </summary>
		public bool? IsSelfDelivery { get; set; }

		/// <summary>
		/// Фильтр по наличию кассовых чеков (true - только с чеками, false - только без чеков, null - все)
		/// </summary>
		public bool? OnlyWithCashReceipts { get; set; }

		/// <summary>
		/// Фильтр по заказам в маршрутных листах (true - только в МЛ без фур, false - только не в МЛ, null - все)
		/// </summary>
		public bool? OnlyOrdersFromRouteLists { get; set; }
	}
}
