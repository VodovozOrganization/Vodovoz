﻿using Gamma.Utilities;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using QS.Dialog;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModels.Dialogs.Email;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtViewModel : EntityTabViewModelBase<OrderWithoutShipmentForDebt>, ITdiTabAddedNotifier
	{
		private readonly CommonMessages _commonMessages;
		private readonly IRDLPreviewOpener _rdlPreviewOpener;
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
			IRDLPreviewOpener rdlPreviewOpener) : base(uowBuilder, uowFactory, commonServices)
		{
			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}

			_commonMessages = commonMessages ?? throw new ArgumentNullException(nameof(commonMessages));
			_rdlPreviewOpener = rdlPreviewOpener ?? throw new ArgumentNullException(nameof(rdlPreviewOpener));

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

			SendDocViewModel =
				new SendDocumentByEmailViewModel(
					new EmailRepository(),
					new EmailParametersProvider(new ParametersProvider()),
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
