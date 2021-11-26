using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NLog;
using Vodovoz.EntityRepositories.Goods.BottleAnalytics;
using WhereIsTheBottle.Commands;
using WhereIsTheBottle.Models.MainContent;
using WhereIsTheBottle.Models.MainContent.Nodes;
using WhereIsTheBottle.Nodes;

namespace WhereIsTheBottle.ViewModels.MainContent
{
	public class DeltaShabbyViewModel : BottleAnalyticsReportViewModelBase<DeltaShabbyModel>
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private bool _isDetailedWarehouseItemsLoading;
		private bool _isSummaryItemsLoading;
		private AssetFilterNode _selectedAssetNode;
		private ObservableCollection<AssetFilterNode> _selectableWarehouseNodes;
		private ObservableCollection<DetailedNode> _detailedWarehouseItems;
		private ObservableCollection<SummaryNode> _summaryItems;
		private RelayCommand _loadDataCommand;

		public DeltaShabbyViewModel(DeltaShabbyModel model)
			: base(model)
		{
			SummaryItems = new ObservableCollection<SummaryNode>();
			DetailedWarehouseItems = new ObservableCollection<DetailedNode>();
		}

		public override string HeaderString => GetHeaderString();

		public bool ShowSummaryItems => SelectedAssetNode?.AssetType == AssetType.All;

		public bool IsSummaryItemsLoading
		{
			get => _isSummaryItemsLoading;
			set => SetField(ref _isSummaryItemsLoading, value);
		}

		public bool IsDetailedWarehouseItemsLoading
		{
			get => _isDetailedWarehouseItemsLoading;
			set => SetField(ref _isDetailedWarehouseItemsLoading, value);
		}

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

		public ObservableCollection<SummaryNode> SummaryItems
		{
			get => _summaryItems;
			set => SetField(ref _summaryItems, value);
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

					IsDataLoading = IsDetailedWarehouseItemsLoading = true;
					IList<Task> runningTasks = new List<Task>();
					runningTasks.Add(LoadWarehouseItemsAsync(startDate, endDate, SelectedAssetNode?.WarehouseId));

					if(ShowSummaryItems)
					{
						runningTasks.Add(LoadSummaryItemsAsync(startDate, endDate));
						IsSummaryItemsLoading = true;
					}

					await Task.WhenAll(runningTasks);

					DateFormed = DateTime.Now;
					IsDataLoaded = true;
					OnPropertyChanged(nameof(HeaderString));
				}
				catch(Exception ex)
				{
					_logger.Error(ex);
					throw;
				}
				finally
				{
					IsDataLoading = IsSummaryItemsLoading = IsDetailedWarehouseItemsLoading = false;
				}
			}, () => !IsDataLoading
		);

		private async Task LoadSummaryItemsAsync(DateTime startDate, DateTime endDate)
		{
			var nodes = await Task.Run(() => Model.GetWarehouseSummaryNodes(startDate, endDate));
			SummaryItems = new ObservableCollection<SummaryNode>(nodes);
			IsSummaryItemsLoading = false;
		}

		private async Task LoadWarehouseItemsAsync(DateTime startDate, DateTime endDate, int? warehouseId)
		{
			var nodes = await Task.Run(() => Model.GetDetailedWarehouseNodes(startDate, endDate, warehouseId));
			DetailedWarehouseItems = new ObservableCollection<DetailedNode>(nodes);
			IsDetailedWarehouseItemsLoading = false;
		}

		private string GetHeaderString()
		{
			if(!StartDate.HasValue || !EndDate.HasValue)
			{
				return "Дельта - Стройка";
			}

			var baseString = "Дельта - Стройка{0}за " + $"{StartDate?.ToString("d")} - {EndDate?.ToString("d")}";

			if(SelectedAssetNode == null || SelectedAssetNode.AssetType == AssetType.All)
			{
				return String.Format(baseString, " ");
			}

			return String.Format(baseString, $" - {SelectedAssetNode.Name} ");
		}

		private void OnFilterChanged()
		{
			SummaryItems.Clear();
			DetailedWarehouseItems.Clear();
			DateFormed = null;

			OnPropertyChanged(nameof(ShowSummaryItems));
			OnPropertyChanged(nameof(HeaderString));
		}
	}
}
