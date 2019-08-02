using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;
using Gamma.ColumnConfig;
using Gamma.Binding;
using System.Linq;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintDiscussionView : EntityWidgetViewBase<ComplaintDiscussionViewModel>
	{
		public ComplaintDiscussionView(ComplaintDiscussionViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ylabelSubdivision.Binding.AddFuncBinding(ViewModel.Entity, e => $"<b>{e.Subdivision.Name}</b>", w => w.LabelProp).InitializeFromSource();

			ydatepickerPlannedCompletionDate.Binding.AddBinding(ViewModel.Entity, e => e.PlannedCompletionDate, w => w.Date).InitializeFromSource();
			ydatepickerPlannedCompletionDate.Binding.AddBinding(ViewModel, vm => vm.CanEditDate, w => w.Sensitive).InitializeFromSource();

			ViewModel.PropertyChanged += (sender, e) => { 
				if(e.PropertyName == nameof(ViewModel.CanEditStatus)) {
					yenumcomboStatus.ClearEnumHideList();
					if(!ViewModel.CanEditStatus) {
						yenumcomboStatus.AddEnumToHideList(ViewModel.HiddenStatuses.Cast<object>().ToArray());
					}
				}
			};
			yenumcomboStatus.ItemsEnum = typeof(ComplaintStatuses);
			yenumcomboStatus.Binding.AddBinding(ViewModel.Entity, e => e.Status, w => w.SelectedItem).InitializeFromSource();
			yenumcomboStatus.Binding.AddBinding(ViewModel, vm => vm.CanEditStatus, w => w.Sensitive).InitializeFromSource();

			ytreeviewComments.ColumnsConfig = FluentColumnsConfig<object>.Create()
				.AddColumn("Комментарий").AddTextRenderer(x => GetNodeName(x))
				.Finish();
			var levels = LevelConfigFactory.FirstLevel<ComplaintDiscussionComment, ComplaintFile>(x => x.Files).LastLevel(c => c.ComplaintDiscussionComment).EndConfig();
			ytreeviewComments.YTreeModel = new LevelTreeModel<ComplaintDiscussionComment>(ViewModel.Entity.ObservableComments, levels);
			ytreeviewComments.Binding.AddBinding(ViewModel.Entity, e => e.ObservableComments, w => w.ItemsDataSource).InitializeFromSource();
			ytreeviewComments.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			ytreeviewComments.RowActivated += YtreeviewComments_RowActivated;

			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.NewCommentText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonAddFiles.Clicked += (sender, e) => ViewModel.AddFilesCommand.Execute();
			ybuttonAddFiles.Binding.AddBinding(ViewModel, vm => vm.CanAddFiles, w => w.Sensitive).InitializeFromSource();

			ybuttonClearFiles.Clicked += (sender, e) => ViewModel.ClearFilesCommand.Execute();
			ybuttonClearFiles.Binding.AddBinding(ViewModel, vm => vm.CanClearFiles, w => w.Sensitive).InitializeFromSource();

			ybuttonAddComment.Clicked += (sender, e) => ViewModel.AddCommentCommand.Execute();
			ybuttonAddComment.Binding.AddBinding(ViewModel, vm => vm.CanAddComment, w => w.Sensitive).InitializeFromSource();
		}

		void YtreeviewComments_RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			ViewModel.OpenFileCommand.Execute(ytreeviewComments.GetSelectedObject() as ComplaintFile);
		}

		private string GetNodeName(object node)
		{
			if(node is ComplaintDiscussionComment) {
				return (node as ComplaintDiscussionComment).Comment;
			}
			if(node is ComplaintFile) {
				return (node as ComplaintFile).FileStorageId;
			}
			return "";
		}
	}
}
