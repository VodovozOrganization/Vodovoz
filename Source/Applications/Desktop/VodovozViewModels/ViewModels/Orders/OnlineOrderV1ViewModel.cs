using Autofac;
using Microsoft.Extensions.Logging;
using QS.Navigation;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Mango;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Factories;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	/// <summary>
	/// Вью модель для онлайн заказа 1 версии
	/// </summary>
	public class OnlineOrderV1ViewModel : OnlineOrderViewModel
	{
		public OnlineOrderV1ViewModel(
			ILogger<OnlineOrderV1ViewModel> logger,
			ILifetimeScope scope,
			IGtkTabsOpener gtkTabsOpener,
			IEntityViewModelContext viewModelContext,
			ICommonServices commonServices,
			INavigationManager navigation,
			IEmployeeService employeeService,
			IOnlineOrderValidatorCreator onlineOrderValidatorCreator,
			IExternalCounterpartyMatchingRepository externalCounterpartyMatchingRepository,
			ViewModelEEVMBuilder<DeliveryPoint> deliveryPointViewModelBuilder,
			DeliveryPointJournalFilterViewModel deliveryPointJournalFilterViewModel,
			IDiscountController discountController,
			IOrderOrganizationManager orderOrganizationManager,
			MangoManager mangoManager
		)
			: base(
				logger,
				scope,
				gtkTabsOpener,
				viewModelContext,
				commonServices,
				navigation,
				employeeService,
				onlineOrderValidatorCreator,
				externalCounterpartyMatchingRepository,
				deliveryPointViewModelBuilder,
				deliveryPointJournalFilterViewModel,
				discountController,
				orderOrganizationManager,
				mangoManager
			)
		{
			Initialize();
		}
	
		public new OnlineOrderV1 Entity => (OnlineOrderV1)entity;
	}
}
