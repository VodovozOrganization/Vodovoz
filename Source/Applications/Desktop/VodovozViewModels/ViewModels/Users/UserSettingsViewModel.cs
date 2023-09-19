﻿using System;
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
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.Users
{
	public class UserSettingsViewModel : EntityTabViewModelBase<UserSettings>, ITDICloseControlTab
	{
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly INomenclaturePricesRepository _nomenclatureFixedPriceRepository;
		private DelegateCommand _updateFixedPricesCommand;
		private bool _sortingSettingsUpdated;
		private bool _isFixedPricesUpdating;
		private string _progressMessage;
		private double _progressFraction;
		private decimal _incrementFixedPrices = 20;
		private const double _progressStep = 0.25;

		public UserSettingsViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			ISubdivisionRepository subdivisionRepository,
			INomenclaturePricesRepository nomenclatureFixedPriceRepository)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
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

		public string ProgressMessage
		{
			get => _progressMessage;
			private set => SetField(ref _progressMessage, value);
		}

		public double ProgressFraction
		{
			get => _progressFraction;
			private set => SetField(ref _progressFraction, value);
		}
		
		public IInteractiveService InteractiveService { get; }
		public IEntityAutocompleteSelectorFactory SubdivisionSelectorDefaultFactory { get; }
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public bool IsUserFromOkk => _subdivisionParametersProvider.GetOkkId()
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
							UpdateProgress("Получаем фиксу, которую нужно обновить...");
							using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
							{
								var fixedPrices = _nomenclatureFixedPriceRepository.GetFixedPricesFor19LWater(uow);
								UpdateProgress($"Получили данные, которые нужно обновить. Всего {fixedPrices.Count} объектов");
								
								for(int i = 0; i < fixedPrices.Count; i++)
								{
									fixedPrices[i].Price += IncrementFixedPrices;
									uow.Save(fixedPrices[i]);
								}
								
								UpdateProgress($"Получили данные, которые нужно обновить. Всего {fixedPrices.Count} объектов. " +
									"Обновляем фиксу с записью в историю изменений...");
								uow.Commit();
								UpdateProgress("Готово");
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
				ShowWarningMessage("Дождитесь завершения задачи и повторите");
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
		
		private void UpdateProgress(string message)
		{
			ProgressMessage = message;
			ProgressFraction += _progressStep;
		}

		private void ConfigureCashSorting()
		{
			var availableSubdivisions = _subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, CurrentUser).ToList();

			_sortingSettingsUpdated = Entity.UpdateCashSortingSettings(availableSubdivisions);
		}
	}
}
