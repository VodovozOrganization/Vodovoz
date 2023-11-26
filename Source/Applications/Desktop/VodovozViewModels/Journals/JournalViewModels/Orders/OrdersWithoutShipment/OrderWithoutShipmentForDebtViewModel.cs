using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.Specifications.Orders.EdoContainers;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Email;
using EdoDocumentType = Vodovoz.Domain.Orders.Documents.Type;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtViewModel : EntityTabViewModelBase<OrderWithoutShipmentForDebt>, ITdiTabAddedNotifier
	{
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private IGenericRepository<EdoContainer> _edoContainerRepository;

		public Action<string> OpenCounterpartyJournal;

		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		public OrderWithoutShipmentForDebtViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			CommonMessages commonMessages,
			IRDLPreviewOpener rdlPreviewOpener,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IGenericRepository<EdoContainer> edoContainerRepository) : base(uowBuilder, uowFactory, commonServices)
		{
			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}

			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_edoContainerRepository = edoContainerRepository ?? throw new ArgumentNullException(nameof(edoContainerRepository));

			bool canCreateBillsWithoutShipment =
				CommonServices.PermissionService.ValidateUserPresetPermission("can_create_bills_without_shipment", CurrentUser.Id);
			var currentEmployee = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);

			if(uowBuilder.IsNewEntity)
			{
				if(canCreateBillsWithoutShipment)
				{
					if(!AskQuestion("Вы действительно хотите создать счет без отгрузки на долг?"))
					{
						AbortOpening();
						return;
					}
					else
					{
						Entity.Author = currentEmployee;
					}
				}
				else
				{
					AbortOpening("У Вас нет прав на выставление счетов без отгрузки.");
					return;
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

			if(Entity.Id != 0)
			{
				UpdateEdoContainers();

				if(!Entity.IsBillWithoutShipmentSent && Entity.Client.NeedSendBillByEdo)
				{
					EdoContainers.Add(new EdoContainer
					{
						Type = EdoDocumentType.BillWithoutShipmentForDebt,
						Created = DateTime.Now,
						Container = new byte[64],
						OrderWithoutShipmentForDebt = Entity,
						Counterparty = Entity.Counterparty,
						MainDocumentId = string.Empty,
						EdoDocFlowStatus = EdoDocFlowStatus.PreparingToSend
					});
				}
			}

			CancelCommand = new DelegateCommand(
				() => Close(true, CloseSource.Cancel),
				() => true);

			OpenBillCommand = new DelegateCommand(
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
				() => true);
		}

		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }
		public IEntityUoWBuilder EntityUoWBuilder { get; }
		public ICounterpartyJournalFactory CounterpartyJournalFactory => _counterpartyJournalFactory;

		#region Commands

		public DelegateCommand CancelCommand { get; }
		
		public DelegateCommand OpenBillCommand { get; }

		private void SendBillByEdo(IUnitOfWork uow)
		{
			var edoContainer = EdoContainers
				.SingleOrDefault(x => x.Type == EdoDocumentType.BillWithoutShipmentForDebt)
					?? new EdoContainer
					{
						Type = EdoDocumentType.BillWithoutShipmentForDebt,
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
			EdoContainers.Clear();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				foreach(var item in _edoContainerRepository.Get(uow, EdoContainerSpecification.CreateForOrderWithoutShipmentForDebtId(Entity.Id)))
				{
					EdoContainers.Add(item);
				}
			}
		}

		#endregion

		public GenericObservableList<EdoContainer> EdoContainers { get; } =
			new GenericObservableList<EdoContainer>();

		public List<EdoContainer> GetOutgoingUpdDocuments()
		{
			var orderUpdDocuments = new List<EdoContainer>();

			if(Entity.Id == 0)
			{
				return orderUpdDocuments;
			}

			orderUpdDocuments = EdoContainers
				.Where(c =>
					!c.IsIncoming
					&& c.Type == EdoDocumentType.Upd)
				.ToList();

			return orderUpdDocuments;
		}

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
