using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.Utilities.Enums;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Presentation.ViewModels.Widgets.Profitability;
using Vodovoz.Settings.Logistics;
using Vodovoz.ViewModels.Services.DriverSchedule;
using VodovozBusiness.EntityRepositories.Logistic;
using VodovozBusiness.Nodes;
using VodovozInfrastructure.StringHandlers;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<DriverScheduleViewModel> _logger;
		private readonly ICarEventSettings _carEventSettings;
		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IUserService _userService;
		private readonly IFileDialogService _fileDialogService;
		private readonly IDriverScheduleService _driverScheduleService;
		private readonly ILogisticRepository _logisticRepository;

		private ObservableList<SubdivisionNode> _subdivisions;
		private IList<CarTypeOfUse> _selectedCarTypeOfUse;
		private IList<CarOwnType> _selectedCarOwnTypes;
		private IList<int> _selectedSubdivisionIds;
		private DateTime _startDate;
		private DateTime _endDate;

		public DriverScheduleViewModel(
			ILogger<DriverScheduleViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			ICarEventSettings carEventSettings,
			INavigationManager navigation,
			IStringHandler stringHandler,
			IDatePickerViewModelFactory weekPickerViewModelFactory,
			IUserService userService,
			IFileDialogService fileDialogService,
			IDriverScheduleService driverScheduleService,
			ILogisticRepository logisticRepository
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			StringHandler = stringHandler ?? throw new ArgumentNullException(nameof(stringHandler));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_driverScheduleService = driverScheduleService ?? throw new ArgumentNullException(nameof(driverScheduleService));
			_logisticRepository = logisticRepository ?? throw new ArgumentNullException(nameof(logisticRepository));

			InitializeWeekPicker(weekPickerViewModelFactory);

			Title = "График водителей";

			SetPermissions();
			var typesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>().ToList();
			typesOfUse.Remove(CarTypeOfUse.Loader);
			typesOfUse.Remove(CarTypeOfUse.Truck);

			var carOwnTypes = EnumHelper.GetValuesList<CarOwnType>();

			SelectedCarTypeOfUse = typesOfUse;
			SelectedCarOwnTypes = carOwnTypes;

			InitializeSubdivisions();

			DriverScheduleRows = GenerateRows();
			LoadAvailableCarEventTypes();

			SaveCommand = new DelegateCommand(SaveDriverSchedule, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);

			CancelCommand = new DelegateCommand(() => Close(AskSaveOnClose, CloseSource.Cancel));

			ExportCommand = new DelegateCommand(Export, () => CanSave);
			ExportCommand.CanExecuteChangedWith(this, x => x.CanSave);

			InfoCommand = new DelegateCommand(() => ShowInfoMessage());

			ApplyFiltersCommand = new DelegateCommand(() =>
			{
				DriverScheduleRows = GenerateRows();
				OnPropertyChanged(nameof(DriverScheduleRows));
			});
		}

		public DatePickerViewModel WeekPickerViewModel { get; private set; }

		public IInteractiveService InteractiveService => _interactiveService;

		public IList<CarTypeOfUse> SelectedCarTypeOfUse
		{
			get => _selectedCarTypeOfUse;
			set => SetField(ref _selectedCarTypeOfUse, value);
		}

		public IList<CarOwnType> SelectedCarOwnTypes
		{
			get => _selectedCarOwnTypes;
			set => SetField(ref _selectedCarOwnTypes, value);
		}

		public IList<int> SelectedSubdivisionIds
		{
			get => _selectedSubdivisionIds;
			set => SetField(ref _selectedSubdivisionIds, value);
		}

		public DateTime StartDate
		{
			get => _startDate;
			private set => SetField(ref _startDate, value);
		}

		public DateTime EndDate
		{
			get => _endDate;
			private set => SetField(ref _endDate, value);
		}

		public ObservableList<SubdivisionNode> Subdivisions
		{
			get => _subdivisions;
			private set => SetField(ref _subdivisions, value);
		}

		public bool CanEdit;
		public bool CanEditAfter13;
		public bool CanSave => CanEdit;
		public bool AskSaveOnClose => CanEdit;

		public ObservableList<DriverScheduleRow> DriverScheduleRows { get; private set; }
		public List<CarEventType> AvailableCarEventTypes { get; } = new List<CarEventType>();

		public IStringHandler StringHandler { get; }

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand ExportCommand { get; }
		public DelegateCommand InfoCommand { get; }
		public DelegateCommand ApplyFiltersCommand { get; }

		private void InitializeWeekPicker(IDatePickerViewModelFactory weekPickerViewModelFactory)
		{
			WeekPickerViewModel = weekPickerViewModelFactory.CreateNewDatePickerViewModel(
				DateTime.Now,
				ChangeDateType.Week);

			WeekPickerViewModel.DateChanged += (s, e) => UpdateDateRange();
			UpdateDateRange();
		}

		private void UpdateDateRange()
		{
			StartDate = WeekPickerViewModel.SelectedDate;
			EndDate = WeekPickerViewModel.SelectedDate.AddDays(6);
		}

		public string GetShortDayString(DateTime date) => _driverScheduleService.GetShortDayString(date);

		private void SetPermissions()
		{
			CanEdit = _currentPermissionService.ValidatePresetPermission(Core.Domain.Permissions.LogisticPermissions.CanWorkWithDriverSchedule);
			CanEditAfter13 = _currentPermissionService.ValidatePresetPermission(Core.Domain.Permissions.LogisticPermissions.CanEditEventsAndCapacitiesAfter13);
		}

		private void InitializeSubdivisions()
		{
			var carTypeOfUseArray = SelectedCarTypeOfUse.ToArray();

			var subdivisions = _logisticRepository.GetSubdivisionsForDriverSchedule(UoW, carTypeOfUseArray, StartDate, EndDate);

			Subdivisions = new ObservableList<SubdivisionNode>(
				subdivisions.Select(subdivision => new SubdivisionNode(subdivision) { Selected = true })
			);
		}

		private ObservableList<DriverScheduleRow> GenerateRows()
		{
			var selectedSubdivisionIds = Subdivisions
				.Where(s => s.Selected)
				.Select(s => s.SubdivisionId)
				.ToArray();

			try
			{
				var rows = _driverScheduleService.LoadScheduleData(
					UoW,
					StartDate,
					EndDate,
					selectedSubdivisionIds,
					SelectedCarOwnTypes?.ToArray(),
					SelectedCarTypeOfUse?.ToArray(),
					CanEditAfter13,
					AvailableCarEventTypes);

				return new ObservableList<DriverScheduleRow>(rows);
			}
			catch(Exception ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error,
					$"Ошибка при загрузке данных: {ex.Message}");

				_logger.LogError(ex, "Ошибка при загрузке данных в графике водителей");

				return new ObservableList<DriverScheduleRow>();
			}
		}

		private void LoadAvailableCarEventTypes()
		{
			var noneEventType = new CarEventType { Id = -1, ShortName = "Нет", Name = "Нет" };

			AvailableCarEventTypes.Add(noneEventType);

			var allowedIds = _carEventSettings.AllowedCarEventTypeIdsForDriverSchedule;

			AvailableCarEventTypes.AddRange(UoW.GetAll<CarEventType>()
				.Where(x => !x.IsArchive && allowedIds.Contains(x.Id))
				.ToList());
		}

		private void SaveDriverSchedule()
		{
			try
			{
				var changedRows = DriverScheduleRows
					.Where(r => !(r is DriverScheduleTotalRow) && r.HasChanges)
					.ToList();

				if(!changedRows.Any())
				{
					_interactiveService.ShowMessage(ImportanceLevel.Info, "Нет изменений для сохранения");
					return;
				}

				_driverScheduleService.SaveScheduleChanges(
					UoW,
					changedRows,
					StartDate,
					EndDate,
					_userService.CurrentUserId);

				foreach(var row in changedRows)
				{
					row.HasChanges = false;
				}

				_interactiveService.ShowMessage(ImportanceLevel.Info, "Сохранено успешно");
				UoW.Commit();
			}
			catch(Exception ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error, $"Ошибка при сохранении:\n{ex.Message}");

				_logger.LogError(ex, "Ошибка при сохранении в графике водителей");
			}
		}

		private void Export()
		{
			if(DriverScheduleRows == null || DriverScheduleRows.Count == 0)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Нет данных для экспорта");
				return;
			}

			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = $"График_водителей_{StartDate:dd.MM.yyyy}_{EndDate:dd.MM.yyyy}.xlsx"
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				try
				{
					var excelData = _driverScheduleService.ExportToExcel(
						DriverScheduleRows,
						StartDate,
						EndDate);

					System.IO.File.WriteAllBytes(result.Path, excelData);

					_interactiveService.ShowMessage(
						ImportanceLevel.Info,
						"Файл успешно сохранен");
				}
				catch(Exception ex)
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Error,
						$"Ошибка при экспорте:\n{ex.Message}\n{ex.InnerException?.Message}");

					_logger.LogError(ex, "Ошибка при экспорте данных в графике водителей");
				}
			}
		}

		private void ShowInfoMessage()
		{
			var infoMessage =
				"Пояснения к столбцам:\n" +
				"\"П\" - принадлежность\n" +
				"\"Т\" - тип ТС\n" +
				"Принадлежность - принадлежность из карточки сотрудника\n" +
				"Дата посл. изм. - дата последнего изменения потенциала водителя\n" +
				"\n" +
				"Условные обозначения отчёта:\n" +
				"\"К\" - ТС компании\n" +
				"\"В\" - ТС водителя\n" +
				"\"Р\" - ТС в раскате\n" +
				"\"Л\" - Легковой (Ларгус)\n" +
				"\"Г\" - Грузовой (Газель)\n" +
				"\"Т\" - Фургон (Transit Mini)\n" +
				"\n" +
				"Логика работы со столбцами:\n" +
				"\n" +
				"1) После проставления чисел в столбцы Утро и Вечер (потенциала водителя) данные автоматически подтягиваются в столбцы Утро и Вечер, которые привязаны к дням недели.\n" +
				"\n" +
				"2) Если в столбце с днём недели проставлено любое событие, кроме \"Нет\" - в ячейках Утро и Вечер, привязанным к этому дню, автоматически выставляются значения, равные 0.\n" +
				"\n" +
				"3) Разрешено вручную редактировать данные ячейки только если статус выставлен \"Нет\". После редактирования ячеек, данные в них обновятся только после смены статуса, либо после изменения столбцов Утро и Вечер (которые идут до дней недели).\n" +
				"\n" +
				"4) Столбцы 8 и 9 изменяют остальные столбцы только на текущий день и далее, данные ПРОШЛЫХ дней не перезаписываются.\n" +
				"\n" +
				"График \"Отчёт по простою\" учитывает информацию о событиях из графика водителей.\n" +
				"\n" +
				"Если событие создано через Журнал событий (Например, ремонт) - оно добавляется в график водителей в проставленный период в сокращенном виде и его нельзя изменить.\n" +
				"\n" +
				"Вы можете перемещаться с помощью стрелочек на клавиатуре между ячейками для ввода данных." +
				"\n" +
				"Вертикальную прокрутку столбцов с днями недели можно выполнять с зажатой клавишей Shift.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, infoMessage);
		}
	}
}
