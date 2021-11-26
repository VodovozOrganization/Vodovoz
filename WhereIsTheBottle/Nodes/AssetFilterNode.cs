using WhereIsTheBottle.Models.MainContent.Nodes;

namespace WhereIsTheBottle.Nodes
{
	public class AssetFilterNode
	{
		public AssetType AssetType { get; set; }
		public int? WarehouseId { get; set; }
		public string Name { get; set; }
	}
}
