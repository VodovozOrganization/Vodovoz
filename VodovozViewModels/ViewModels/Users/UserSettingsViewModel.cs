using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using FluentNHibernate.Utils;
using MoreLinq;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.Users
{
	public class UserSettingsViewModel : EntityTabViewModelBase<UserSettings>
	{
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionService _subdivisionService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private bool _sortingSettingsUpdated;

		public UserSettingsViewModel(IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			ISubdivisionService subdivisionService,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			ISubdivisionRepository subdivisionRepository)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_subdivisionService = subdivisionService ?? throw new ArgumentNullException(nameof(subdivisionService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			SubdivisionSelectorDefaultFactory =
				(subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory)))
				.CreateDefaultSubdivisionAutocompleteSelectorFactory();
			CounterpartySelectorFactory =
				(counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory();

			if(UserIsCashier)
			{
				ConfigureCashSorting();
			}
		}

		#region Свойства

		public IEntityAutocompleteSelectorFactory SubdivisionSelectorDefaultFactory { get; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public bool IsUserFromOkk => _subdivisionService.GetOkkId()
		                             == _employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId)?.Subdivision?.Id;

		public bool IsUserFromRetail => CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");
		public bool UserIsCashier => CommonServices.CurrentPermissionService.ValidatePresetPermission("role_cashier");

		public IList<CashSubdivisionSortingSettings> SubdivisionSortingSettings => Entity.ObservableCashSubdivisionSortingSettings;

		#endregion

		public override void Close(bool askSave, CloseSource source)
		{
			if(_sortingSettingsUpdated)
			{
				if(CommonServices.InteractiveService.Question(
					"Ваши настройки сортировки касс были автоматически обновлены. Выйти без сохранения?"))
				{
					base.Close(false, source);
				}
				else
				{
					return;
				}
			}
			base.Close(askSave, source);
		}

		public void UpdateIndices() => Entity.UpdateCashSortingIndices();

		private void ConfigureCashSorting()
		{
			var availableSubdivisions = _subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, CurrentUser).ToList();

			_sortingSettingsUpdated = Entity.UpdateCashSortingSettings(availableSubdivisions);
		}
	}
}
