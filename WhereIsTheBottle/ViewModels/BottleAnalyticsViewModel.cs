using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using QS.ViewModels;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Store;
using WhereIsTheBottle.Commands;
using WhereIsTheBottle.Models;
using WhereIsTheBottle.Models.MainContent.Nodes;
using WhereIsTheBottle.Nodes;
using WhereIsTheBottle.ViewModels.MainContent;

namespace WhereIsTheBottle.ViewModels
{
	public class BottleAnalyticsViewModel : ViewModelBase
	{
		private readonly BottleAnalyticsModel _bottleAnalyticsModel;
		private readonly ILifetimeScope _rootScope;
		private readonly Dictionary<int, ViewModelBase> _viewModels = new();

		private ViewModelBase _currentAnalyticsReportViewModel;
		private ObservableCollection<MenuItem> _menuItems;
		private RelayCommand<MenuItem> _switchMainContentCommand;

		private DateTime? _endDate = DateTime.Now;
		private DateTime? _startDate = DateTime.Now.AddDays(-5);
		private bool _isMenuInitializing;

		public BottleAnalyticsViewModel(ILifetimeScope rootScope)
		{
			_rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
			_bottleAnalyticsModel = rootScope.Resolve<BottleAnalyticsModel>();
		}

		public RelayCommand<MenuItem> SwitchMainContentCommand =>
			_switchMainContentCommand ??= new RelayCommand<MenuItem>(
				item =>
				{
					if(item == null)
					{
						return;
					}
					SwitchViewModel(item);
				}
			);

		public ViewModelBase CurrentAnalyticsReportViewModel
		{
			get => _currentAnalyticsReportViewModel;
			set => SetField(ref _currentAnalyticsReportViewModel, value);
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public bool IsMenuInitializing
		{
			get => _isMenuInitializing;
			set => SetField(ref _isMenuInitializing, value);
		}

		public ObservableCollection<MenuItem> MenuItems
		{
			get => _menuItems;
			set => SetField(ref _menuItems, value);
		}

		public async Task InititalizeAsync()
		{
			try
			{
				IsMenuInitializing = true;

				var summary = new MenuItem { Title = "Общая сводка", Type = BottleAnalyticsType.GeneralSummary, Index = GetNextIndex() };
				var delta = new MenuItem { Title = "Дельта", Type = BottleAnalyticsType.GeneralDelta, Index = GetNextIndex() };
				var asset = new MenuItem { Title = "Актив", Type = BottleAnalyticsType.GeneralAsset, Index = GetNextIndex() };

				var warehouseNodes = await _bottleAnalyticsModel.GetActiveWarehouseNodesAsync();
				AddSubDeltaMenuItems(delta, null, warehouseNodes);

				// foreach(var warehouseNode in warehouseNodes)
				// {
				// 	var warehouseAssetNode = new MenuItem
				// 	{
				// 		Title = $"Движения - {warehouseNode.Name}",
				// 		Index = GetNextIndex(),
				// 		SelectedWarehouseNode = warehouseNode
				// 	};
				// 	AddSubDeltaMenuItems(warehouseAssetNode);
				// 	warehouseAssetNode.Type = warehouseNode.WarehouseUsing == WarehouseUsing.Shipment
				// 		? BottleAnalyticsType.AssetShipment
				// 		: BottleAnalyticsType.AssetProduction;
				// 	asset.ChildItems.Add(warehouseAssetNode);
				// }

				// var driversAsset = new MenuItem
				// 	{ Title = "Движения - Водители", Type = BottleAnalyticsType.AssetDrivers, Index = GetNextIndex() };
				// AddSubDeltaMenuItems(driversAsset);
				// asset.ChildItems.Add(driversAsset);
				MenuItems = new ObservableCollection<MenuItem> { summary, delta, asset };
			}
			finally
			{
				IsMenuInitializing = false;
			}
		}

		private void SwitchViewModel(MenuItem item)
		{
			if(_viewModels[item.Index] != null)
			{
				CurrentAnalyticsReportViewModel = _viewModels[item.Index];
				return;
			}
			switch(item.Type)
			{
				case BottleAnalyticsType.GeneralSummary:
					var generalSummaryViewModel = _rootScope.Resolve<GeneralSummaryViewModel>();
					generalSummaryViewModel.StartDate = _startDate;
					generalSummaryViewModel.EndDate = _endDate;
					CurrentAnalyticsReportViewModel = _viewModels[item.Index] = generalSummaryViewModel;
					break;
				case BottleAnalyticsType.GeneralDelta:
					var generalDeltaViewModel = _rootScope.Resolve<GeneralDeltaViewModel>();
					generalDeltaViewModel.StartDate = _startDate;
					generalDeltaViewModel.EndDate = _endDate;
					CurrentAnalyticsReportViewModel = _viewModels[item.Index] = generalDeltaViewModel;
					break;
				case BottleAnalyticsType.GeneralAsset:
					var generalAssetViewModel = _rootScope.Resolve<GeneralAssetViewModel>();
					generalAssetViewModel.StartDate = _startDate;
					generalAssetViewModel.EndDate = _endDate;
					CurrentAnalyticsReportViewModel = _viewModels[item.Index] = generalAssetViewModel;
					break;
				case BottleAnalyticsType.DeltaLoss:
					var deltaLossViewModel = _rootScope.Resolve<DeltaLossViewModel>();
					deltaLossViewModel.StartDate = _startDate;
					deltaLossViewModel.EndDate = _endDate;
					deltaLossViewModel.SelectableWarehouseNodes =
						CreateAssetFilterNodesFromWarehouseNodes(item.AvailableWarehouseNodes, true, true);
					deltaLossViewModel.SelectedAssetNode = deltaLossViewModel.SelectableWarehouseNodes.First();
					CurrentAnalyticsReportViewModel = _viewModels[item.Index] = deltaLossViewModel;
					break;
				case BottleAnalyticsType.DeltaShabby:
					var deltaShabbyViewModel = _rootScope.Resolve<DeltaShabbyViewModel>();
					deltaShabbyViewModel.StartDate = _startDate;
					deltaShabbyViewModel.EndDate = _endDate;
					deltaShabbyViewModel.SelectableWarehouseNodes =
						CreateAssetFilterNodesFromWarehouseNodes(item.AvailableWarehouseNodes, false, true);
					deltaShabbyViewModel.SelectedAssetNode = deltaShabbyViewModel.SelectableWarehouseNodes.First();
					CurrentAnalyticsReportViewModel = _viewModels[item.Index] = deltaShabbyViewModel;
					break;
				case BottleAnalyticsType.DeltaDefective:
					var deltaDefectiveViewModel = _rootScope.Resolve<DeltaDefectiveViewModel>();
					deltaDefectiveViewModel.StartDate = _startDate;
					deltaDefectiveViewModel.EndDate = _endDate;
					deltaDefectiveViewModel.SelectableWarehouseNodes =
						CreateAssetFilterNodesFromWarehouseNodes(item.AvailableWarehouseNodes, false, true);
					deltaDefectiveViewModel.SelectedAssetNode = deltaDefectiveViewModel.SelectableWarehouseNodes.First();
					CurrentAnalyticsReportViewModel = _viewModels[item.Index] = deltaDefectiveViewModel;
					break;
				case BottleAnalyticsType.AssetDrivers:
					var assetDriversViewModel = _rootScope.Resolve<AssetDriversViewModel>();
					assetDriversViewModel.StartDate = _startDate;
					assetDriversViewModel.EndDate = _endDate;
					CurrentAnalyticsReportViewModel = _viewModels[item.Index] = assetDriversViewModel;
					break;
				case BottleAnalyticsType.AssetProduction:
				case BottleAnalyticsType.AssetShipment:
					var assetWarehouseViewModel = _rootScope.Resolve<AssetWarehouseViewModel>();
					assetWarehouseViewModel.StartDate = _startDate;
					assetWarehouseViewModel.EndDate = _endDate;
					assetWarehouseViewModel.SelectedWarehouseNode = item.SelectedWarehouseNode;
					assetWarehouseViewModel.SelectableWarehouseNodes =
						new ObservableCollection<WarehouseNode>(item.AvailableWarehouseNodes);
					CurrentAnalyticsReportViewModel = _viewModels[item.Index] = assetWarehouseViewModel;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(item), item.Type, $"{item.Type} не реализован");
			}
		}

		private void AddSubDeltaMenuItems(MenuItem menuItem, WarehouseNode selectedWarehouseNode = null,
			IList<WarehouseNode> availableWarehouseNodes = null)
		{
			menuItem.ChildItems.Add(new MenuItem
			{
				Title = "Дельта - Потери",
				Type = BottleAnalyticsType.DeltaLoss,
				Index = GetNextIndex(),
				SelectedWarehouseNode = selectedWarehouseNode,
				AvailableWarehouseNodes = availableWarehouseNodes?.ToList()
			});
			menuItem.ChildItems.Add(new MenuItem
			{
				Title = "Дельта - Стройка",
				Type = BottleAnalyticsType.DeltaShabby,
				Index = GetNextIndex(),
				SelectedWarehouseNode = selectedWarehouseNode,
				AvailableWarehouseNodes = availableWarehouseNodes?.ToList()
			});
			menuItem.ChildItems.Add(new MenuItem
			{
				Title = "Дельта - Брак",
				Type = BottleAnalyticsType.DeltaDefective,
				Index = GetNextIndex(),
				SelectedWarehouseNode = selectedWarehouseNode,
				AvailableWarehouseNodes = availableWarehouseNodes?.ToList()
			});
		}

		private int GetNextIndex()
		{
			var count = _viewModels.Count;
			_viewModels.Add(_viewModels.Count, null);
			return count;
		}

		private ObservableCollection<AssetFilterNode> CreateAssetFilterNodesFromWarehouseNodes(IList<WarehouseNode> warehouseNodes,
			bool addDriversNode = false, bool addAllNode = false)
		{
			var assetFilterNodes = new List<AssetFilterNode>();
			if(addAllNode)
			{
				assetFilterNodes.Add(new AssetFilterNode { Name = "Все", AssetType = AssetType.All });
			}
			if(addDriversNode)
			{
				assetFilterNodes.Add(new AssetFilterNode { Name = "Водители", AssetType = AssetType.Drivers });
			}
			foreach(var warehouseNode in warehouseNodes)
			{
				assetFilterNodes.Add(new AssetFilterNode
				{
					Name = warehouseNode.Name,
					WarehouseId = warehouseNode.Id,
					AssetType = AssetType.Warehouse
				});
			}

			return new ObservableCollection<AssetFilterNode>(assetFilterNodes);
		}
	}

	public class MenuItem
	{
		public string Title { get; set; }
		public BottleAnalyticsType Type { get; set; }
		public int Index { get; set; }
		public WarehouseNode SelectedWarehouseNode { get; set; }
		public IList<WarehouseNode> AvailableWarehouseNodes { get; set; } = new List<WarehouseNode>();
		public ObservableCollection<MenuItem> ChildItems { get; set; } = new();
	}
}
