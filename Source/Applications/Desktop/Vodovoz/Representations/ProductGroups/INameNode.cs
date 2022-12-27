using Gamma.ColumnConfig;
using QSOrmProject.RepresentationModel;

namespace Vodovoz.Representations.ProductGroups
{
	public interface INameNode
	{
		[UseForSearch]
		[SearchHighlight]
		int Id { get; set; }
		[UseForSearch]
		[SearchHighlight]
		string Name { get; set; }
		bool IsArchive { get; set; }
		int? ParentId { get; set; }
	}
}
