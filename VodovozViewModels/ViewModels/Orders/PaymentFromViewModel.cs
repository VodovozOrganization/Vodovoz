using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class PaymentFromViewModel : EntityTabViewModelBase<PaymentFrom>, IAskSaveOnCloseViewModel
	{
		public PaymentFromViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IOrganizationJournalFactory organizationJournalFactory) : base(uoWBuilder, uowFactory, commonServices)
		{
			OrganizationSelectorFactory =
				(organizationJournalFactory ?? throw new ArgumentNullException(nameof(organizationJournalFactory)))
				.CreateOrganizationsForAvangardPaymentsAutocompleteSelectorFactory();
		}

		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate && Entity.Id == 0);
		public IEntityAutocompleteSelectorFactory OrganizationSelectorFactory { get; }
		public bool AskSaveOnClose => CanEdit;
	}
}
