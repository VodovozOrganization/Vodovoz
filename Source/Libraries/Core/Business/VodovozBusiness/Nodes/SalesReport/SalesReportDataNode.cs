using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Gamma.Utilities;

namespace VodovozBusiness.Nodes.SalesReport
{
	public class SalesReportDataNode
	{
		/// <summary>
		/// Идентификатор контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Наименование контрагента
		/// </summary>
		public string Counterparty { get; set; }

		/// <summary>
		/// Тип контрагента
		/// </summary>
		public CounterpartyType CounterpartyType { get; set; }

		/// <summary>
		/// Наименование организации, от которой оформлен заказ
		/// </summary>
		public string Organization { get; set; }

		/// <summary>
		/// Адрес точки доставки
		/// </summary>
		public string DeliveryPoint { get; set; }

		/// <summary>
		/// Телефоны контрагента или точки доставки
		/// </summary>
		public string Phone { get; set; }

		/// <summary>
		/// Идентификатор заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Тип оплаты заказа
		/// </summary>
		public PaymentType PaymentType { get; set; }

		/// <summary>
		/// Дата доставки заказа
		/// </summary>
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// Идентификатор маршрутного листа
		/// </summary>
		public int? RouteList { get; set; }

		/// <summary>
		/// Идентификатор позиции заказа
		/// </summary>
		public int OrderItemId { get; set; }

		/// <summary>
		/// Официальное наименование номенклатуры
		/// </summary>
		public string NomenclatureName { get; set; }

		/// <summary>
		/// Категория номенклатуры
		/// </summary>
		public NomenclatureCategory NomenclatureCategory { get; set; }

		/// <summary>
		/// Краткое ФИО автора заказа
		/// </summary>
		public string OrderAuthor { get; set; }

		/// <summary>
		/// Суммарное количество единиц товара (с учетом фактического или планового)
		/// </summary>
		public decimal TotalCount { get; set; }

		/// <summary>
		/// Суммарная стоимость товара (с учетом скидок)
		/// </summary>
		public decimal TotalSum { get; set; }

		/// <summary>
		/// Признак одноразовой тары для воды
		/// </summary>
		public bool IsDisposableTare { get; set; }

		/// <summary>
		/// Идентификатор номенклатуры
		/// </summary>
		public int NomenclatureId { get; set; }

		/// <summary>
		/// Наименование подразделения автора заказа
		/// </summary>
		public string AuthorSubdivision { get; set; }

		/// <summary>
		/// Идентификатор подразделения автора заказа
		/// </summary>
		public int AuthorSubdivisionId { get; set; }

		/// <summary>
		/// Количество знаков после запятой для единицы измерения номенклатуры
		/// </summary>
		public uint Digits { get; set; }

		/// <summary>
		/// Наименование группы номенклатуры 1-го уровня
		/// </summary>
		public string NomenGroupLevel1Name { get; set; }

		/// <summary>
		/// Наименование группы номенклатуры 2-го уровня
		/// </summary>
		public string NomenGroupLevel2Name { get; set; }

		/// <summary>
		/// Наименование группы номенклатуры 3-го уровня
		/// </summary>
		public string NomenGroupLevel3Name { get; set; }

		/// <summary>
		/// Идентификатор группы номенклатуры 1-го уровня
		/// </summary>
		public int NomenGroupLevel1Id { get; set; }

		/// <summary>
		/// Идентификатор группы номенклатуры 2-го уровня
		/// </summary>
		public int NomenGroupLevel2Id { get; set; }

		/// <summary>
		/// Идентификатор группы номенклатуры 3-го уровня
		/// </summary>
		public int NomenGroupLevel3Id { get; set; }

		/// <summary>
		/// Классификация контрагента 
		/// </summary>
		public string CounterpartyClassification { get; set; }

		/// <summary>
		/// Наименование промонабора
		/// </summary>
		public string PromotionalSet { get; set; }

		/// <summary>
		/// ФИО менеджера по продажам, закрепленного за контрагентом
		/// </summary>
		public string SalesManagerName { get; set; }

		/// <summary>
		/// Полное ФИО автора заказа
		/// </summary>
		public string OrderAuthorName { get; set; }

		public string NomenclatureCategoryString
		{
			get
			{
				switch(NomenclatureCategory)
				{
					case NomenclatureCategory.water:
						return IsDisposableTare ? "Вода в одноразовой таре" : "Вода в многооборотной таре";
					default:
						return NomenclatureCategory.GetEnumTitle();
				}
			}
		}

		/// <summary>
		/// Детали заказа в формате: "Номер заказа\nДата доставки\nКраткое ФИО автора"
		/// </summary>
		public string OrdDetails => $"{OrderId}\n{DeliveryDate:dd.MM.yyyy}\n{OrderAuthor}";
	}
}
