using QS.DomainModel.Entity;
using System;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.Domain.Discussions
{
	public interface IDiscussionComment<TFileInformation>
		: IDomainObject, IHasAttachedFilesInformations<TFileInformation>
		where TFileInformation : DiscussionCommentFileInformation
	{
		Employee Author { get; set; }
		string Comment { get; set; }
		DateTime CreationTime { get; set; }
		string Title { get; }

		void AddFileInformation(string fileName);
		void DeleteFileInformation(string fileName);
	}
}
