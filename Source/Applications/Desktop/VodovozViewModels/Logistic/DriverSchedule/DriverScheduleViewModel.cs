using NHibernate;
using NHibernate.Criterion;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Utilities.Enums;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Presentation.ViewModels.Widgets.Profitability;
using Vodovoz.Settings.Logistics;
using VodovozBusiness.Nodes;
using VodovozInfrastructure.StringHandlers;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleViewModel : DialogTabViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private readonly ICarEventSettings _carEventSettings;

		private ObservableList<SubdivisionNode> _subdivisions;
		private IList<CarTypeOfUse> _selectedCarTypeOfUse;
		private IList<CarOwnType> _selectedCarOwnTypes;
		private IList<int> _selectedSubdivisionIds;
		private DateTime _startDate;
		private DateTime _endDate;
		private bool _isIdleState = true;

		public DriverScheduleViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICarEventSettings carEventSettings,
			INavigationManager navigation,
			IStringHandler stringHandler,
			IDatePickerViewModelFactory weekPickerViewModelFactory
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{

			StringHandler = stringHandler ?? throw new ArgumentNullException(nameof(stringHandler));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));

			InitializeWeekPicker(weekPickerViewModelFactory);

			Title = "График водителей";

			var typesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>().ToList();
			typesOfUse.Remove(CarTypeOfUse.Loader);
			typesOfUse.Remove(CarTypeOfUse.Truck);

			var carOwnTypes = EnumHelper.GetValuesList<CarOwnType>();

			SelectedCarTypeOfUse = typesOfUse;
			SelectedCarOwnTypes = carOwnTypes;

			InitializeSubdivisions();

			DriverScheduleRows = GenerateDriverRows();
			LoadAvailableCarEventTypes();
			LoadAvailableDeliverySchedules();

			SaveCommand = new DelegateCommand(() => SaveAndClose());
			CancelCommand = new DelegateCommand(() => Close(AskSaveOnClose, CloseSource.Cancel));
			ExportlCommand = new DelegateCommand(() => ExportCommand());
			InfoCommand = new DelegateCommand(() => ShowInfoMessage());
			ApplyFiltersCommand = new DelegateCommand(() =>
			{
				DriverScheduleRows = GenerateDriverRows();
				OnPropertyChanged(nameof(DriverScheduleRows));
			});
		}

		public DatePickerViewModel WeekPickerViewModel { get; private set; }

		public bool IsIdleState
		{
			get => _isIdleState;
			set => SetField(ref _isIdleState, value);
		}

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

		public bool CanEdit => true;
		public bool AskSaveOnClose => CanEdit;


		public ObservableList<DriverScheduleNode> DriverScheduleRows { get; private set; }
		public List<CarEventType> AvailableCarEventTypes { get; } = new List<CarEventType>();
		public List<DeliverySchedule> AvailableDeliverySchedules { get; } = new List<DeliverySchedule>();

		public IStringHandler StringHandler { get; }

		public ICommand SaveCommand { get; }
		public ICommand CancelCommand { get; }
		public ICommand ExportlCommand { get; }
		public ICommand InfoCommand { get; }
		public ICommand ApplyFiltersCommand { get; }

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

		private void InitializeSubdivisions()
		{
			Subdivision subdivisionAlias = null;
			CarModel carModelAlias = null;

			var subdivisionIds = GetFilteredDriversQuery()
				.Left.JoinAlias(e => e.Subdivision, () => subdivisionAlias)
				.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsIn(SelectedCarTypeOfUse.ToArray())
				.SelectList(list => list
					.SelectGroup(() => subdivisionAlias.Id)
				)
				.List<int>()
				.Distinct()
				.ToList();

			var subdivisions = subdivisionIds.Count > 0
				? UoW.Session.QueryOver<Subdivision>()
					.WhereRestrictionOn(s => s.Id).IsIn(subdivisionIds.ToArray())
					.OrderBy(s => s.Name).Asc
					.List()
					.ToList()
				: new List<Subdivision>();

			Subdivisions = new ObservableList<SubdivisionNode>(
				subdivisions.Select(subdivision => new SubdivisionNode(subdivision) { Selected = true })
			);
		}

		private ObservableList<DriverScheduleNode> GenerateDriverRows()
		{
			Employee employeeAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			Phone phoneAlias = null;
			DriverScheduleNode resultAlias = null;
			CarVersion carVersionAlias = null;
			VodovozBusiness.Domain.Logistic.Drivers.DriverSchedule driverScheduleAlias = null;
			DriverScheduleItem driverScheduleItemAlias = null;

			var selectedSubdivisionIds = Subdivisions
				.Where(s => s.Selected)
				.Select(s => s.SubdivisionId)
				.ToArray();

			var driversQuery = GetFilteredDriversQuery()
				.JoinEntityAlias(
					() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => driverScheduleAlias,
					() => driverScheduleAlias.Driver.Id == employeeAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin
				)
				.WhereRestrictionOn(e => e.Subdivision.Id).IsIn(selectedSubdivisionIds);

			if(SelectedCarTypeOfUse != null && SelectedCarTypeOfUse.Any())
			{
				driversQuery.Where(Restrictions.Disjunction()
					.Add(Restrictions.IsNull(Projections.Property(() => carModelAlias.CarTypeOfUse)))
					.Add(Restrictions.In(Projections.Property(() => carModelAlias.CarTypeOfUse),
						SelectedCarTypeOfUse.ToArray()))
				);
			}

			var phoneSubquery = QueryOver.Of(() => phoneAlias)
				.Where(() => phoneAlias.Employee.Id == ((Employee)null).Id)
				.OrderBy(() => phoneAlias.Id).Asc
				.Select(Projections.Property(() => phoneAlias.Number))
				.Take(1);

			var result = driversQuery
				.SelectList(list => list
					.Select(e => e.Id).WithAlias(() => resultAlias.DriverId)
					.Select(() => carModelAlias.CarTypeOfUse).WithAlias(() => resultAlias.CarTypeOfUse)
					.Select(() => carVersionAlias.CarOwnType).WithAlias(() => resultAlias.CarOwnType)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.RegNumber)
					.Select(e => e.LastName).WithAlias(() => resultAlias.LastName)
					.Select(e => e.Name).WithAlias(() => resultAlias.Name)
					.Select(e => e.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(e => e.DriverOfCarOwnType).WithAlias(() => resultAlias.DriverCarOwnType)
					.Select(e => e.District).WithAlias(() => resultAlias.District)
					.Select(() => driverScheduleAlias.MorningAddressesPotential).WithAlias(() => resultAlias.MorningAddresses)
					.Select(() => driverScheduleAlias.MorningBottlesPotential).WithAlias(() => resultAlias.MorningBottles)
					.Select(() => driverScheduleAlias.EveningAddressesPotential).WithAlias(() => resultAlias.EveningAddresses)
					.Select(() => driverScheduleAlias.EveningBottlesPotential).WithAlias(() => resultAlias.EveningBottles)
					.Select(() => driverScheduleAlias.LastChangeTime).WithAlias(() => resultAlias.LastModifiedDateTime)
					.SelectSubQuery(phoneSubquery).WithAlias(() => resultAlias.DriverPhone)
				)
				.OrderBy(e => e.LastName).Asc
				.TransformUsing(NHibernate.Transform.Transformers.AliasToBean<DriverScheduleNode>())
				.List<DriverScheduleNode>();

			var driverIds = result.Select(r => r.DriverId).ToList();
			if(driverIds.Any())
			{
				var scheduleItems = UoW.Session.QueryOver<DriverScheduleItem>()
					.JoinAlias(i => i.DriverSchedule, () => driverScheduleAlias)
					.JoinAlias(() => driverScheduleAlias.Driver, () => employeeAlias)
					.WhereRestrictionOn(() => employeeAlias.Id).IsIn(driverIds.ToArray())
					.Where(i => i.Date >= StartDate && i.Date <= EndDate)
					.List();

				foreach(var node in result)
				{
					var driverScheduleItems = scheduleItems
						.Where(si => si.DriverSchedule.Driver.Id == node.DriverId)
						.ToList();

					foreach(var item in driverScheduleItems)
					{
						int dayIndex = (int)(item.Date - StartDate).TotalDays;
						if(dayIndex >= 0 && dayIndex < 7)
						{
							node.Days[dayIndex].Date = item.Date;
							node.Days[dayIndex].CarEventType = item.CarEventType;
							node.Days[dayIndex].MorningAddresses = item.MorningAddresses;
							node.Days[dayIndex].MorningBottles = item.MorningBottles;
							node.Days[dayIndex].EveningAddresses = item.EveningAddresses;
							node.Days[dayIndex].EveningBottles = item.EveningBottles;
							node.Days[dayIndex].ParentNode = node;
						}
					}
				}
			}


			foreach(var row in result)
			{
				row.InitializeEmptyCarEventTypes();
			}

			return new ObservableList<DriverScheduleNode>(result);
		}

		private IQueryOver<Employee, Employee> GetFilteredDriversQuery()
		{
			Employee employeeAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;

			return UoW.Session.QueryOver(() => employeeAlias)
				.JoinEntityAlias(
					() => carAlias,
					() => carAlias.Driver.Id == employeeAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin
				)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Where(() => employeeAlias.Status != EmployeeStatus.IsFired)
				.Where(() => employeeAlias.Category == EmployeeCategory.driver)
				//.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsIn(SelectedCarTypeOfUse.ToArray())
				.WhereRestrictionOn(() => employeeAlias.DriverOfCarOwnType).IsIn(SelectedCarOwnTypes.ToArray())
				;
		}

		private void LoadAvailableCarEventTypes()
		{
			var noneEventType = new CarEventType { Id = 0, ShortName = "Нет", Name = "Нет" };

			AvailableCarEventTypes.Add(noneEventType);

			var allowedIds = _carEventSettings.AllowedCarEventTypeIdsForDriverSchedule;

			AvailableCarEventTypes.AddRange(UoW.GetAll<CarEventType>()
				.Where(x => !x.IsArchive && allowedIds.Contains(x.Id))
				.ToList());
		}

		private void LoadAvailableDeliverySchedules()
		{
			AvailableDeliverySchedules.AddRange(UoW.GetAll<DeliverySchedule>()
				.Where(x => !x.IsArchive)
				.ToList());
		}

		private void ExportCommand()
		{
			var i = 228;
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
				"Вы можете перемещаться с помощью стрелочек на клавиатуре между ячейками для ввода данных.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, infoMessage);
		}
	}
}
