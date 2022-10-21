using Gamma.Utilities;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QSOrmProject;
using QSReport;
using System;
using Vodovoz.Dialogs.Email;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtViewModel : EntityTabViewModelBase<OrderWithoutShipmentForDebt>, ITdiTabAddedNotifier
	{
		private readonly IParametersProvider _parametersProvider;
		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }
		public Action<string> OpenCounterpartyJournal;
		public IEntityUoWBuilder EntityUoWBuilder { get; }

		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		public OrderWithoutShipmentForDebtViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			IParametersProvider parametersProvider) : base(uowBuilder, uowFactory, commonServices)
		{
			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}

			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));

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

			SendDocViewModel = new SendDocumentByEmailViewModel(new EmailRepository(), new EmailParametersProvider(new ParametersProvider()), currentEmployee, commonServices.InteractiveService, UoW);
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
				
				if (UoWGeneric.HasChanges &&
				    CommonDialogs.SaveBeforePrint(typeof(OrderWithoutShipmentForDebt), whatToPrint))
				{
					if (Save(false))
						TabParent.AddTab(DocumentPrinter.GetPreviewTab(Entity), this, false);
				}
				
				if(!UoWGeneric.HasChanges && Entity.Id > 0)
					TabParent.AddTab(DocumentPrinter.GetPreviewTab(Entity), this, false);
			},
			() => true
		));
		
		#endregion

		public void OnTabAdded()
		{
			if(EntityUoWBuilder.IsNewEntity)
				OpenCounterpartyJournal?.Invoke(string.Empty);
		}

		public void OnEntityViewModelEntryChanged(object sender, EventArgs e)
		{
			var email = Entity.GetEmailAddressForBill();

			if(email != null)
				SendDocViewModel.Update(Entity, email.Address);
			else
				SendDocViewModel.Update(Entity, string.Empty);
		}
	}
}
