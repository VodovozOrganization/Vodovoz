using Vodovoz.Core.Domain.Common;

namespace VodovozBusiness.Domain.Discussions
{
	public abstract class DiscussionCommentFileInformation : FileInformation
	{
		public abstract int ContainerId { get; }
	}
}
