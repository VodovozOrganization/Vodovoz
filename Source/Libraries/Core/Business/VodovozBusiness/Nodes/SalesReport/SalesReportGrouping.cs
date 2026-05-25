using Vodovoz.Reports.Editing.Modifiers;

namespace VodovozBusiness.Nodes.SalesReport
{
	public class SalesReportGrouping
	{
		public GroupingType Type { get; set; }

		public string GetGroupKey(SalesReportDataNode node)
		{
			switch(Type)
			{
				case GroupingType.Order:
					return $"Заказ №{node.OrderId}";
				case GroupingType.Counterparty:
					return node.Counterparty ?? "Не указан";
				case GroupingType.Subdivision:
					return node.AuthorSubdivision ?? "Не указано";
				case GroupingType.DeliveryDate:
					return node.DeliveryDate.ToString("dd.MM.yyyy");
				case GroupingType.RouteList:
					return node.RouteList.HasValue ? $"МЛ №{node.RouteList}" : "Без МЛ";
				case GroupingType.Nomenclature:
					return node.NomenclatureName ?? "Не указана";
				case GroupingType.NomenclatureType:
					return node.NomenclatureCategory.ToString();
				case GroupingType.NomenclatureGroup1:
					return node.NomenGroupLevel1Name ?? "Без группы";
				case GroupingType.NomenclatureGroup2:
					return node.NomenGroupLevel2Name ?? "Без группы";
				case GroupingType.NomenclatureGroup3:
					return node.NomenGroupLevel3Name ?? "Без группы";
				case GroupingType.CounterpartyType:
					return node.CounterpartyType.ToString();
				case GroupingType.PaymentType:
					return node.PaymentType.ToString();
				case GroupingType.Organization:
					return node.Organization ?? "Не указана";
				case GroupingType.CounterpartyClassification:
					return node.CounterpartyClassification ?? "Новый";
				case GroupingType.PromotionalSet:
					return node.PromotionalSet ?? "Без промонабора";
				case GroupingType.CounterpartyManager:
					return node.SalesManagerName ?? "Без менеджера";
				case GroupingType.OrderAuthor:
					return node.OrderAuthorName ?? "Без автора";
				default:
					return string.Empty;
			}
		}
	}
}
