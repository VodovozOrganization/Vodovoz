using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;
using Gamma.ColumnConfig;
using Gamma.Binding;
using System.Linq;
using Gtk;
using System;
using Gamma.Binding.Core.LevelTreeConfig;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintDiscussionView : WidgetViewBase<ComplaintDiscussionViewModel>
	{
		public ComplaintDiscussionView(ComplaintDiscussionViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ylabelSubdivision.Binding.AddFuncBinding(ViewModel.Entity, e => $"<b>{e.Subdivision.Name}</b>", w => w.LabelProp).InitializeFromSource();
			ylabelConnectTime.Binding.AddFuncBinding(ViewModel.Entity, e => $"подключен {e.StartSubdivisionDate:dd.MM.yy HH:mm}", w => w.LabelProp).InitializeFromSource();

			ydatepickerPlannedCompletionDate.Binding.AddBinding(ViewModel.Entity, e => e.PlannedCompletionDate, w => w.Date).InitializeFromSource();
			ydatepickerPlannedCompletionDate.Binding.AddBinding(ViewModel, vm => vm.CanEditDate, w => w.Sensitive).InitializeFromSource();

			ViewModel.PropertyChanged += (sender, e) => {
				if(e.PropertyName == nameof(ViewModel.CanEditStatus)) {
					UpdateStatusEnum();
				}
			};
			yenumcomboStatus.ItemsEnum = typeof(ComplaintDiscussionStatuses);
			yenumcomboStatus.Binding.AddBinding(ViewModel.Entity, e => e.Status, w => w.SelectedItem).InitializeFromSource();
			yenumcomboStatus.Binding.AddBinding(ViewModel, vm => vm.CanEditStatus, w => w.Sensitive).InitializeFromSource();
			UpdateStatusEnum();

			ytreeviewComments.ShowExpanders = false;
			ytreeviewComments.ColumnsConfig = FluentColumnsConfig<object>.Create()
				.AddColumn("Время")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => GetTime(x))
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => GetAuthor(x))
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => GetNodeName(x))
						.WrapWidth(300)
						.WrapMode(Pango.WrapMode.WordChar)
				.RowCells().AddSetter<CellRenderer>(SetColor)
				.Finish();
			var levels = LevelConfigFactory.FirstLevel<ComplaintDiscussionComment, ComplaintFile>(x => x.ComplaintFiles).LastLevel(c => c.ComplaintDiscussionComment).EndConfig();
			ytreeviewComments.YTreeModel = new LevelTreeModel<ComplaintDiscussionComment>(ViewModel.Entity.Comments, levels);

			ViewModel.Entity.ObservableComments.ListContentChanged += (sender, e) => {
				ytreeviewComments.YTreeModel.EmitModelChanged();
				ytreeviewComments.ExpandAll();
			};
			ytreeviewComments.ExpandAll();
			ytreeviewComments.RowActivated += YtreeviewComments_RowActivated;

			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.NewCommentText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			filesview.ViewModel = ViewModel.FilesViewModel;
			ViewModel.FilesViewModel.ReadOnly = !ViewModel.CanEdit;

			ybuttonAddComment.Clicked += (sender, e) => ViewModel.AddCommentCommand.Execute();
			ybuttonAddComment.Binding.AddBinding(ViewModel, vm => vm.CanAddComment, w => w.Sensitive).InitializeFromSource();
		}

		private void UpdateStatusEnum()
		{
			yenumcomboStatus.ClearEnumHideList();
			if(!ViewModel.CanCompleteDiscussion) {
				yenumcomboStatus.AddEnumToHideList(ViewModel.HiddenDiscussionStatuses.Cast<object>().ToArray());
			}
			yenumcomboStatus.SelectedItem = ViewModel.Entity.Status;
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

		private string GetTime(object node)
		{
			if(node is ComplaintDiscussionComment) {
				return (node as ComplaintDiscussionComment).CreationTime.ToShortDateString() + "\n" + (node as ComplaintDiscussionComment).CreationTime.ToShortTimeString();
			}

			return "";
		}

		private string GetAuthor(object node)
		{
			if(node is ComplaintDiscussionComment) {
				var author = (node as ComplaintDiscussionComment).Author;
				var subdivisionName = author.Subdivision != null && !string.IsNullOrWhiteSpace(author.Subdivision.ShortName) ? "\n" + author.Subdivision.ShortName : "";
				var result = $"{author.GetPersonNameWithInitials()}{subdivisionName}";
				return result;
			}
			return "";
		}

		private void SetColor(CellRenderer cell, object node)
		{
			if(node is ComplaintDiscussionComment) {
				cell.CellBackgroundGdk = new Gdk.Color(230, 230, 245);
			} else {
				cell.CellBackgroundGdk = new Gdk.Color(255, 255, 255);
			}
		}
	}
}
