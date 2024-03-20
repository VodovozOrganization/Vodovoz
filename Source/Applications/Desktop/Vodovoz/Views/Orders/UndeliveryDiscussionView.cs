using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Orders;
namespace Vodovoz.Views.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UndeliveryDiscussionView : WidgetViewBase<UndeliveryDiscussionViewModel>
	{
		public UndeliveryDiscussionView(UndeliveryDiscussionViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureWidget();
		}

		protected override void ConfigureWidget()
		{
			ylabelSubdivision.Binding.AddFuncBinding(ViewModel.Entity, e => $"<b>{e.Subdivision.Name}</b>", w => w.LabelProp).InitializeFromSource();
			ylabelConnectTime.Binding.AddFuncBinding(ViewModel.Entity, e => $"подключен {e.StartSubdivisionDate:dd.MM.yy HH:mm}", w => w.LabelProp).InitializeFromSource();

			ydatepickerPlannedCompletionDate.Binding.AddBinding(ViewModel.Entity, e => e.PlannedCompletionDate, w => w.Date).InitializeFromSource();
			ydatepickerPlannedCompletionDate.Binding.AddBinding(ViewModel, vm => vm.CanEditDate, w => w.Sensitive).InitializeFromSource();
			
			yenumcomboStatus.ItemsEnum = typeof(UndeliveryDiscussionStatus);
			yenumcomboStatus.Binding
				.AddBinding(ViewModel.Entity, e => e.Status, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEditStatus, w => w.Sensitive)
				.InitializeFromSource();

			UpdateStatusEnum();

			ytreeviewComments.ShowExpanders = false;
			ytreeviewComments.ColumnsConfig = FluentColumnsConfig<UndeliveryDiscussionComment>.Create()
				.AddColumn("Время")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => GetTime(n), useMarkup: true)
				.AddColumn("Автор")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => GetAuthor(n), useMarkup: true)
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Comment, useMarkup: true)
						.WrapWidth(300)
						.WrapMode(Pango.WrapMode.WordChar)
				.RowCells().AddSetter<CellRenderer>(SetColor)
				.Finish();

			ytreeviewComments.ItemsDataSource = ViewModel.Entity.ObservableComments;			
			ytreeviewComments.ExpandAll();

			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.NewCommentText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonAddComment.Clicked += (sender, e) => ViewModel.AddCommentCommand.Execute();
			ybuttonAddComment.Binding.AddBinding(ViewModel, vm => vm.CanAddComment, w => w.Sensitive).InitializeFromSource();

			ViewModel.PropertyChanged += (sender, e) =>
			{
				if(e.PropertyName == nameof(ViewModel.CanEditStatus))
				{
					UpdateStatusEnum();
				}
			};

			filesView.Sensitive = false;
		}

		private void SetColor(CellRenderer cell, object node)
		{
			cell.CellBackgroundGdk = GdkColors.DiscussionCommentBase;
		}

		private string GetColor(UndeliveryDiscussionComment n) => ViewModel.Entity.ObservableComments.IndexOf(n) % 2 == 0 ? GdkColors.InfoText.ToHtmlColor() : GdkColors.DangerText.ToHtmlColor();

		private void UpdateStatusEnum()
		{
			yenumcomboStatus.ClearEnumHideList();

			if(!ViewModel.CanCompleteDiscussion)
			{
				yenumcomboStatus.AddEnumToHideList(ViewModel.HiddenDiscussionStatuses.Cast<object>().ToArray());
			}

			yenumcomboStatus.SelectedItem = ViewModel.Entity.Status;
		}

		private string GetTime(UndeliveryDiscussionComment comment) =>
			comment.CreationTime.ToShortDateString() + "\n" + comment.CreationTime.ToShortTimeString();

		private string GetAuthor(object node)
		{
			if(node is UndeliveryDiscussionComment)
			{
				var author = (node as UndeliveryDiscussionComment).Author;
				var subdivisionName = author.Subdivision != null && !string.IsNullOrWhiteSpace(author.Subdivision.ShortName) ? "\n" + author.Subdivision.ShortName : "";
				var result = $"{author.GetPersonNameWithInitials()}{subdivisionName}";
				return result;
			}
			return "";
		}
	}
}
