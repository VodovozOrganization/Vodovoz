using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Dialogs.Email;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtViewModel : EntityTabViewModelBase<OrderWithoutShipmentForDebt>, ITdiTabAddedNotifier
	{
		public SendDocumentByEmailViewModel SendDocViewModel { get; set; }
		public Action<string> OpenCounterpatyJournal;
		public IEntityUoWBuilder EntityUoWBuilder { get; }

		public bool IsDocumentSent => Entity.IsOrderWithoutShipmentSent;

		public OrderWithoutShipmentForDebtViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices) : base(uowBuilder, uowFactory, commonServices)
		{
			TabName = "Счет без отгрузки на долг";
			EntityUoWBuilder = uowBuilder;

			if (uowBuilder.IsNewEntity)
			{
				Entity.Author = EmployeeSingletonRepository.GetInstance().GetEmployeeForCurrentUser(UoW);
			}
		}

		public void OnTabAdded()
		{
			if(EntityUoWBuilder.IsNewEntity)
				OpenCounterpatyJournal?.Invoke(string.Empty);
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
