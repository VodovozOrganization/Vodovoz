using QS.Tdi;
using QS.ViewModels.Extension;
using QS.ViewModels;
using QS.DomainModel.UoW;
using QS.Project.Services.FileDialog;
using QS.Services;
using Vodovoz.EntityRepositories;
using QS.Navigation;
using QS.Project.Domain;
using Vodovoz.Services;
using Vodovoz.Domain.Employees;
using QS.Project.Services;
using System;

namespace Vodovoz.ViewModels.Dialogs.Counterparty
{
	public class CloseSupplyToCounterpartyViewModel : EntityTabViewModelBase<Domain.Client.Counterparty>
	{
		private readonly ICommonServices _commonServices;
		private readonly IEmployeeService _employeeService;
		private Employee _currentEmployee;
		private readonly int _currentUserId = ServicesConfig.UserService.CurrentUserId;

		public CloseSupplyToCounterpartyViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeService employeeService) : base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			if(uowBuilder is null)
			{
				throw new ArgumentNullException(nameof(uowBuilder));
			}

			if(uowFactory is null)
			{
				throw new ArgumentNullException(nameof(uowFactory));
			}

			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
		}

		public Employee CurrentEmployee =>
			_currentEmployee ?? (_currentEmployee = _employeeService.GetEmployeeForUser(UoW, _currentUserId));

		public bool CanCloseDeliveries => 
			_commonServices.CurrentPermissionService.ValidatePresetPermission("can_close_deliveries_for_counterparty");
	}
}
