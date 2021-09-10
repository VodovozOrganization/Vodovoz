using Vodovoz.Domain.Attachments;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Attachments;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.Factories
{
	public interface IAttachmentsViewModelFactory
	{
		AttachmentsViewModel CreateNewAttachmentsViewModel(
			IFileChooserProvider fileChooserProvider, IScanDialog scanDialog, EntityType entityType, int entityId = -1);
	}
}
