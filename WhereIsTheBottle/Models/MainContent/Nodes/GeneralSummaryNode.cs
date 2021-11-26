using System;

namespace WhereIsTheBottle.Models.MainContent.Nodes
{
	public class GeneralSummaryNode
	{
		public DayOfWeek DayOfWeek { get; set; }
		public DateTime Date { get; set; }
		public string DateString => Date.ToString("dd.MM");
		public int AssetByMorning => WarehousesAsset + RouteListsAsset + MovementDocumentsAsset;
		public double NecessaryAssetPercent { get; set; }
		public int NecessaryAssetDifference { get; set; }
		public int BottleDifference { get; set; }
		public double PercentDifference { get; set; }

		public int WarehousesAsset { get; set; }
		public int RouteListsAsset { get; set; }
		public int MovementDocumentsAsset { get; set; }

		public int NecessaryAsset { get; set; }
		public int MinimalAsset { get; set; }
	}
}
