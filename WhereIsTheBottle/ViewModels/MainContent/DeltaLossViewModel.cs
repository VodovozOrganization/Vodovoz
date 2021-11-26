using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Goods.BottleAnalytics;
using WhereIsTheBottle.Commands;
using WhereIsTheBottle.Models.MainContent;
using WhereIsTheBottle.Models.MainContent.Nodes;
using WhereIsTheBottle.Nodes;

namespace WhereIsTheBottle.ViewModels.MainContent
{
	public sealed class DeltaLossViewModel : BottleAnalyticsReportViewModelBase<DeltaLossModel>
	{
		private RelayCommand _loadDataCommand;
		private ObservableCollection<SummaryNode> _warehouseSummaryItems;
		private ObservableCollection<DetailedNode> _detailedDriverItems;
		private ObservableCollection<DetailedNode> _detailedWarehouseItems;
		private bool _isDriverItemsLoading;
		private bool _isWarehouseItemsLoading;
		private bool _isSummaryItemsLoading;
		private ObservableCollection<AssetFilterNode> _selectableWarehouseNodes;
		private AssetFilterNode _selectedAssetNode;

		public DeltaLossViewModel(DeltaLossModel model)
			: base(model)
		{
			WarehouseSummaryItems = new ObservableCollection<SummaryNode>();
			DetailedDriverItems = new ObservableCollection<DetailedNode>();
			DetailedWarehouseItems = new ObservableCollection<DetailedNode>();
		}

		public override string HeaderString => GetHeaderString();
		public bool ShowSummaryItems => SelectedAssetNode?.AssetType == AssetType.All;
		public bool ShowWarehouseItems => SelectedAssetNode?.AssetType is AssetType.All or AssetType.Warehouse;
		public bool ShowDriverItems => SelectedAssetNode?.AssetType is AssetType.All or AssetType.Drivers;

		public AssetFilterNode SelectedAssetNode
		{
			get => _selectedAssetNode;
			set
			{
				if(SetField(ref _selectedAssetNode, value))
				{
					OnFilterChanged();
				}
			}
		}

		public ObservableCollection<AssetFilterNode> SelectableWarehouseNodes
		{
			get => _selectableWarehouseNodes;
			set => SetField(ref _selectableWarehouseNodes, value);
		}

		public bool IsSummaryItemsLoading
		{
			get => _isSummaryItemsLoading;
			set => SetField(ref _isSummaryItemsLoading, value);
		}

		public bool IsDriverItemsLoading
		{
			get => _isDriverItemsLoading;
			set => SetField(ref _isDriverItemsLoading, value);
		}

		public bool IsWarehouseItemsLoading
		{
			get => _isWarehouseItemsLoading;
			set => SetField(ref _isWarehouseItemsLoading, value);
		}

		public ObservableCollection<SummaryNode> WarehouseSummaryItems
		{
			get => _warehouseSummaryItems;
			set => SetField(ref _warehouseSummaryItems, value);
		}

		public ObservableCollection<DetailedNode> DetailedDriverItems
		{
			get => _detailedDriverItems;
			set => SetField(ref _detailedDriverItems, value);
		}

		public ObservableCollection<DetailedNode> DetailedWarehouseItems
		{
			get => _detailedWarehouseItems;
			set => SetField(ref _detailedWarehouseItems, value);
		}

		public RelayCommand LoadDataCommand => _loadDataCommand ??= new RelayCommand(
			async () =>
			{
				if(StartDate == null || EndDate == null)
				{
					return;
				}

				try
				{
					var startDate = StartDate.Value.Date;
					var endDate = EndDate.Value.Date;

					IList<Task> runningTasks = new List<Task>();

					IsDataLoading = true;
					if(ShowSummaryItems)
					{
						runningTasks.Add(LoadSummaryItemsAsync(startDate, endDate));
						IsSummaryItemsLoading = true;
					}
					if(ShowWarehouseItems)
					{
						runningTasks.Add( LoadWarehouseItemsAsync(startDate, endDate, SelectedAssetNode?.WarehouseId));
						IsWarehouseItemsLoading = true;
					}
					if(ShowDriverItems)
					{
						runningTasks.Add(LoadDriverItemsAsync(startDate, endDate));
						IsDriverItemsLoading = true;
					}
					await Task.WhenAll(runningTasks);

					DateFormed = DateTime.Now;
					IsDataLoaded = true;
					OnPropertyChanged(nameof(HeaderString));
				}
				finally
				{
					IsDataLoading = IsSummaryItemsLoading = IsDriverItemsLoading = IsWarehouseItemsLoading = false;
				}
			},
			() => !IsDataLoading
		);

		private async Task LoadSummaryItemsAsync(DateTime startDate, DateTime endDate)
		{
			var nodes = await Task.Run(() => Model.GetWarehouseSummaryNodes(startDate, endDate));
			WarehouseSummaryItems = new ObservableCollection<SummaryNode>(nodes);
			IsSummaryItemsLoading = false;
		}

		private async Task LoadDriverItemsAsync(DateTime startDate, DateTime endDate)
		{
			var nodes = await Task.Run(() => Model.GetDetailedDriverNodes(startDate, endDate));
			DetailedDriverItems = new ObservableCollection<DetailedNode>(nodes);
			IsDriverItemsLoading = false;
		}

		private async Task LoadWarehouseItemsAsync(DateTime startDate, DateTime endDate,
			int? warehouseId)
		{
			var nodes = await Task.Run(() => Model.GetDetailedWarehouseNodes(startDate, endDate, warehouseId));
			DetailedWarehouseItems = new ObservableCollection<DetailedNode>(nodes);
			IsWarehouseItemsLoading = false;
		}

		private string GetHeaderString()
		{
			if(!StartDate.HasValue || !EndDate.HasValue)
			{
				return "Дельта - Потери";
			}

			var baseString = "Дельта - Потери{0}за " + $"{StartDate?.ToString("d")} - {EndDate?.ToString("d")}";

			if(SelectedAssetNode == null || SelectedAssetNode.AssetType == AssetType.All)
			{
				return String.Format(baseString, " ");
			}

			return String.Format(baseString, $" - {SelectedAssetNode.Name} ");
		}

		private void OnFilterChanged()
		{
			WarehouseSummaryItems.Clear();
			DetailedDriverItems.Clear();
			DetailedWarehouseItems.Clear();
			DateFormed = null;

			OnPropertyChanged(nameof(ShowSummaryItems));
			OnPropertyChanged(nameof(ShowDriverItems));
			OnPropertyChanged(nameof(ShowWarehouseItems));
			OnPropertyChanged(nameof(HeaderString));
		}
	}
}
