using System;

namespace WhereIsTheBottle.Models.MainContent.Nodes
{
	public class GeneralDeltaNode : ICustomSortNode
	{
		private string _assetByMorningString;
		private string _assetByEveningString;
		private string _dateString;
		public DateTime Date { get; set; }

		public string DateString
		{
			get => _dateString ?? Date.ToString("dd.MM");
			set => _dateString = value;
		}

		public int AssetByMorning => WarehousesAsset + RouteListAsset + MovementDocumentsAsset;

		public string AssetByMorningString
		{
			get => _assetByMorningString ?? AssetByMorning.ToString();
			set => _assetByMorningString = value;
		}

		public int WarehousesAsset { get; set; }
		public int RouteListAsset { get; set; }
		public int MovementDocumentsAsset { get; set; }

		public int InventarizationIncome { get; set; }
		public int InventarizationLoss { get; set; }

		public int RegradingOfGoodsIncome { get; set; }
		public int RegradingOfGoodsLoss { get; set; }

		public int CounterpartySelfDeliveryIncome { get; set; }
		public int CounterpartySelfDeliveryLoss { get; set; }

		public int DriversDiscrepancyIncome { get; set; }
		public int DriversDiscrepancyLoss { get; set; }

		public int CounterpartyReturnIncome { get; set; }
		public int CounterpartyReturnLoss { get; set; }

		public int IncomingInvoiceIncome { get; set; }
		public int WriteoffDocumentLoss { get; set; }

		public int TotalLoss { get; set; }
		public int TotalIncome { get; set; }
		public int Delta { get; set; }
		public int AssetByEvening { get; set; }

		public string AssetByEveningString
		{
			get => _assetByEveningString ?? AssetByEvening.ToString();
			set => _assetByEveningString = value;
		}

		public void Calculate()
		{
			TotalLoss =
				CounterpartyReturnLoss
				+ CounterpartySelfDeliveryLoss
				+ RegradingOfGoodsLoss
				+ InventarizationLoss
				+ WriteoffDocumentLoss
				+ DriversDiscrepancyLoss;
			TotalIncome =
				DriversDiscrepancyIncome
				+ IncomingInvoiceIncome
				+ CounterpartyReturnIncome
				+ InventarizationIncome
				+ RegradingOfGoodsIncome
				+ CounterpartySelfDeliveryIncome;
			Delta = TotalIncome + TotalLoss;
			AssetByEvening = AssetByMorning + Delta;
		}

		public SortType SortType { get; set; } = SortType.Sortable;
	}

	public interface ICustomSortNode
	{
		public SortType SortType { get; }
	}

	public enum SortType
	{
		AlwaysOnTop,
		Sortable,
		AlwaysAtTheBottom
	}
}
