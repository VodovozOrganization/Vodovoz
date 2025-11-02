using System;
using Autofac;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PermissionCommentView : Gtk.Bin
	{
		private IEmployeeRepository _employeeRepository;
		public IUnitOfWork UoW { get; set; }

		public bool AddCommentInfo { get; set; } = false;

		public event Action<string> CommentChanged;

		private string comment;
		public string Comment { get { return comment; } set { comment = value; UpdateUI(); } }

		private string title;
		public string Title { get { return title; } set { title = value; labelTitle.LabelProp = value; } }

		public string PermissionName { get; set; }

		public PermissionCommentView()
		{
			_employeeRepository = ScopeProvider.Scope.Resolve<IEmployeeRepository>();
			Build();
			ytextviewComment.Buffer.Changed += (sender, args) => 
				CommentChanged?.Invoke(Comment);
		}

		public void UpdateUI()
		{
			bool isCommentEmpty = String.IsNullOrWhiteSpace(Comment);

			ytextviewComment.Buffer.Text = Comment;
			ytextviewComment.Sensitive = isCommentEmpty;
			buttonSaveComment.Sensitive = isCommentEmpty;
			buttonEditComment.Sensitive = !isCommentEmpty;
		}

		private bool CheckPermissions()
		{
			if(UoW == null)
				throw new ArgumentNullException($"Необходимо установить UnitOfWork для {nameof(PermissionCommentView)}");
			if(String.IsNullOrWhiteSpace(PermissionName))
				throw new ArgumentNullException($"Необходимо установить PermissionName для {nameof(PermissionCommentView)}");

			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_cashier_review_comment")) 
			{
				MessageDialogHelper.RunWarningDialog("У вас нет прав для изменения/создания комментария");
				return false;
			}

			return true;
		}

		public void Save()
		{
			OnButtonSaveCloseCommentClicked(buttonSaveComment, EventArgs.Empty);
		}

		protected void OnButtonSaveCloseCommentClicked(object sender, EventArgs e)
		{
			if(!CheckPermissions())
				return;

			var employee = _employeeRepository.GetEmployeeForCurrentUser(UoW);

			if(AddCommentInfo)
				Comment = employee.ShortName + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + ": " + ytextviewComment.Buffer.Text;
			else
				Comment = ytextviewComment.Buffer.Text;

			Comment = ytextviewComment.Buffer.Text;
			CommentChanged?.Invoke(Comment);
		}

		protected void OnButtonEditCloseDeliveryCommentClicked(object sender, EventArgs e)
		{
			if(!CheckPermissions())
				return;
			if(!MessageDialogHelper.RunQuestionDialog("Вы уверены что хотите изменить комментарий (преведущий комментарий будет удален)?"))
				return;

			Comment = String.Empty;
			CommentChanged?.Invoke(comment);
		}

		public override void Destroy()
		{
			_employeeRepository = null;
			base.Destroy();
		}
	}
}
