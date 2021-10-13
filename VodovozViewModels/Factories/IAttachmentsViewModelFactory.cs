using System.Collections.Generic;
using QS.Attachments.Domain;
using QS.Attachments.ViewModels.Widgets;

namespace Vodovoz.Factories
{
	public interface IAttachmentsViewModelFactory
	{
		AttachmentsViewModel CreateNewAttachmentsViewModel(IList<Attachment> attachments);
	}
}
