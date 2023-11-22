using Gamma.Utilities;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using Microsoft.Extensions.Logging;
using QS.Dialog;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.ViewModels.Dialogs.Email;
using Vodovoz.TempAdapters;
using Vodovoz.Domain.Orders.Documents;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using EdoDocumentType = Vodovoz.Domain.Orders.Documents.Type;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtViewModel : EntityTabViewModelBase<OrderWithoutShipmentForDebt>, ITdiTabAddedNotifier
	{
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private GenericObservableList<EdoContainer> _edoContainers = new GenericObservableList<EdoContainer>();
		
		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }
		public Action<string> OpenCounterpartyJournal;
		public IEntityUoWBuilder EntityUoWBuilder { get; }

		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		public OrderWithoutShipmentForDebtViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			CommonMessages commonMessages,
			IRDLPreviewOpener rdlPreviewOpener,
			ICounterpartyJournalFactory counterpartyJournalFactory) : base(uowBuilder, uowFactory, commonServices)
		{
			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}

			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			bool canCreateBillsWithoutShipment = 
				CommonServices.PermissionService.ValidateUserPresetPermission("can_create_bills_without_shipment", CurrentUser.Id);
			var currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
			
			if (uowBuilder.IsNewEntity)
			{
				if (canCreateBillsWithoutShipment)
				{
					if (!AskQuestion("Вы действительно хотите создать счет без отгрузки на долг?"))
					{
						AbortOpening();
					}
					else
					{
						Entity.Author = currentEmployee;
					}
				}
				else
				{
					AbortOpening("У Вас нет прав на выставление счетов без отгрузки.");
				}
			}
			
			TabName = "Счет без отгрузки на долг";
			EntityUoWBuilder = uowBuilder;

			var loggerFactory = new LoggerFactory();
			var settingsController = new SettingsController(UnitOfWorkFactory, new Logger<SettingsController>(loggerFactory));
			SendDocViewModel =
				new SendDocumentByEmailViewModel(
					new EmailRepository(),
					new EmailParametersProvider(settingsController),
					currentEmployee,
					commonServices.InteractiveService,
					UoW);
		}

		
		#region Commands

		private DelegateCommand cancelCommand;
		public DelegateCommand CancelCommand => cancelCommand ?? (cancelCommand = new DelegateCommand(
			() => Close(true, CloseSource.Cancel),
			() => true
        ));
		
		private DelegateCommand openBillCommand;
		public DelegateCommand OpenBillCommand => openBillCommand ?? (openBillCommand = new DelegateCommand(
			() =>
			{
				string whatToPrint = "документа \"" + Entity.Type.GetEnumTitle() + "\"";
				
				if(UoWGeneric.HasChanges && _commonMessages.SaveBeforePrint(typeof(OrderWithoutShipmentForDebt), whatToPrint))
				{
					if(Save(false))
					{
						_rdlPreviewOpener.OpenRldDocument(typeof(OrderWithoutShipmentForDebt), Entity);
					}
				}
				
				if(!UoWGeneric.HasChanges && Entity.Id > 0)
				{
					_rdlPreviewOpener.OpenRldDocument(typeof(OrderWithoutShipmentForDebt), Entity);
				}
			},
			() => true
		));

		private void SendBillByEdo(IUnitOfWork uow)
		{
			var edoContainer = _edoContainers.SingleOrDefault(x => x.Type == EdoDocumentType.Bill)
							   ?? new EdoContainer
							   {
								   Type = EdoDocumentType.Bill,
								   Created = DateTime.Now,
								   Container = new byte[64],
								   OrderWithoutShipmentForDebt = Entity,
								   Counterparty = Entity.Counterparty,
								   MainDocumentId = string.Empty,
								   EdoDocFlowStatus = EdoDocFlowStatus.PreparingToSend
							   };

			uow.Save(edoContainer);
			uow.Commit();
		}

		private void UpdateEdoContainers()
		{
			_edoContainers.Clear();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				foreach(var item in _orderRepository.GetEdoContainersByOrderId(uow, Entity.Id))
				{
					_edoContainers.Add(item);
				}
			}
		}

		public ICounterpartyJournalFactory CounterpartyJournalFactory => _counterpartyJournalFactory;

		#endregion

		public void OnTabAdded()
		{
			if(EntityUoWBuilder.IsNewEntity)
			{
				OpenCounterpartyJournal?.Invoke(string.Empty);
			}
		}

		public void OnEntityViewModelEntryChanged(object sender, EventArgs e)
		{
			var email = Entity.GetEmailAddressForBill();
			SendDocViewModel.Update(Entity, email != null ? email.Address : string.Empty);
		}
	}
}
