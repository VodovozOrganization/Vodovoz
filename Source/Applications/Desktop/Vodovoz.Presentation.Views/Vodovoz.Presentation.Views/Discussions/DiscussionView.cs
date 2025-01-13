using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.Presentation.ViewModels.Discussions;

namespace Vodovoz.Presentation.Views.Discussions
{
	[ToolboxItem(true)]
	public partial class DiscussionView : WidgetViewBase<DiscussionViewModel>
	{
		public DiscussionView()
		{
			Build();
		}
	}
}
