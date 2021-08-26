using System;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Tdi;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Journals.JournalActionsViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
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
				new CounterpartyJournalFactory(),
				new DeliveryPointJournalFactory(),
				new SubdivisionJournalFactory())
			{
				RestrictOldOrder = oldOrder,
				RestrictOldOrderStartDate = deliveryDate,
				RestrictOldOrderEndDate = deliveryDate,
				RestrictUndeliveryStatus = undeliveryStatus
			};

			var gtkDlgOpener = new GtkTabsOpener();

			var dlg = new UndeliveredOrdersJournalViewModel(
				new UndeliveredOrdersJournalActionsViewModel(undeliveredOrdersFilter, ServicesConfig.InteractiveService, gtkDlgOpener),
				undeliveredOrdersFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				gtkDlgOpener,
				new EmployeeJournalFactory(),
				VodovozGtkServicesConfig.EmployeeService,
				new UndeliveredOrdersJournalOpener(),
				new OrderSelectorFactory(),
				new UndeliveredOrdersRepository());

			tab.TabParent.AddSlaveTab(tab, dlg);
		}
	}
}
