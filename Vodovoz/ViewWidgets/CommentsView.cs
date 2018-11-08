using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CommentsView : QS.Dialog.Gtk.WidgetOnDialogBase
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
				viewModel = new CommentsVM(value);
				ytreeComments.RepresentationModel = viewModel;
				ytreeComments.RepresentationModel.UpdateNodes();
			}
		}

		CommentsVM viewModel;

		public CommentsView()
		{
			this.Build();
		}

		public IList<CommentsVMNode> Items { get { return viewModel.ItemsList as IList<CommentsVMNode>; } }

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			MyTab.TabParent.AddTab(new NuanceDlg(UoW.RootObject), MyTab);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			MyTab.TabParent.AddTab(new NuanceDlg(UoW.RootObject, ytreeComments.GetSelectedId()), MyTab);
		}

		protected void OnYtreeCommentsCursorChanged(object sender, EventArgs e)
		{
			bool selected = ytreeComments.Selection.CountSelectedRows() > 0;
			buttonEdit.Sensitive = selected;
		}
	}



	public class CommentsVM : RepresentationModelWithoutEntityBase<CommentsVMNode>
	{
		public CommentsVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
		}

		public CommentsVM(IUnitOfWork uow) : base(typeof(Comments))
		{
			this.UoW = uow;
		}

		public override void UpdateNodes()
		{

			CommentsVMNode resultAlias = null;
			Comments commentAlias = null;
			CommentsGroups commentsGroupsAlias = null;
			Employee commentsAuthorAlias = null;
			Counterparty counterpartyAlias = null;
			DeliveryPoint deliveryPointAlias = null;



			var UowCounterparty = UoW.RootObject as Counterparty;
			if(UowCounterparty != null) {

				var orderBottles = UoW.Session.QueryOver<Comments>(() => commentAlias)
							 .JoinAlias(() => commentAlias.Author, () => commentsAuthorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .JoinAlias(() => commentAlias.CommentsGroups, () => commentsGroupsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .JoinAlias(() => commentAlias.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .JoinAlias(() => commentAlias.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .SelectList(list => list
								.Select(() => commentAlias.Id).WithAlias(() => resultAlias.Id)
								.Select(() => commentsGroupsAlias.Name).WithAlias(() => resultAlias.Group)
								.Select(() => commentsAuthorAlias.LastName).WithAlias(() => resultAlias.Author)
								.Select(() => commentAlias.Text).WithAlias(() => resultAlias.Text)
								.Select(() => commentAlias.CreateDate).WithAlias(() => resultAlias.Date)
								.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counteparty)
								.Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.DeliveryPoint)
								.Select(() => commentAlias.IsFixed).WithAlias(() => resultAlias.IsFixed)
										)
									  .Where(() => counterpartyAlias.Name == UowCounterparty.Name &&
											 (commentAlias.AncorPoint == CommentsAncorPoint.Counterparty))
									  .TransformUsing(Transformers.AliasToBean<CommentsVMNode>())
					.List<CommentsVMNode>();

				SetItemsSource(orderBottles.ToList());
			}

			var UowDeliveryPoint = UoW.RootObject as DeliveryPoint;
			if(UowDeliveryPoint != null) {
				var orderBottles = UoW.Session.QueryOver<Comments>(() => commentAlias)
							 .JoinAlias(() => commentAlias.Author, () => commentsAuthorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .JoinAlias(() => commentAlias.CommentsGroups, () => commentsGroupsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .JoinAlias(() => commentAlias.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .JoinAlias(() => commentAlias.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .SelectList(list => list
								.Select(() => commentAlias.Id).WithAlias(() => resultAlias.Id)
								.Select(() => commentsGroupsAlias.Name).WithAlias(() => resultAlias.Group)
								.Select(() => commentsAuthorAlias.LastName).WithAlias(() => resultAlias.Author)
								.Select(() => commentAlias.Text).WithAlias(() => resultAlias.Text)
								.Select(() => commentAlias.CreateDate).WithAlias(() => resultAlias.Date)
								.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counteparty)
								.Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.DeliveryPoint)
								.Select(() => commentAlias.IsFixed).WithAlias(() => resultAlias.IsFixed)
										)
									  .Where(() => deliveryPointAlias.ShortAddress == UowDeliveryPoint.ShortAddress &&
											 (commentAlias.AncorPoint == CommentsAncorPoint.DeliveryPoint || commentAlias.AncorPoint == CommentsAncorPoint.Counterparty))
									  .TransformUsing(Transformers.AliasToBean<CommentsVMNode>())
					.List<CommentsVMNode>();

				SetItemsSource(orderBottles.ToList());
			}

			var UowOrder = UoW.RootObject as Order;
			if(UowOrder != null) {

				var orderBottles = UoW.Session.QueryOver<Comments>(() => commentAlias)
							 .JoinAlias(() => commentAlias.Author, () => commentsAuthorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .JoinAlias(() => commentAlias.CommentsGroups, () => commentsGroupsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .JoinAlias(() => commentAlias.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .JoinAlias(() => commentAlias.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
							 .SelectList(list => list
								.Select(() => commentAlias.Id).WithAlias(() => resultAlias.Id)
								.Select(() => commentsGroupsAlias.Name).WithAlias(() => resultAlias.Group)
								.Select(() => commentsAuthorAlias.LastName).WithAlias(() => resultAlias.Author)
								.Select(() => commentAlias.Text).WithAlias(() => resultAlias.Text)
								.Select(() => commentAlias.CreateDate).WithAlias(() => resultAlias.Date)
								.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counteparty)
								.Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.DeliveryPoint)
								.Select(() => commentAlias.IsFixed).WithAlias(() => resultAlias.IsFixed)
										)
									  .Where(() => commentAlias.Order.Id == UowOrder.Id)
									  .TransformUsing(Transformers.AliasToBean<CommentsVMNode>())
					.List<CommentsVMNode>();

				SetItemsSource(orderBottles.ToList());
			}
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<CommentsVMNode>.Create()
					  .AddColumn("№").AddNumericRenderer(node => node.Id)
					  .AddColumn("Группа").AddTextRenderer(node => node.Group)
					  .AddColumn("Автор").AddTextRenderer(node => node.Author)
					  .AddColumn("Текст").AddTextRenderer(node => node.Text)
					  .AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
					  .AddColumn("Точка доставки").AddTextRenderer(node => node.DeliveryPoint)
					  .AddColumn("Контрагент").AddTextRenderer(node => node.Counteparty)
					  .AddColumn("Фиксирован").AddTextRenderer(node => node.IsFixed ? "Да" : "Нет")
					//.Editing(true)
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

	public class CommentsVMNode
	{
		public int Id { get; set; }
		public string Group { get; set; }
		public string Author { get; set; }
		public string Text { get; set; }
		public DateTime Date { get; set; }
		public string DeliveryPoint { get; set; }
		public string Counteparty { get; set; }
		public bool IsFixed { get; set; }
	}
}



//ITdiTab mytab = DialogHelper.FindParentTab(this);
//if(mytab == null)
//return;
// использование WidgetOnDialogBase вместо Gtk позволяет не писать код выше, а сразу обращаться через this к окну
//MyOrmDialog.UoW


