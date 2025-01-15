using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Extension;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Settings.Orders;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Orders
{
	public class PaymentFromViewModel : EntityTabViewModelBase<PaymentFrom>, IAskSaveOnCloseViewModel
	{
		public PaymentFromViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IPaymentFromRepository paymentFromRepository,
			IOrderSettings orderSettings) : base(uoWBuilder, uowFactory, commonServices)
		{
			if(paymentFromRepository is null)
			{
				throw new ArgumentNullException(nameof(paymentFromRepository));
			}

			CanShowOrganization = true;
			ValidationContext.ServiceContainer.AddService(typeof(IPaymentFromRepository), paymentFromRepository);
			ValidationContext.ServiceContainer.AddService(typeof(IOrderSettings), orderSettings);
		}

		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate && Entity.Id == 0);
		public IEntityAutocompleteSelectorFactory OrganizationSelectorFactory { get; }
		public bool AskSaveOnClose => CanEdit;
		public bool CanShowOrganization { get; }
	}
}
