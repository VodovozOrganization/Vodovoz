using Gamma.ColumnConfig;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;

namespace Vodovoz.Representations
{
	public class CommentTemplatesVM : RepresentationModelEntityBase<CommentsTemplates, CommentsTemplatesVMNode>
	{
 
		#region Поля
		#endregion

		#region Конструкторы

		public CommentTemplatesVM() : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new FineFilter(UoW);
		}

		public CommentTemplatesVM(FineFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public CommentTemplatesVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
		}

		#endregion

		#region Свойства

		public virtual FineFilter Filter {
			get {
				return RepresentationFilter as FineFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#endregion
	}


	public class CommentsTemplatesVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public DateTime Date { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string EmployeesName { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string FineReason { get; set; }
	}
}
