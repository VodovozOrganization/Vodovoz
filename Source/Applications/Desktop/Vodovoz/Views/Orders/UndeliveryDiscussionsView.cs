using System;
using System.Data.Bindings.Collections.Generic;
using Gamma.GtkWidgets;
using QS.DomainModel.UoW;
using QS.Views.GtkUI;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.Views.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveryDiscussionsView : WidgetViewBase<UndeliveryDiscussionsViewModel>
	{
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IUndeliveredOrderCommentsRepository _undeliveredOrderCommentsRepository = new UndeliveredOrderCommentsRepository();
		
		public UndeliveryDiscussionsView()
		{
			this.Build();
		}

		IUnitOfWork UoW { get; set; }
		UndeliveredOrderComment Comment { get; set; }
		GenericObservableList<UndeliveredOrderCommentsNode> Comments { get; } = new GenericObservableList<UndeliveredOrderCommentsNode>();

		CommentedFields Field { get; set; }
		UndeliveredOrder UndeliveredOrder { get; set; }
		Employee Employee { get; set; }

		public event EventHandler<EventArgs> CommentAdded;

		void OnCommentAdded()
		{
			if(CommentAdded != null)
			{
				CommentAdded(this, new EventArgs());
			}
		}

		public void Configure(IUnitOfWork uow, UndeliveredOrder undeliveredOrder, CommentedFields field)
		{
			UoW = uow;
			UndeliveredOrder = undeliveredOrder;
			Employee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			Field = field;
			
			UpdateCommentsTreeview();
		}

		void CreateComment()
		{
			Comment = new UndeliveredOrderComment();
			Comment.UndeliveredOrder = UndeliveredOrder;
			Comment.CommentedField = Field;
			Comment.Employee = Employee;
			Comment.CommentDate = DateTime.Now;
			Comment.Comment = txtAddComment.Buffer.Text;
			txtAddComment.Buffer.Text = string.Empty;
		}

		void UpdateCommentsTreeview()
		{
			var comments = _undeliveredOrderCommentsRepository.GetCommentNodes(UoW, UndeliveredOrder, Field);

			Comments.Clear();

			foreach(var comment in comments)
			{
				Comments.Add(comment);
			}

			yTreeComments.ColumnsConfig = ColumnsConfigFactory.Create<UndeliveredOrderCommentsNode>()
				.AddColumn("Дата - Имя")
					.AddTextRenderer(c => "", useMarkup: true)
					.AddSetter((c, n) =>
					{
						var color = Comments.IndexOf(n) % 2 == 0 ? GdkColors.InfoText.ToHtmlColor() : GdkColors.DangerText.ToHtmlColor();
						c.Markup = $"<span foreground=\"{color}\"><b>{n.UserDateAndName}</b></span>";
					})
				.AddColumn("Комментарий")
					.AddTextRenderer(n => "", useMarkup: true)
					.AddSetter((c, n) =>
					{
						var color = Comments.IndexOf(n) % 2 == 0 ? GdkColors.InfoText.ToHtmlColor() : GdkColors.DangerText.ToHtmlColor();
						c.Markup = $"<span foreground=\"{color}\"><b>{n.Comment}</b></span>";
					})
					.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.Finish();

			yTreeComments.ItemsDataSource = Comments;
		}

		protected void OnBtnAddCommentClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(txtAddComment.Buffer.Text))
			{
				return;
			}

			CreateComment();
			UoW.Save(Comment);
			UoW.Commit();
			UpdateCommentsTreeview();
			Comment = null;
			OnCommentAdded();
		}

		protected void OnBtnShowHideClicked(object sender, EventArgs e)
		{
			mainBox.Visible = !mainBox.Visible;
			btnShowHide.Label = mainBox.Visible ? "<" : ">";
		}
	}
}
