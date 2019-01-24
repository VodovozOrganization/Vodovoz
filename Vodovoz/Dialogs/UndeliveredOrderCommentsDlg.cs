using System;
using Gamma.GtkWidgets;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.Dialogs
{
	public partial class UndeliveredOrderCommentsDlg : QS.Dialog.Gtk.TdiTabBase
	{
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

		public UndeliveredOrderCommentsDlg(IUnitOfWork uow, int id, CommentedFields field, string valueOfField)
		{
			UoW = uow;
			Field = field;
			UndeliveredOrder = uow.GetById<UndeliveredOrder>(id);
			Employee = EmployeeRepository.GetEmployeeForCurrentUser(uow);
			if(Employee == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику и вы не можете комментировать недовозы, так как некого указывать в качестве автора.");
				FailInitialize = true;
				return;
			}
			this.Build();
			TabName = "Добавить комментарий";
			lblValOfField.Text = valueOfField;
			ConfigureDlg();
		}

		void CreateComment(){
			Comment = new UndeliveredOrderComment();
			Comment.UndeliveredOrder = UndeliveredOrder;
			Comment.CommentedField = Field;
			Comment.Employee = Employee;
			Comment.CommentDate = DateTime.Now;
			Comment.Comment = txtAddComment.Buffer.Text;
			txtAddComment.Buffer.Text = String.Empty;
		}

		public void ConfigureDlg()
		{
			yEnumCMBField.Sensitive = false;
			yEnumCMBField.ItemsEnum = typeof(CommentedFields);
			yEnumCMBField.SelectedItem = Field;
			yTreeComments.ColumnsConfig = ColumnsConfigFactory.Create<UndeliveredOrderCommentsNode>()
				.AddColumn("Дата - Имя")
					.AddTextRenderer(n => n.UserDateAndName, useMarkup: true)
				.AddColumn("Комментарий")
					.AddTextRenderer(n => n.MarkedupComment, useMarkup: true)
					.WrapWidth(450).WrapMode(Pango.WrapMode.WordChar)
				.Finish();
			GetComments();
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

		protected void OnBtnCloseClicked(object sender, EventArgs e)
		{
			this.OnCloseTab(false);
		}
	}
}
