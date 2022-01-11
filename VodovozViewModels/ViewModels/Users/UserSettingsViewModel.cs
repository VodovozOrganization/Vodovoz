using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.Users
{
	public class UserSettingsViewModel : EntityTabViewModelBase<UserSettings>, ITDICloseControlTab
	{
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionService _subdivisionService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly INomenclatureFixedPriceRepository _nomenclatureFixedPriceRepository;
		private DelegateCommand _updateFixedPricesCommand;
		private bool _sortingSettingsUpdated;
		private bool _isFixedPricesUpdating;
		private decimal _incrementFixedPrices = 20;
		private const double _progressStep = 0.25;

		public UserSettingsViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			ISubdivisionService subdivisionService,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			ISubdivisionRepository subdivisionRepository,
			INomenclatureFixedPriceRepository nomenclatureFixedPriceRepository)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_subdivisionService = subdivisionService ?? throw new ArgumentNullException(nameof(subdivisionService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_nomenclatureFixedPriceRepository =
				nomenclatureFixedPriceRepository ?? throw new ArgumentNullException(nameof(nomenclatureFixedPriceRepository));
			InteractiveService = commonServices.InteractiveService;
			SubdivisionSelectorDefaultFactory =
				(subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory)))
				.CreateDefaultSubdivisionAutocompleteSelectorFactory();
			CounterpartySelectorFactory =
				(counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory();
			
			SetPermissions();

			if(UserIsCashier)
			{
				ConfigureCashSorting();
			}
		}

		#region Свойства

		public decimal IncrementFixedPrices
		{
			get => _incrementFixedPrices;
			set => SetField(ref _incrementFixedPrices, value);
		}

		public bool IsFixedPricesUpdating
		{
			get => _isFixedPricesUpdating;
			private set => SetField(ref _isFixedPricesUpdating, value);
		}
		
		public IInteractiveService InteractiveService { get; }
		public IEntityAutocompleteSelectorFactory SubdivisionSelectorDefaultFactory { get; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public bool IsUserFromOkk => _subdivisionService.GetOkkId()
		                             == _employeeService.GetEmployeeForUser(UoW, CommonServices.UserService.CurrentUserId)?.Subdivision?.Id;

		public bool IsUserFromRetail { get; private set; }
		public bool UserIsCashier { get; private set; } 
		public bool CanUpdateFixedPrices { get; private set; }

		public IList<CashSubdivisionSortingSettings> SubdivisionSortingSettings => Entity.ObservableCashSubdivisionSortingSettings;

		public DelegateCommand UpdateFixedPricesCommand => _updateFixedPricesCommand ?? (_updateFixedPricesCommand = new DelegateCommand(
					() =>
					{
						try
						{
							IsFixedPricesUpdating = true;
							UpdateProgressAction?.Invoke("Получаем фиксу, которую нужно обновить...", _progressStep);
							using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
							{
								var fixedPrices = _nomenclatureFixedPriceRepository.GetFixedPricesFor19LWater(uow);
								
								UpdateProgressAction?.Invoke(CreateMessage(fixedPrices.Count), _progressStep);

								for(int i = 0; i < fixedPrices.Count; i++)
								{
									fixedPrices[i].Price += IncrementFixedPrices;
									uow.Save(fixedPrices[i]);
								}

								UpdateProgressAction?.Invoke(
									$"{CreateMessage(fixedPrices.Count)} Обновляем фиксу с записью в историю изменений...", _progressStep);
								uow.Commit();
								UpdateProgressAction?.Invoke("Готово", 1);
							}
						}
						finally
						{
							IsFixedPricesUpdating = false;
						}
					},
					() => !IsFixedPricesUpdating
			)
		);

		#endregion

		public event Action<string, double> UpdateProgressAction;
		public event Action ShowBusyMessageAction;

		public override void Close(bool askSave, CloseSource source)
		{
			if(_sortingSettingsUpdated && source == CloseSource.Cancel)
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
		
		public bool CanClose()
		{
			if(IsFixedPricesUpdating)
			{
				ShowBusyMessageAction?.Invoke();
			}

			return !IsFixedPricesUpdating;
		}
		
		public void UpdateIndices() => Entity.UpdateCashSortingIndices();
		
		private void SetPermissions()
		{
			CanUpdateFixedPrices = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_update_fixed_prices_for_19l_water");
			IsUserFromRetail = CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");
			UserIsCashier = CommonServices.CurrentPermissionService.ValidatePresetPermission("role_cashier");
		}

		private void ConfigureCashSorting()
		{
			var availableSubdivisions = _subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, CurrentUser).ToList();

			_sortingSettingsUpdated = Entity.UpdateCashSortingSettings(availableSubdivisions);
		}
		
		private string CreateMessage(int pricesCount) => $"Получили данные, которые нужно обновить. Всего {pricesCount} объектов.";
	}
}
