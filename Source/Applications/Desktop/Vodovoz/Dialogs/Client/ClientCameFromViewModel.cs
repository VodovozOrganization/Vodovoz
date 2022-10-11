using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using QS.DomainModel.UoW;

namespace Vodovoz.Dialogs.Client
{
	public class ClientCameFromViewModel : EntityTabViewModelBase<ClientCameFrom>
	{
		public ClientCameFromViewModel(IEntityUoWBuilder uoWBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(uoWBuilder, unitOfWorkFactory, commonServices)
		{
			TabName = "Откуда клиент знает о нас";
		}
	}
}