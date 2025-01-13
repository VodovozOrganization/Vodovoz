using Gamma.Binding;
using Gamma.Binding.Core.LevelTreeConfig;
using Gtk;
using QS.Views.GtkUI;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Presentation.ViewModels.Discussions;
using Vodovoz.Presentation.Views.Themes;
using VodovozBusiness.Domain.Discussions;

namespace Vodovoz.Presentation.Views.Discussions
{
	[ToolboxItem(true)]
	public partial class DiscussionView : WidgetViewBase<DiscussionViewModel>
	{
		public DiscussionView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			if(ViewModel is null)
			{
				return;
			}

			ViewModel.CommentsCollectionChanged -= OnCommentsCollectionChanged;
			ytreeviewComments.RowActivated -= YtreeviewComments_RowActivated;

			ytreeviewComments.CreateFluentColumnsConfig<IDiscussionComment<DiscussionCommentFileInformation>>()
				.AddColumn("Время")
					.AddTextRenderer(x => GetTime(x))
				.AddColumn("Автор")
					.AddTextRenderer(x => GetAuthor(x))
				.AddColumn("Комментарий")
					.AddTextRenderer(x => GetNodeName(x))
				.WrapWidth(300)
					.WrapMode(Pango.WrapMode.WordChar)
				.RowCells().AddSetter<CellRenderer>(SetColor)
				.Finish();

			var levels = LevelConfigFactory
				.FirstLevel<IDiscussionComment<DiscussionCommentFileInformation>, DiscussionCommentFileInformation>(x => x.AttachedFileInformations)
				.LastLevel(afi => ViewModel.Discussion.Comments.FirstOrDefault(c => c.Id == afi.Id))
				.EndConfig();

			ytreeviewComments.YTreeModel = new LevelTreeModel<IDiscussionComment<DiscussionCommentFileInformation>>(ViewModel.Discussion.Comments, levels);

			ytreeviewComments.ExpandAll();
			ytreeviewComments.RowActivated += YtreeviewComments_RowActivated;

			ViewModel.CommentsCollectionChanged += OnCommentsCollectionChanged;

			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.NewCommentText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComment.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ybuttonAddComment.Clicked += (sender, e) => ViewModel.AddCommentCommand.Execute();
			ybuttonAddComment.Binding.AddBinding(ViewModel, vm => vm.CanAddComment, w => w.Sensitive).InitializeFromSource();

			smallfileinformationsview.ViewModel = ViewModel.AttachedFileInformationsViewModel;
		}

		private string GetNodeName(object node)
		{
			if(node is IDiscussionComment<DiscussionCommentFileInformation> discussionComment)
			{
				return discussionComment.Comment;
			}
			if(node is DiscussionCommentFileInformation fileInformation)
			{
				return fileInformation.FileName;
			}
			return "";
		}

		private string GetTime(object node)
		{
			if(node is IDiscussionComment<DiscussionCommentFileInformation> discussionComment)
			{
				return discussionComment.CreationTime.ToShortDateString() + "\n" +
					discussionComment.CreationTime.ToShortTimeString();
			}

			return "";
		}

		private string GetAuthor(object node)
		{
			if(node is IDiscussionComment<DiscussionCommentFileInformation> discussionComment)
			{
				var author = discussionComment.Author;
				var subdivisionName = author.Subdivision != null
					&& !string.IsNullOrWhiteSpace(author.Subdivision.ShortName) ? "\n" + author.Subdivision.ShortName : "";
				var result = $"{author.ShortName}{subdivisionName}";
				return result;
			}
			return "";
		}

		private void SetColor(CellRenderer cell, object node)
		{
			if(node is IDiscussionComment<DiscussionCommentFileInformation>)
			{
				cell.CellBackgroundGdk = GdkColors.DiscussionCommentBase;
			}
			else
			{
				cell.CellBackgroundGdk = GdkColors.PrimaryBase;
			}
		}

		private void YtreeviewComments_RowActivated(object o, RowActivatedArgs args)
		{
			if(!(ytreeviewComments.GetSelectedObject() is IDiscussionComment<DiscussionCommentFileInformation> discussionCommentFileInformation))
			{
				return;
			}
			ViewModel.OpenFileCommand.Execute(discussionCommentFileInformation);
		}

		private void OnCommentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ytreeviewComments.YTreeModel.EmitModelChanged();
			ytreeviewComments.ExpandAll();
		}

		override public void Destroy()
		{
			ViewModel.CommentsCollectionChanged -= OnCommentsCollectionChanged;
			ytreeviewComments.RowActivated -= YtreeviewComments_RowActivated;
			base.Destroy();
		}
	}
}
