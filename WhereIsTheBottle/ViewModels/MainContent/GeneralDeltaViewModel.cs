using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WhereIsTheBottle.Commands;
using WhereIsTheBottle.Models.MainContent;
using WhereIsTheBottle.Models.MainContent.Nodes;

namespace WhereIsTheBottle.ViewModels.MainContent
{
	public class GeneralDeltaViewModel : BottleAnalyticsReportViewModelBase<GeneralDeltaModel>
	{
		private ObservableCollection<GeneralDeltaNode> _items;
		private RelayCommand _loadDataCommand;

		public GeneralDeltaViewModel(GeneralDeltaModel model) : base(model)
		{
			Items = new ObservableCollection<GeneralDeltaNode>();
		}

		public override string HeaderString => StartDate.HasValue && EndDate.HasValue
			? $"Дельта за {StartDate?.ToString("d")} - {EndDate?.ToString("d")}"
			: "Дельта";

		public ObservableCollection<GeneralDeltaNode> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

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
				var nodes = await Task.Run(() => Model.GetGeneralDeltaNodes(StartDate.Value.Date, EndDate.Value.Date));
				Items = new ObservableCollection<GeneralDeltaNode>(nodes);
				AddSummaryItem();
				DateFormed = DateTime.Now;
				IsDataLoaded = true;
				OnPropertyChanged(nameof(HeaderString));
			}
			finally
			{
				IsDataLoading = false;
			}
		}

		private void AddSummaryItem()
		{
			Items.Add(new GeneralDeltaNode
			{
				DateString = "Итого:",
				AssetByMorningString = "",
				CounterpartyReturnLoss = Items.Sum(x => x.CounterpartyReturnLoss),
				InventarizationLoss = Items.Sum(x => x.InventarizationLoss),
				CounterpartySelfDeliveryLoss = Items.Sum(x => x.CounterpartySelfDeliveryLoss),
				DriversDiscrepancyLoss = Items.Sum(x => x.DriversDiscrepancyLoss),
				RegradingOfGoodsLoss = Items.Sum(x => x.RegradingOfGoodsLoss),
				WriteoffDocumentLoss = Items.Sum(x => x.WriteoffDocumentLoss),
				TotalLoss = Items.Sum(x => x.TotalLoss),
				DriversDiscrepancyIncome = Items.Sum(x => x.DriversDiscrepancyIncome),
				IncomingInvoiceIncome = Items.Sum(x => x.IncomingInvoiceIncome),
				CounterpartyReturnIncome = Items.Sum(x => x.CounterpartyReturnIncome),
				CounterpartySelfDeliveryIncome = Items.Sum(x => x.CounterpartySelfDeliveryIncome),
				InventarizationIncome = Items.Sum(x => x.InventarizationIncome),
				RegradingOfGoodsIncome = Items.Sum(x => x.RegradingOfGoodsIncome),
				TotalIncome = Items.Sum(x => x.TotalIncome),
				Delta = Items.Sum(x => x.Delta),
				AssetByEveningString = "",
				SortType = SortType.AlwaysAtTheBottom
			});
		}
	}
}
