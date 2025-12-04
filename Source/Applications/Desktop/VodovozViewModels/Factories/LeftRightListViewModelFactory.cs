using QS.ViewModels.Widgets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vodovoz.Reports.Editing.Modifiers;
using Vodovoz.ViewModels.ReportsParameters.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public class LeftRightListViewModelFactory : ILeftRightListViewModelFactory
	{
		private static ReadOnlyCollection<GroupingNode> _defaultSalesReportsGroupingNodes =>
			new List<GroupingNode>
			{
				new GroupingNode { Name = "Заказ", GroupType = GroupingType.Order },
				new GroupingNode { Name = "Контрагент", GroupType = GroupingType.Counterparty },
				new GroupingNode { Name = "Подразделение", GroupType = GroupingType.Subdivision },
				new GroupingNode { Name = "Дата доставки", GroupType = GroupingType.DeliveryDate },
				new GroupingNode { Name = "Маршрутный лист", GroupType = GroupingType.RouteList },
				new GroupingNode { Name = "Номенклатура", GroupType = GroupingType.Nomenclature },
				new GroupingNode { Name = "Тип номенклатуры", GroupType = GroupingType.NomenclatureType },
				new GroupingNode { Name = "Группа уровень 1", GroupType = GroupingType.NomenclatureGroup1 },
				new GroupingNode { Name = "Группа уровень 2", GroupType = GroupingType.NomenclatureGroup2 },
				new GroupingNode { Name = "Группа уровень 3", GroupType = GroupingType.NomenclatureGroup3 },
				new GroupingNode { Name = "Тип контрагента/подтип", GroupType = GroupingType.CounterpartyType },
				new GroupingNode { Name = "Тип оплаты", GroupType = GroupingType.PaymentType },
				new GroupingNode { Name = "Организация", GroupType = GroupingType.Organization },
				new GroupingNode { Name = "Классификация контрагента", GroupType = GroupingType.CounterpartyClassification },
				new GroupingNode { Name = "Промонаборы", GroupType = GroupingType.PromotionalSet },
				new GroupingNode { Name = "Менеджер КА", GroupType = GroupingType.CounterpartyManager },
				new GroupingNode { Name = "Автор заказа", GroupType = GroupingType.OrderAuthor },
			}.AsReadOnly();

		private static ReadOnlyCollection<GroupingNode> _defaultSalesWithDynamicsReportsGroupingNodes =>
			new List<GroupingNode>
			{
				new GroupingNode { Name = "Заказ", GroupType = GroupingType.Order },
				new GroupingNode { Name = "Контрагент", GroupType = GroupingType.Counterparty },
				new GroupingNode { Name = "Подразделение", GroupType = GroupingType.Subdivision },
				new GroupingNode { Name = "Дата доставки", GroupType = GroupingType.DeliveryDate },
				new GroupingNode { Name = "Маршрутный лист", GroupType = GroupingType.RouteList },
				new GroupingNode { Name = "Номенклатура", GroupType = GroupingType.Nomenclature },
				new GroupingNode { Name = "Тип номенклатуры", GroupType = GroupingType.NomenclatureType },
				new GroupingNode { Name = "Группа товаров", GroupType = GroupingType.NomenclatureGroup },
				new GroupingNode { Name = "Тип контрагента/подтип", GroupType = GroupingType.CounterpartyType },
				new GroupingNode { Name = "Тип оплаты", GroupType = GroupingType.PaymentType },
				new GroupingNode { Name = "Организация", GroupType = GroupingType.Organization },
				new GroupingNode { Name = "Классификация контрагента", GroupType = GroupingType.CounterpartyClassification },
				new GroupingNode { Name = "Промонаборы", GroupType = GroupingType.PromotionalSet },
				new GroupingNode { Name = "Менеджер КА", GroupType = GroupingType.CounterpartyManager },
				new GroupingNode { Name = "Автор заказа", GroupType = GroupingType.OrderAuthor },
			}.AsReadOnly();

		private readonly IEnumerable<GroupingNode> _defaultCompletedDriverEventsSortingNodes =
			new[]
			{
				new GroupingNode { Name = "Сотрудник", GroupType = GroupingType.Employee },
				new GroupingNode { Name = "Время события", GroupType = GroupingType.DriverWarehouseEventDate },
				new GroupingNode { Name = "Событие", GroupType = GroupingType.DriverWarehouseEvent },
			};

		private static readonly IEnumerable<GroupingNode> _defaultEdoControlReportGroupingNodes =
			new[]
			{
				new GroupingNode { Name = "Дата доставки", GroupType = GroupingType.DeliveryDate },
				new GroupingNode { Name = "Контрагент", GroupType = GroupingType.Counterparty },
				new GroupingNode { Name = "Статус документооборота", GroupType = GroupingType.EdoDocFlowStatus },
				new GroupingNode { Name = "Тип доставки", GroupType = GroupingType.OrderDeliveryType },
				new GroupingNode { Name = "Тип переноса заказа", GroupType = GroupingType.OrderTransferType },
			};

		public LeftRightListViewModel<GroupingNode> CreateSalesReportGroupingsConstructor()
		{
			LeftRightListViewModel<GroupingNode> leftRightListViewModel = new LeftRightListViewModel<GroupingNode>
			{
				LeftLabel = "Доступные группировки",
				RightLabel = "Выбранные группировки (макс. 3)",
				RightItemsMaximum = 3
			};

			leftRightListViewModel.SetLeftItems(_defaultSalesReportsGroupingNodes, x => x.Name);

			return leftRightListViewModel;
		}

		public LeftRightListViewModel<GroupingNode> CreateSalesWithDynamicsReportGroupingsConstructor()
		{
			var leftRightListViewModel = new LeftRightListViewModel<GroupingNode>
			{
				LeftLabel = "Доступные группировки",
				RightLabel = "Выбранные группировки (макс. 3)",
				RightItemsMaximum = 3
			};

			SetDefaultLeftItemsForSalesWithDynamicsReportGroupings(leftRightListViewModel);

			return leftRightListViewModel;
		}

		public void SetDefaultLeftItemsForSalesWithDynamicsReportGroupings(LeftRightListViewModel<GroupingNode> leftRightListViewModel)
		{
			leftRightListViewModel.SetLeftItems(_defaultSalesWithDynamicsReportsGroupingNodes, x => x.Name);
		}

		public LeftRightListViewModel<GroupingNode> CreateCompletedDriverEventsSortingConstructor()
		{
			const int maxRightItems = 2;
			
			var leftRightListViewModel = new LeftRightListViewModel<GroupingNode>
			{
				LeftLabel = "Доступные сортировки",
				RightLabel = $"Выбранные сортировки (макс. {maxRightItems})",
				RightItemsMaximum = maxRightItems
			};

			leftRightListViewModel.SetLeftItems(_defaultCompletedDriverEventsSortingNodes, x => x.Name);

			leftRightListViewModel.SelectedLeftItems =
				leftRightListViewModel.LeftItems.Where(x => x.Name != "Событие").ToList();
			
			leftRightListViewModel.MoveRightCommand.Execute();

			return leftRightListViewModel;
		}

		public LeftRightListViewModel<GroupingNode> CreateEdoControlReportGroupingsConstructor()
		{
			LeftRightListViewModel<GroupingNode> leftRightListViewModel = new LeftRightListViewModel<GroupingNode>
			{
				LeftLabel = "Доступные группировки",
				RightLabel = "Выбранные группировки (макс. 3)",
				RightItemsMaximum = 3
			};

			leftRightListViewModel.SetLeftItems(_defaultEdoControlReportGroupingNodes, x => x.Name);

			return leftRightListViewModel;
		}
	}
}
