using System;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Tdi;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;

namespace Vodovoz.JournalViewers
{
	//FIXME Временно. Для возможности открывать старые диалоги из отдельного проекта для моделей представления
	public class UndeliveredOrdersJournalOpener : IUndeliveredOrdersJournalOpener
	{
		//отрытие журнала недовоза на конкретном недовозе из диалога штрафов
		public void OpenFromFine(ITdiTab tab, Order oldOrder, DateTime? deliveryDate, UndeliveryStatus undeliveryStatus)
		{
			var undeliveredOrdersFilter = new UndeliveredOrdersFilterViewModel(
				ServicesConfig.CommonServices,
				new OrderSelectorFactory(),
				new EmployeeJournalFactory(),
				new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope()),
				new DeliveryPointJournalFactory(),
				new SubdivisionJournalFactory())
			{
				RestrictOldOrder = oldOrder,
				RestrictOldOrderStartDate = deliveryDate,
				RestrictOldOrderEndDate = deliveryDate,
				RestrictUndeliveryStatus = undeliveryStatus
			};

			var dlg = new UndeliveredOrdersJournalViewModel(
				undeliveredOrdersFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new GtkTabsOpener(),
				new EmployeeJournalFactory(),
				VodovozGtkServicesConfig.EmployeeService,
				new UndeliveredOrdersJournalOpener(),
				new OrderSelectorFactory(),
				new UndeliveredOrdersRepository(),
				new EmployeeSettings(new ParametersProvider()),
				new SubdivisionParametersProvider(new ParametersProvider())
			);

			tab.TabParent.AddSlaveTab(tab, dlg);
		}
	}
}
