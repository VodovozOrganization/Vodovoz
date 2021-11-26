using System;
using System.Collections.Generic;
using WhereIsTheBottle.Models.MainContent.Nodes;

namespace WhereIsTheBottle.ViewModels.MainContent
{
	public class MainContentParameters
	{
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public AssetNode SelectedWarehouseNode { get; set; }

		private IList<AssetNode> _selectableWarehouseNodes;
		public IList<AssetNode> SelectableWarehouseNodes
		{
			get => _selectableWarehouseNodes ??= new List<AssetNode>();
			set => _selectableWarehouseNodes = value;
		}
	}
}
