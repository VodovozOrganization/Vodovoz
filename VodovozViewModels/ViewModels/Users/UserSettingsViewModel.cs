using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.Users
{
	public class UserSettingsViewModel : EntityTabViewModelBase<UserSettings>
	{
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionService _subdivisionService;
		
		public ISubdivisionJournalFactory SubdivisionJournalFactory { get; }
		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }

		public UserSettingsViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			IEmployeeService employeeService, ISubdivisionService subdivisionService,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService)); ;
			_subdivisionService = subdivisionService ?? throw new ArgumentNullException(nameof(subdivisionService));
			SubdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			CounterpartyAutocompleteSelectorFactory =
				counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
		}

		public bool IsUserFromOkk => _subdivisionService.GetOkkId() == _employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId)?.Subdivision?.Id;

		public bool IsUserFromRetail => CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");
	}
}
