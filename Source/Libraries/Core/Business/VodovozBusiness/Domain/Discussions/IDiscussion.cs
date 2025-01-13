using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace VodovozBusiness.Domain.Discussions
{
	public interface IDiscussion
	{
		IDomainObject Container { get; set; }
		IObservableList<IDiscussionComment<DiscussionCommentFileInformation>> Comments { get; }

		void AddComment(IDiscussionComment<DiscussionCommentFileInformation> discussionComment);
	}
}
