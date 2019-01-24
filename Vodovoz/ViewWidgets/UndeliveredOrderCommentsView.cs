using System;
using Gamma.GtkWidgets;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveredOrderCommentsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		public UndeliveredOrderCommentsView()
		{
			this.Build();
		}

		IUnitOfWork UoW { get; set; }
		UndeliveredOrderComment Comment { get; set; }
		CommentedFields Field { get; set; }
		UndeliveredOrder UndeliveredOrder { get; set; }
		Employee Employee { get; set; }

		public event EventHandler<EventArgs> CommentAdded;

		void OnCommentAdded()
		{
			if(CommentAdded != null)
				CommentAdded(this, new EventArgs());
		}

		public void Configure(IUnitOfWork uow, UndeliveredOrder undeliveredOrder, CommentedFields field)
		{
			UoW = uow;
			UndeliveredOrder = undeliveredOrder;
			Employee = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Field = field;
			yTreeComments.ColumnsConfig = ColumnsConfigFactory.Create<UndeliveredOrderCommentsNode>()
				.AddColumn("Дата - Имя")
					.AddTextRenderer(n => n.UserDateAndName, useMarkup: true)
				.AddColumn("Комментарий")
					.AddTextRenderer(n => n.MarkedupComment, useMarkup: true)
					.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.Finish();
			GetComments();
		}

		void CreateComment()
		{
			Comment = new UndeliveredOrderComment();
			Comment.UndeliveredOrder = UndeliveredOrder;
			Comment.CommentedField = Field;
			Comment.Employee = Employee;
			Comment.CommentDate = DateTime.Now;
			Comment.Comment = txtAddComment.Buffer.Text;
			txtAddComment.Buffer.Text = String.Empty;
		}

		void GetComments()
		{
			yTreeComments.ItemsDataSource = UndeliveredOrderCommentsRepository.GetCommentNodes(UoW, UndeliveredOrder, Field);
		}

		protected void OnBtnAddCommentClicked(object sender, EventArgs e)
		{
			if(String.IsNullOrWhiteSpace(txtAddComment.Buffer.Text))
				return;
			CreateComment();
			UoW.Save(Comment);
			UoW.Commit();
			GetComments();
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
