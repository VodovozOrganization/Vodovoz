using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
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

		#region Commands

		public DelegateCommand CancelCommand { get; private set; }
		
		#endregion
		
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

			SendDocViewModel = new SendDocumentByEmailViewModel(new EmailRepository(), EmployeeSingletonRepository.GetInstance(), UoW);
			
			CreateCommands();
		}

		private void CreateCommands()
		{
			CreateCancelCommand();
		}

		private void CreateCancelCommand()
		{
			CancelCommand = new DelegateCommand(
				() =>Close(false, CloseSource.Cancel),
				() => true
			);
		}
		
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
				if(!string.IsNullOrEmpty(SendDocViewModel.EmailString))
					SendDocViewModel.EmailString = string.Empty;
		}
	}
}
