using System;
using Gamma.ColumnConfig;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;

namespace Vodovoz.Representations
{
	public class CommentsTemplatesVM : RepresentationModelWithoutEntityBase<CommentsTemplatesVMNode>
	{

		#region Fields
		#endregion

		#region Constructors

		public CommentsTemplatesVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			CreateRepresentationFilter = () => new FineFilter(UoW);
		}

		public CommentsTemplatesVM(FineFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public CommentsTemplatesVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
		}

		#endregion

		#region Property

		public IUnitOfWorkGeneric<CommentsTemplates> CommentsTemplatesUoW {
			get {
				return UoW as IUnitOfWorkGeneric<CommentsTemplates>;
			}
		}

		public virtual FineFilter Filter {
			get {
				return RepresentationFilter as FineFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}
		#endregion

		IColumnsConfig columnsConfig = FluentColumnsConfig<CommentsTemplatesVMNode>.Create()
			.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Комментарий").AddTextRenderer(node => node.CommentTmp)
			.AddColumn("Группа").AddTextRenderer(node => node.GroupTmp)
			.Finish();

		public override void UpdateNodes()
		{
			CommentsTemplatesVMNode resultAlias = null;
			CommentsTemplates commentsTemplatesAlias = null;
			CommentsGroups commentsGroupsAlias = null;


			var query = UoW.Session.QueryOver<CommentsTemplates>(() => commentsTemplatesAlias);

			//if(Filter.RestrictionFineDateStart.HasValue) {
			//	query.Where(() => fineAlias.Date >= Filter.RestrictionFineDateStart.Value);
			//}


			var result = query
							.JoinAlias(() => commentsTemplatesAlias.CommentsTmpGroups, () => commentsGroupsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							.SelectList(list => list
								.Select(() => commentsTemplatesAlias.Id).WithAlias(() => resultAlias.Id)
								.Select(() => commentsTemplatesAlias.CommentTemplate).WithAlias(() => resultAlias.CommentTmp)
								.Select(() => commentsGroupsAlias.Name).WithAlias(() => resultAlias.GroupTmp)
							).TransformUsing(Transformers.AliasToBean<CommentsTemplatesVMNode>())
							.List<CommentsTemplatesVMNode>();

			SetItemsSource(result);
		}

		public override IColumnsConfig ColumnsConfig {
			get {
				return columnsConfig;
			}
		}

		protected override bool NeedUpdateFunc(object updatedSubject)
		{
			return true;
		}
	}


	public class CommentsTemplatesVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string CommentTmp { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string GroupTmp { get; set; }

	}
}
