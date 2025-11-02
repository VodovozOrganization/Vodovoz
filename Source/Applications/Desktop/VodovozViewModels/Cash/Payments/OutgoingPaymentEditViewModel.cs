using DocumentFormat.OpenXml.Bibliography;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Extensions;
using VodovozBusiness.Domain.Payments;

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
			PaymentDate = Entity.PaymentDate.ToString("d");

			if(Entity.OrganizationId.HasValue)
			{
				OrganizationName = UoW.GetById<Organization>(Entity.OrganizationId.Value)?.FullName;
			}

			CashlessRequestId = Entity.CashlessRequestId.ToString();
			PaymentNumber = Entity.PaymentNumber.ToString();
			Sum = Entity.Sum.ToString("C");

			SaveCommand = new DelegateCommand(SaveAndClose, () => PermissionResult.CanUpdate);
			CancelCommand = new DelegateCommand(() => Close(HasChanges && PermissionResult.CanUpdate, CloseSource.Cancel));
		}

		public string PaymentDate { get; }
		public string OrganizationName { get; }
		public string CashlessRequestId { get; }
		public string PaymentNumber { get; }
		public string Sum { get; }

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
