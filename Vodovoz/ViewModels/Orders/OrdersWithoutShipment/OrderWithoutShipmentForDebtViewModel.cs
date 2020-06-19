using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.ViewModels.Orders.OrdersWithoutShipment
{
	public class OrderWithoutShipmentForDebtViewModel : EntityTabViewModelBase<OrderWithoutShipmentForDebt>, ITdiTabAddedNotifier
	{
		public DelegateCommand SendEmailCommand { get; private set; }
		public Action<string> OpenCounterpatyJournal;
		public IEntityUoWBuilder EntityUoWBuilder { get; }

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

			CreateCommands();
		}

		private void CreateCommands()
		{
			CreateSendEmailCommand();
		}

		private void CreateSendEmailCommand()
		{
			SendEmailCommand = new DelegateCommand(
				() => Close(false, QS.Navigation.CloseSource.Cancel),
				() => true
			);
		}

		public void OnTabAdded()
		{
			if(EntityUoWBuilder.IsNewEntity)
				OpenCounterpatyJournal?.Invoke(string.Empty);
		}
	}
}
