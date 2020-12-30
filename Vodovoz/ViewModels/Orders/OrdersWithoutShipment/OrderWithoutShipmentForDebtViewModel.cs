using System;
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
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Dialogs.Email;
using Vodovoz.EntityRepositories;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtViewModel : EntityTabViewModelBase<OrderWithoutShipmentForDebt>, ITdiTabAddedNotifier
	{
		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }
		public Action<string> OpenCounterpartyJournal;
		public IEntityUoWBuilder EntityUoWBuilder { get; }

		public bool IsDocumentSent => Entity.IsBillWithoutShipmentSent;

		public OrderWithoutShipmentForDebtViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices) : base(uowBuilder, uowFactory, commonServices)
		{
			bool canCreateBillsWithoutShipment = CommonServices.PermissionService.ValidateUserPresetPermission("can_create_bills_without_shipment", CurrentUser.Id);
			
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
						Entity.Author = EmployeeSingletonRepository.GetInstance().GetEmployeeForCurrentUser(UoW);
					}
				}
				else
				{
					AbortOpening("У Вас нет прав на выставление счетов без отгрузки.");
				}
			}
			
			TabName = "Счет без отгрузки на долг";
			EntityUoWBuilder = uowBuilder;

			SendDocViewModel = new SendDocumentByEmailViewModel(new EmailRepository(), EmployeeSingletonRepository.GetInstance(), commonServices.InteractiveService, UoW);
		}

		
		#region Commands

		private DelegateCommand cancelCommand;
		public DelegateCommand CancelCommand => cancelCommand ?? (cancelCommand = new DelegateCommand(
			() => Close(false, CloseSource.Cancel),
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
