using System.ComponentModel;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Complaints;
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

			ytreeviewComments.CreateFluentColumnsConfig<IDiscussionComment<FileInformation>>()
				.AddColumn("Время")
					.AddTextRenderer(x => x.CreationTime.ToShortDateString())
				.AddColumn("Автор")
					.AddTextRenderer(x => x.Author.ShortName)
				.AddColumn("Комментарий")
					.AddTextRenderer(x => x.Comment)
				.WrapWidth(300)
					.WrapMode(Pango.WrapMode.WordChar)
				.RowCells().AddSetter<CellRenderer>(SetColor)
				.Finish();
		}

		private void SetColor(CellRenderer cell, object node)
		{
			if(node is ComplaintDiscussionComment)
			{
				cell.CellBackgroundGdk = GdkColors.DiscussionCommentBase;
			}
			else
			{
				cell.CellBackgroundGdk = GdkColors.PrimaryBase;
			}
		}
	}
}
