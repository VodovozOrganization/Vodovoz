using System.Collections.Generic;
using Gamma.ColumnConfig;
using QSOrmProject.RepresentationModel;

namespace Vodovoz.Representations.ProductGroups
{
	public class ProductGroupVMNode : INameNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }
		[UseForSearch]
		[SearchHighlight]
		public string Name { get; set; }
		public bool IsArchive { get; set; }
		public IList<ProductGroupVMNode> ChildGroups { get; set; } = new List<ProductGroupVMNode>();
		public IList<NomenclatureNode> ChildNomenclatures { get; set; } = new List<NomenclatureNode>();
		public ProductGroupVMNode Parent { get; set; }
		public int? ParentId { get; set; }
	}
}
