using Autofac;
using EdoService.Library;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;
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
		private IGenericRepository<EdoContainer> _edoContainerRepository;
		private IGenericRepository<OrderEdoTrueMarkDocumentsActions> _orderEdoTrueMarkDocumentsActionsRepository;
		private readonly IEdoService _edoService;
		public Action<string> OpenCounterpartyJournal;
		private bool _userHavePermissionToResendEdoDocuments;
		
		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		public OrderWithoutShipmentForDebtViewModel(
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			CommonMessages commonMessages,
			IRDLPreviewOpener rdlPreviewOpener,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IGenericRepository<EdoContainer> edoContainerRepository,
			IGenericRepository<OrderEdoTrueMarkDocumentsActions> orderEdoTrueMarkDocumentsActionsRepository,
			IEdoService edoService) : base(uowBuilder, uowFactory, commonServices)
		{
			if(lifetimeScope == null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}

			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));
			_edoContainerRepository = edoContainerRepository ?? throw new ArgumentNullException(nameof(edoContainerRepository));
			_orderEdoTrueMarkDocumentsActionsRepository = orderEdoTrueMarkDocumentsActionsRepository ?? throw new ArgumentNullException(nameof(orderEdoTrueMarkDocumentsActionsRepository));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));
			CounterpartyAutocompleteSelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope);
			
			bool canCreateBillsWithoutShipment = 
				CommonServices.PermissionService.ValidateUserPresetPermission("can_create_bills_without_shipment", CurrentUser.Id);
			_userHavePermissionToResendEdoDocuments = CommonServices.PermissionService.ValidateUserPresetPermission(Vodovoz.Permissions.EdoContainer.OrderWithoutShipmentForDebt.CanResendEdoBill, CurrentUser.Id);

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

			UpdateEdoContainers();

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

			Entity.PropertyChanged += OnEntityPropertyChanged;
		}
		
		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }
		
		public bool CanSendBillByEdo => Entity.Client?.NeedSendBillByEdo ?? false && !EdoContainers.Any();

		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }
		public IEntityUoWBuilder EntityUoWBuilder { get; }
		
		#region Commands

		public DelegateCommand CancelCommand { get; }
		
		public DelegateCommand OpenBillCommand { get; }

		private void OnEntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Client))
			{
				OnPropertyChanged(nameof(CanSendBillByEdo));
				OnPropertyChanged(nameof(CanResendEdoBill));
			}
		}

		public void OnButtonSendDocumentAgainClicked(object sender, EventArgs e)
		{
			var edoValidateResult = _edoService.ValidateEdoContainers(EdoContainers);

			var errorMessages = edoValidateResult.Errors.Select(x => x.Message).ToArray();

			if(edoValidateResult.IsFailure)
			{
				if(edoValidateResult.Errors.Any(error => error.Code == Errors.Edo.Edo.AlreadySuccefullSended)
					&& !CommonServices.InteractiveService.Question(
						"Вы уверены, что хотите отправить дубль?\n" +
						string.Join("\n", errorMessages),
						"Требуется подтверждение!"))
				{
					return;
				}
			}

			if(UoW.IsNew)
			{
				if(CommonServices.InteractiveService.Question("Перед отправкой необходимо сохранить счёт, продолжить?"))
				{
					UoW.Save();
				}
				else
				{
					return;
				}
			}

			_edoService.SetNeedToResendEdoDocumentForOrder(Entity, EdoDocumentType.BillWSForDebt);

			UpdateEdoContainers();

			OnPropertyChanged(nameof(CanSendBillByEdo));
			OnPropertyChanged(nameof(CanResendEdoBill));

			CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Отправлено");
		}

		public void UpdateEdoContainers()
		{
			EdoContainers.Clear();

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				foreach(var item in _edoContainerRepository.Get(uow, EdoContainerSpecification.CreateForOrderWithoutShipmentForDebtId(Entity.Id)))
				{
					EdoContainers.Add(item);
				}

				var action = _orderEdoTrueMarkDocumentsActionsRepository.Get(uow, x => x.OrderWithoutShipmentForDebt.Id == Entity.Id && x.IsNeedToResendEdoBill == true)
					.FirstOrDefault();

				if(action != null)
				{
					var tempContainer = new EdoContainer { Type = EdoDocumentType.BillWSForAdvancePayment, EdoDocFlowStatus = EdoDocFlowStatus.PreparingToSend};
					EdoContainers.Add(tempContainer);
				}
			}
		}

		#endregion

		public GenericObservableList<EdoContainer> EdoContainers { get; } = new GenericObservableList<EdoContainer>();

		public bool CanResendEdoBill => _userHavePermissionToResendEdoDocuments && EdoContainers.Any();

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

		public override bool Save(bool close)
		{
			OnPropertyChanged(nameof(CanSendBillByEdo));

			return base.Save(close);
		}
	}
}
