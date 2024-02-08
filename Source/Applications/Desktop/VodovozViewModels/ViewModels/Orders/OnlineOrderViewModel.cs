using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Orders
{
	public class OnlineOrderViewModel : EntityTabViewModelBase<OnlineOrder>
	{
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly Employee _currentEmployee;

		public OnlineOrderViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IEmployeeService employeeService,
			IGtkTabsOpener gtkTabsOpener)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_currentEmployee =
				(employeeService ?? throw new ArgumentNullException(nameof(employeeService)))
				.GetEmployeeForUser(UoW, CurrentUser.Id);

			if(_currentEmployee is null)
			{
				AbortOpening("Ваш пользователь не привязан к сотруднику. Дальнейшая работа не возможна");
			}
			
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			CreateCommands();
		}
		
		public DelegateCommand GetToWorkCommand { get; private set; }

		private void CreateCommands()
		{
			CreateGetToWorkCommand();
		}

		private void CreateGetToWorkCommand()
		{
			GetToWorkCommand = new DelegateCommand(
				() =>
				{
					Entity.EmployeeWorkWith = _currentEmployee;
					
					if(!Save(false))
					{
						return;
					}

					CreateOrderFromOnlineOrder();
					//_gtkTabsOpener.OpenOrderDlg(this);
				});
		}

		private void CreateOrderFromOnlineOrder()
		{
			throw new NotImplementedException();
		}
	}
}
