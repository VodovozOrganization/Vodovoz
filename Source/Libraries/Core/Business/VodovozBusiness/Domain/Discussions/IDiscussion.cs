using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Common;

namespace VodovozBusiness.Domain.Discussions
{
	public interface IDiscussion<TContainer, TDiscussionComment, TFileInformation>
		: IDomainObject
		where TContainer : IDomainObject
		where TFileInformation : FileInformation
		where TDiscussionComment
			: IDiscussionComment<TFileInformation>
	{
		TContainer Container { get; set; }
		IObservableList<TDiscussionComment> Comments { get; }

		void AddComment(TDiscussionComment discussionComment);
	}
}
