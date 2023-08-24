using QS.ViewModels.Widgets;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vodovoz.Reports.Editing.Modifiers;
using Vodovoz.ViewModels.ReportsParameters.Profitability;

namespace Vodovoz.ViewModels.Factories
{
	public class LeftRightListViewModelFactory : ILeftRightListViewModelFactory
	{
		private static ReadOnlyCollection<GroupingNode> _defaultSalerReportsGroupingNodes =>
			new List<GroupingNode>
			{
				new GroupingNode { Name = "Заказ", GroupType = GroupingType.Order },
				new GroupingNode { Name = "Контрагент", GroupType = GroupingType.Counterparty },
				new GroupingNode { Name = "Подразделение", GroupType = GroupingType.Subdivision },
				new GroupingNode { Name = "Дата доставки", GroupType = GroupingType.DeliveryDate },
				new GroupingNode { Name = "Маршрутный лист", GroupType = GroupingType.RouteList },
				new GroupingNode { Name = "Номенклатура", GroupType = GroupingType.Nomenclature },
				new GroupingNode { Name = "Группа уровень 1", GroupType = GroupingType.NomenclatureGroup1 },
				new GroupingNode { Name = "Группа уровень 2", GroupType = GroupingType.NomenclatureGroup2 },
				new GroupingNode { Name = "Группа уровень 3", GroupType = GroupingType.NomenclatureGroup3 }
			}.AsReadOnly();

		public LeftRightListViewModel<GroupingNode> CreateSalesReportGroupingsConstructor()
		{
			LeftRightListViewModel<GroupingNode> leftRightListViewModel = new LeftRightListViewModel<GroupingNode>
			{
				LeftLabel = "Доступные группировки",
				RightLabel = "Выбранные группировки (макс. 3)",
				RightItemsMaximum = 3
			};

			leftRightListViewModel.SetLeftItems(_defaultSalerReportsGroupingNodes, x => x.Name);

			return leftRightListViewModel;
		}
	}
}
