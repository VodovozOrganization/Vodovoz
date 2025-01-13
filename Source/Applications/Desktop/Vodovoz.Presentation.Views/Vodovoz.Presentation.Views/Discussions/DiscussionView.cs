using QS.DomainModel.Entity;
using QS.ViewModels;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Presentation.ViewModels.Discussions;
using VodovozBusiness.Domain.Discussions;

namespace Vodovoz.Presentation.Views.Discussions
{
	[ToolboxItem(true)]
	public partial class DiscussionView
		: WidgetViewBase<WidgetViewModelBase>
		//public partial class DiscussionView<
		//		TDiscussionViewModel,
		//		TDiscussionContainer,
		//		TDiscussion,
		//		TDiscussionComment,
		//		TFileInformation>
		//	: WidgetViewBase<TDiscussionViewModel>
		//	where TDiscussionContainer : IDomainObject
		//	where TDiscussionComment
		//		: class,
		//		IDiscussionComment<TFileInformation>,
		//		new()
		//	where TDiscussion
		//		: class,
		//		IDiscussion<TDiscussionContainer, TDiscussionComment, TFileInformation>
		//	where TFileInformation : FileInformation
		//	where TDiscussionViewModel
		//		: DiscussionViewModel<
		//			TDiscussionContainer,
		//			TDiscussion,
		//			TDiscussionComment,
		//			TFileInformation>
	{
		public DiscussionView()
		{
			Build();
		}
	}
}
