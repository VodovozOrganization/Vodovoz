using Vodovoz.ViewModels.Comments;
using Vodovoz.Domain.Comments;
using Vodovoz.EntityRepositories.Employees;
using QS.DomainModel.UoW;

namespace Vodovoz.Views.Comments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DocumentCommentView : Gtk.Bin
	{
		DocumentCommentViewModel ViewModel { get; set; }

		public DocumentCommentView(ICommentedDocument commentedDocument, IEmployeeRepository employeeRepository, IUnitOfWork uow)
		{
			this.Build();
			ViewModel = new DocumentCommentViewModel(commentedDocument, employeeRepository, uow);
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ytextviewComments.Binding.AddBinding(ViewModel, vm => vm.Comment, v => v.Buffer.Text).InitializeFromSource();
			ytextviewLastComment.Binding.AddBinding(ViewModel, vm => vm.CurComment, v => v.Buffer.Text).InitializeFromSource();

			ybuttonAddComment.Clicked += (sender, e) => ViewModel.AddCommentCommand.Execute(ytextviewLastComment.Buffer.Text);
			ybuttonRevertComment.Clicked += (sender, e) => ViewModel.RevertLastCommentCommand.Execute();
		}
	}
}
