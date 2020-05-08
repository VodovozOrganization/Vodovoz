using System;
using Vodovoz.ViewModels.Comments;
using Vodovoz.Domain.Client;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Comments;
namespace Vodovoz.Views.Comments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DocumentCommentView : Gtk.Bin
	{

		DocumentCommentViewModel ViewModel { get; set; }



		public DocumentCommentView()
		{
			this.Build();
			ViewModel = new DocumentCommentViewModel();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ytextviewComments.Binding.AddBinding(ViewModel, vm => vm.Comment, v => v.Buffer.Text).InitializeFromSource();
			ytextviewLastComment.Binding.AddBinding(ViewModel, vm => vm.LastComment, v => v.Buffer.Text).InitializeFromSource();

			ybuttonAddComment.Clicked += (sender, e) => ViewModel.AddCommentCommand.Execute(ytextviewLastComment.Buffer.Text);
			ybuttonRevertComment.Clicked += (sender, e) => ViewModel.RevertLastCommentCommand.Execute(ViewModel.LastComment);
		}
	}
}
