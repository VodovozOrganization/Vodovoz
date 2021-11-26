using Vodovoz.Domain.Store;

namespace WhereIsTheBottle.Models.MainContent.Nodes
{
	public class AssetNode
	{
		public string Name { get; set; }
		public AssetType AssetType { get; set; }
		public int? Id { get; set; }

		public WarehouseUsing? WarehouseUsing { get; set; }
	}

	public enum AssetType
	{
		All,
		Warehouse,
		Drivers
	}
}
