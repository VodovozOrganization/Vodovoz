using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Pacs;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Orders
{
	public class PaymentFromViewModel : EntityTabViewModelBase<PaymentFrom>, IAskSaveOnCloseViewModel
	{
		public PaymentFromViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IPaymentFromRepository paymentFromRepository,
			IOrderSettings orderSettings,
			ViewModelEEVMBuilder<Organization> organizationViewModelEEVMBuilder)
			: base(uoWBuilder, uowFactory, commonServices)
		{
			if(paymentFromRepository is null)
			{
				throw new ArgumentNullException(nameof(paymentFromRepository));
			}

			if(organizationViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(organizationViewModelEEVMBuilder));
			}

			CanShowOrganization = true;
			ValidationContext.ServiceContainer.AddService(typeof(IPaymentFromRepository), paymentFromRepository);
			ValidationContext.ServiceContainer.AddService(typeof(IOrderSettings), orderSettings);

			OrganizationViewModel = organizationViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.OrganizationForOnlinePayments)
				.UseViewModelJournalAndAutocompleter<OperatorsJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();
		}

		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate && Entity.Id == 0);
		public bool AskSaveOnClose => CanEdit;
		public bool CanShowOrganization { get; }
		public IEntityEntryViewModel OrganizationViewModel { get; }
	}
}
