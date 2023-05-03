using QS.Tdi;
using QS.ViewModels.Extension;
using QS.ViewModels;
using QS.DomainModel.UoW;
using QS.Project.Services.FileDialog;
using QS.Services;
using Vodovoz.EntityRepositories;

namespace Vodovoz.ViewModels.Dialogs.Counterparty
{
	public class CloseSupplyToCounterpartyViewModel : EntityWidgetViewModelBase<Domain.Client.Counterparty>, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		public CloseSupplyToCounterpartyViewModel(
			Domain.Client.Counterparty entity,
			ICommonServices commonServices,
			IUnitOfWork uow) : base(entity, commonServices)
		{
			UoW = uow ?? throw new System.ArgumentNullException(nameof(uow));
		}
		
		public bool AskSaveOnClose => throw new System.NotImplementedException();

		public bool CanClose()
		{
			throw new System.NotImplementedException();
		}
	}
}
