using QS.Tdi;
using QS.ViewModels.Extension;
using QS.ViewModels;
using QS.DomainModel.UoW;
using QS.Project.Services.FileDialog;
using QS.Services;
using Vodovoz.EntityRepositories;
using QS.Navigation;
using QS.Project.Domain;

namespace Vodovoz.ViewModels.Dialogs.Counterparty
{
	public class CloseSupplyToCounterpartyViewModel : EntityTabViewModelBase<Domain.Client.Counterparty>
	{
		public CloseSupplyToCounterpartyViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager) : base(uowBuilder, uowFactory, commonServices, navigationManager)
		{

		}
	}
}
