using System.Collections.Generic;
using Gamma.ColumnConfig;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CommentsTemplateView : Gtk.Bin
	{
		private IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if(uow == value)
					return;
				uow = value;
				viewModel = new CommentsTemplateVM(value);
				ytreeComments.RepresentationModel = viewModel;
				ytreeComments.RepresentationModel.UpdateNodes();
			}
		}

		CommentsTemplateVM viewModel;

		public CommentsTemplateView()
		{
			this.Build();
		}

		public IList<CommentsTemplatesVMNode> Items { get { return viewModel.ItemsList as IList<CommentsTemplatesVMNode>; } }

	}

	public class CommentsTemplateVM : RepresentationModelWithoutEntityBase<CommentsTemplatesVMNode>
	{
		public CommentsTemplateVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
		}

		public CommentsTemplateVM(IUnitOfWork uow) : base(typeof(CommentsTemplates))
		{
			this.UoW = uow;
		}

		public override void UpdateNodes()
		{
			CommentsTemplatesVMNode resultAlias = null;
			CommentsTemplates commentsTemplatesAlias = null;


			var query = UoW.Session.QueryOver<CommentsTemplates>(() => commentsTemplatesAlias);

			//if(Filter.RestrictionFineDateStart.HasValue) {
			//	query.Where(() => fineAlias.Date >= Filter.RestrictionFineDateStart.Value);
			//}

			var result = query.SelectList(list => list
									.Select(() => commentsTemplatesAlias.Id).WithAlias(() => resultAlias.Id)
											 .Select(() => commentsTemplatesAlias.CommentTemplate).WithAlias(() => resultAlias.CommentTmp)
						).TransformUsing(Transformers.AliasToBean<CommentsTemplatesVMNode>())
						.List<CommentsTemplatesVMNode>();

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<CommentsTemplatesVMNode>.Create()
					  .AddColumn("№").AddNumericRenderer(node => node.Id)
		              .AddColumn("Комментарий").AddNumericRenderer(node => node.CommentTmp)
					  .AddColumn("")
					  .Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
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

	}
}
