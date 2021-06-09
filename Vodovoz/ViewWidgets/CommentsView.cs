using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NHibernate.Transform;
using QS.DomainModel.UoW;
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
		private CommentsVM _viewModel;

		public CommentsView()
		{
			this.Build();
		}
		
		public void Configure(IUnitOfWork uow)
		{
			_viewModel = new CommentsVM(uow);
			ytreeComments.RepresentationModel = _viewModel;
			ytreeComments.RepresentationModel.UpdateNodes();
		}

		public void Configure(IUnitOfWork uow, Order order)
		{
			_viewModel = new CommentsVM(uow, order);
			ytreeComments.RepresentationModel = _viewModel;
			ytreeComments.RepresentationModel.UpdateNodes();
		}

		public IList<CommentsVMNode> Items { get { return _viewModel.ItemsList as IList<CommentsVMNode>; } }

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			MyTab.TabParent.AddTab(new NuanceDlg(_viewModel.Order), MyTab);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			MyTab.TabParent.AddTab(new NuanceDlg(_viewModel.Order, ytreeComments.GetSelectedId()), MyTab);
		}

		protected void OnYtreeCommentsCursorChanged(object sender, EventArgs e)
		{
			bool selected = ytreeComments.Selection.CountSelectedRows() > 0;
			buttonEdit.Sensitive = selected;
		}
	}



	public class CommentsVM : RepresentationModelWithoutEntityBase<CommentsVMNode>
	{
		public Order Order { get; }

		public CommentsVM(IUnitOfWork uow, Order order) : base(typeof(Comments))
		{
			this.UoW = uow;
			Order = order;
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


			if(UoW.RootObject is Counterparty UowCounterparty) {

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

			if(UoW.RootObject is DeliveryPoint UowDeliveryPoint) {
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

			var UowOrder = Order;
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
