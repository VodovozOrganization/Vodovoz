using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WhereIsTheBottle.Commands;
using WhereIsTheBottle.Models.MainContent;
using WhereIsTheBottle.Models.MainContent.Nodes;

namespace WhereIsTheBottle.ViewModels.MainContent
{
	public class GeneralSummaryViewModel : BottleAnalyticsReportViewModelBase<GeneralSummaryModel>
	{
		private RelayCommand _loadDataCommand;
		private ObservableCollection<GeneralSummaryNode> _items = new();

		public GeneralSummaryViewModel(GeneralSummaryModel model)
			: base(model)
		{
			SubscribeToModelPropertyUpdates();
		}

		public ObservableCollection<GeneralSummaryNode> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		public int NecessaryAssetValue
		{
			get => Model.NecessaryAssetValue;
			set => Model.NecessaryAssetValue = value;
		}

		public int MinimalAssetValue
		{
			get => Model.MinimalAssetValue;
			set => Model.MinimalAssetValue = value;
		}

		public double MinimalAssetPercent
		{
			get => Model.MinimalAssetPercent;
			set => Model.MinimalAssetPercent = value;
		}

		public override string HeaderString => StartDate.HasValue && EndDate.HasValue
			? $"Общая сводка за {StartDate?.ToString("d")} - {EndDate?.ToString("d")}"
			: "Общая сводка";

		public RelayCommand LoadDataCommand => _loadDataCommand ??= new RelayCommand(
			LoadDataAsync,
			() => !IsDataLoading
		);

		private async void LoadDataAsync()
		{
			if(StartDate == null || EndDate == null)
			{
				return;
			}

			try
			{
				IsDataLoading = true;
				var nodes = await Task.Run(() => Model.GetGeneralSummaryNodes(
					StartDate.Value.Date,
					EndDate.Value.Date,
					NecessaryAssetValue,
					MinimalAssetValue));

				Items = new ObservableCollection<GeneralSummaryNode>(nodes);
				DateFormed = DateTime.Now;
				IsDataLoaded = true;
				OnPropertyChanged(nameof(HeaderString));
			}
			finally
			{
				IsDataLoading = false;
			}
		}
	}
}
