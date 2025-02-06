using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Extensions;
using VodovozBusiness.Domain.Cash.Payments;

namespace Vodovoz.ViewModels.Cash.Payments
{
	public class OutgoingPaymentEditViewModel : EntityTabViewModelBase<OutgoingPayment>
	{
		private Counterparty _counterparty;

		public OutgoingPaymentEditViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			SaveCommand = new DelegateCommand(SaveAndClose, () => PermissionResult.CanUpdate);
			CancelCommand = new DelegateCommand(() => Close(HasChanges && PermissionResult.CanUpdate, CloseSource.Cancel));
		}

		public Counterparty Counterparty
		{
			get => this.GetIdRefField(ref _counterparty, Entity.CounterpartyId);
			set => this.SetIdRefField(SetField, ref _counterparty, () => Entity.CounterpartyId, value);
		}

		public IEntityEntryViewModel CounterpartyViewModel { get; set; }
		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
	}
}
