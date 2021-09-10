using QS.DomainModel.UoW;
using Vodovoz.Domain.Attachments;
using Vodovoz.EntityRepositories;
using Vodovoz.Factories;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Attachments;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Factories
{
	public class AttachmentsViewModelFactory : IAttachmentsViewModelFactory
	{
		private readonly IUserRepository _userRepository = new UserRepository();
		private readonly IUnitOfWorkFactory _uowFactory = UnitOfWorkFactory.GetDefaultFactory;

		public AttachmentsViewModel CreateNewAttachmentsViewModel(
			IFileChooserProvider fileChooserProvider, IScanDialog scanDialog, EntityType entityType, int entityId = -1) =>
				new AttachmentsViewModel(_uowFactory, fileChooserProvider, scanDialog, _userRepository, entityType, entityId);
	}
}
