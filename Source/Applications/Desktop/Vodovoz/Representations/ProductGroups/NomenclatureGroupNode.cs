using System.Collections.Generic;

namespace Vodovoz.Representations.ProductGroups
{
	public class NomenclatureGroupNode : INameNode
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsArchive { get; set; }
		public IList<NomenclatureNode> ChildNomenclatures { get; set; } = new List<NomenclatureNode>();
		public ProductGroupVMNode Parent { get; set; }
		public int? ParentId { get; set; }
	}
}
