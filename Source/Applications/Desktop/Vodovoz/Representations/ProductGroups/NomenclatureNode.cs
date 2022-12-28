using Gamma.ColumnConfig;
using QSOrmProject.RepresentationModel;

namespace Vodovoz.Representations.ProductGroups
{
	public class NomenclatureNode : INameNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }
		[UseForSearch]
		[SearchHighlight]
		public string Name { get; set; }
		public bool IsArchive { get; set; }
		public NomenclatureGroupNode Parent { get; set; }
		public int? ParentId { get; set; }
	}
}
