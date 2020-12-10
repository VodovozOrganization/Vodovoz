using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using FluentNHibernate.Data;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using Google.OrTools.ConstraintSolver;
using Gtk;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Tdi;
using QSOrmProject;
using Vodovoz.Additions;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Repositories.Sale;
using Vodovoz.Repository.Logistics;
using Vodovoz.Services;
using Vodovoz.Core.DataService;
using Vodovoz.JournalViewModels;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class AtWorksDlg : TdiTabBase, ITdiDialog, ISingleUoWDialog
	{
		public AtWorksDlg(IAuthorizationService authorizationService)
		{
			this.authorizationService =
				authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			this.Build();

			var colorWhite = new Color(0xff, 0xff, 0xff);
			var colorLightRed = new Color(0xff, 0x66, 0x66);
			ytreeviewAtWorkDrivers.ColumnsConfig = FluentColumnsConfig<AtWorkDriver>.Create()
				.AddColumn("Приоритет")
					.AddNumericRenderer(x => x.PriorityAtDay)
					.Editing(new Gtk.Adjustment(6, 1, 10, 1, 1, 1))
				.AddColumn("Статус")
					.AddTextRenderer(x => x.Status.GetEnumTitle())
				.AddColumn("Причина")
					.AddTextRenderer(x => x.Reason)
						.AddSetter((cell, driver) => cell.Editable = driver.Status == AtWorkDriver.DriverStatus.NotWorking)
				.AddColumn("Водитель")
					.AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Скор.")
					.AddTextRenderer(x => x.Employee.DriverSpeed.ToString("P0"))
				.AddColumn("График работы")
					.AddComboRenderer(x => x.DaySchedule)
					.SetDisplayFunc(x => x.Name)
					.FillItems(UoW.GetAll<DeliveryDaySchedule>().ToList())
					.Editing()
				.AddColumn("Оконч. работы")
					.AddTextRenderer(x => x.EndOfDayText).Editable()
				.AddColumn("Экспедитор")
					.AddComboRenderer(x => x.WithForwarder)
					.SetDisplayFunc(x => x.Employee.ShortName).Editing().Tag(Columns.Forwarder)
				.AddColumn("Автомобиль")
					.AddPixbufRenderer(x => x.Car != null && x.Car.IsCompanyCar ? vodovozCarIcon : null)
					.AddTextRenderer(x => x.Car != null ? x.Car.RegistrationNumber : "нет")
				.AddColumn("База")
					.AddComboRenderer(x => x.GeographicGroup)
					.SetDisplayFunc(x => x.Name)
					.FillItems(GeographicGroupRepository.GeographicGroupsWithCoordinates(UoW))
					.AddSetter(
						(c, n) => {
							c.Editable = true;
							c.BackgroundGdk = n.GeographicGroup == null
								? colorLightRed
								: colorWhite;
						}
					)
				.AddColumn("Грузоп.")
					.AddTextRenderer(x => x.Car != null ? x.Car.MaxWeight.ToString("D") : null)
				.AddColumn("Районы доставки")
					.AddTextRenderer(x => string.Join(", ", x.DistrictsPriorities.Select(d => d.District.DistrictName)))
				.AddColumn("")
				.AddColumn("Комментарий")
					.AddTextRenderer(x => x.Comment)
						.Editable(true)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.Status == AtWorkDriver.DriverStatus.NotWorking? "gray": "black")
				.Finish();

			ytreeviewAtWorkDrivers.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewAtWorkDrivers.Selection.Changed += YtreeviewDrivers_Selection_Changed;

			ytreeviewOnDayForwarders.ColumnsConfig = FluentColumnsConfig<AtWorkForwarder>.Create()
				.AddColumn("Экспедитор").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Едет с водителем").AddTextRenderer(x => RenderForwaderWithDriver(x))
				.Finish();
			ytreeviewOnDayForwarders.Selection.Mode = Gtk.SelectionMode.Multiple;
			ytreeviewOnDayForwarders.Selection.Changed += YtreeviewForwarders_Selection_Changed;
			
			ydateAtWorks.Date = DateTime.Today;
			
			int currentUserId = ServicesConfig.CommonServices.UserService.CurrentUserId;
			CanReturnDriver = ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission("can_return_driver_to_work", currentUserId);
		}
		
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		private readonly Gdk.Pixbuf vodovozCarIcon = Pixbuf.LoadFromResource("Vodovoz.icons.buttons.vodovoz-logo.png");
		private readonly IAuthorizationService authorizationService;
			
		private IList<AtWorkDriver> driversAtDay;
		private IList<AtWorkForwarder> forwardersAtDay;
		private HashSet<AtWorkDriver> driversWithCommentChanged = new HashSet<AtWorkDriver>();
		private GenericObservableList<AtWorkDriver> observableDriversAtDay;
		private GenericObservableList<AtWorkForwarder> observableForwardersAtDay;
		private bool CanReturnDriver;

		public IUnitOfWork UoW { get; } = UnitOfWorkFactory.CreateWithoutRoot();
		public bool HasChanges => UoW.HasChanges;
		public event EventHandler<EntitySavedEventArgs> EntitySaved;
		
		private DateTime DialogAtDate => ydateAtWorks.Date;

		#region Properties

		private IList<AtWorkDriver> DriversAtDay {
			set {
				driversAtDay = value;
				observableDriversAtDay = new GenericObservableList<AtWorkDriver>(driversAtDay);
				ytreeviewAtWorkDrivers.SetItemsSource(observableDriversAtDay);
				observableDriversAtDay.PropertyOfElementChanged += (sender, args) => driversWithCommentChanged.Add(sender as AtWorkDriver);
			}
			get => driversAtDay;
		}

		private IList<AtWorkForwarder> ForwardersAtDay {
			set {
				forwardersAtDay = value;
				if(observableForwardersAtDay != null)
					observableForwardersAtDay.ListChanged -= ObservableForwardersAtDay_ListChanged;
				observableForwardersAtDay = new GenericObservableList<AtWorkForwarder>(forwardersAtDay);
				observableForwardersAtDay.ListChanged += ObservableForwardersAtDay_ListChanged;
				ytreeviewOnDayForwarders.SetItemsSource(observableForwardersAtDay);
				ObservableForwardersAtDay_ListChanged(null);
			}
			get => forwardersAtDay;
		}

		public override string TabName {
			get => $"Работают {ydateAtWorks.Date:d}";
			protected set => throw new InvalidOperationException("Установка протеворечит логике работы.");
		}

		#endregion
		

		#region Events

		#region Buttons
		protected void OnButtonSaveChangesClicked(object sender, EventArgs e)
		{
			Save();
		}
		protected void OnButtonCancelChangesClicked(object sender, EventArgs e)
		{
			UoW.Session.Clear();
			FillDialogAtDay();
		}
		
		protected void OnButtonAddWorkingDriversClicked(object sender, EventArgs e)
		{
			var workDriversAtDay = EmployeeSingletonRepository.GetInstance().GetWorkingDriversAtDay(UoW, DialogAtDate);

			if(workDriversAtDay.Count > 0) {
				foreach(var driver in workDriversAtDay) {
					if(driversAtDay.Any(x => x.Employee.Id == driver.Id)) {
						logger.Warn($"Водитель {driver.ShortName} уже добавлен. Пропускаем...");
						continue;
					}

					var car = CarRepository.GetCarByDriver(UoW, driver);
					var daySchedule = GetDriverWorkDaySchedule(driver, new BaseParametersProvider());

					var atwork = new AtWorkDriver(driver, DialogAtDate, car, daySchedule);
					GetDefaultForwarder(driver, atwork);

					observableDriversAtDay.Add(atwork);
				}
			}
			DriversAtDay = driversAtDay.OrderBy(x => x.Employee.ShortName).ToList();
		}
		
		protected void OnButtonAddDriverClicked(object sender, EventArgs e)
		{
			var drvFilter = new EmployeeFilterViewModel();
			drvFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking 
			);

			var selectDrivers = new EmployeesJournalViewModel(
				drvFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			) {
				SelectionMode = JournalSelectionMode.Multiple,
				TabName = "Водители"
			};
			var list = selectDrivers.Items;
			selectDrivers.OnEntitySelectedResult += SelectDrivers_OnEntitySelectedResult;
			TabParent.AddSlaveTab(this, selectDrivers);
		}

		
		protected void OnButtonRemoveDriverClicked(object sender, EventArgs e)
		{
			var toDel = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
			
			foreach(var driver in toDel)
			{
				if (driver.Id > 0)
				{
					ChangeButtonAddRemove(driver.Status == AtWorkDriver.DriverStatus.IsWorking);
					if (driver.Status == AtWorkDriver.DriverStatus.NotWorking)
					{
						if (CanReturnDriver)
						{
							driver.Status = AtWorkDriver.DriverStatus.IsWorking;
						}
					}
					else
					{
						driver.Status = AtWorkDriver.DriverStatus.NotWorking;
						driver.AuthorRemovedDriver = EmployeeSingletonRepository.GetInstance().GetEmployeeForCurrentUser(UoW);
						driver.RemovedDate = DateTime.Now;
					}
				}
				observableDriversAtDay.OnPropertyChanged(nameof(driver.Status));
			}
		}

		protected void OnButtonDriverSelectAutoClicked(object sender, EventArgs e)
		{
			var SelectDriverCar = new OrmReference(
				UoW,
				CarRepository.ActiveCompanyCarsQuery()
			);
			var driver = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>().First();
			SelectDriverCar.Tag = driver;
			SelectDriverCar.Mode = OrmReferenceMode.Select;
			SelectDriverCar.ObjectSelected += SelectDriverCar_ObjectSelected;
			TabParent.AddSlaveTab(this, SelectDriverCar);
		}
		
				protected void OnButtonAppointForwardersClicked(object sender, EventArgs e)
		{
			var toAdd = new List<AtWorkForwarder>();
			foreach(var forwarder in ForwardersAtDay.Where(f => DriversAtDay.All(d => d.WithForwarder != f))) {
				var defaulDriver = DriversAtDay.FirstOrDefault(d => d.WithForwarder == null && d.Employee.DefaultForwarder?.Id == forwarder.Employee.Id);
				if(defaulDriver != null)
					defaulDriver.WithForwarder = forwarder;
				else
					toAdd.Add(forwarder);
			}

			if(toAdd.Count == 0)
				return;

			var orders = ScheduleRestrictionRepository.OrdersCountByDistrict(UoW, DialogAtDate, 12);
			var districtsBottles = orders.GroupBy(x => x.DistrictId).ToDictionary(x => x.Key, x => x.Sum(o => o.WaterCount));

			foreach(var forwarder in toAdd) {
				var driversToAdd = DriversAtDay.Where(x => x.WithForwarder == null && x.Car != null && x.Car.TypeOfUse != CarTypeOfUse.CompanyLargus).ToList();

				if(driversToAdd.Count == 0) {
					logger.Warn("Не осталось водителей для добавленя экспедиторов.");
					break;
				}

				Func<int, int> ManOnDistrict = (int districtId) => driversAtDay.Where(dr => dr.Car != null && dr.Car.TypeOfUse != CarTypeOfUse.CompanyLargus && dr.DistrictsPriorities.Any(dd2 => dd2.District.Id == districtId))
																			   .Sum(dr => dr.WithForwarder == null ? 1 : 2);

				var driver = driversToAdd.OrderByDescending(
					x => districtsBottles.Where(db => x.Employee.Districts.Any(dd => dd.District.Id == db.Key))
									   .Max(db => (double)db.Value / ManOnDistrict(db.Key))
					).First();

				var testSum = driversToAdd.ToDictionary(x => x, x => districtsBottles.Where(db => x.Employee.Districts.Any(dd => dd.District.Id == db.Key))
														.Max(db => (double)db.Value / ManOnDistrict(db.Key)));

				driver.WithForwarder = forwarder;
			}

			MessageDialogHelper.RunInfoDialog("Готово.");
		}

		protected void OnButtonOpenCarClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
			TabParent.OpenTab(
				DialogHelper.GenerateDialogHashName<Car>(selected[0].Car.Id),
				() => new CarsDlg(selected[0].Car)
			);
		}
		
		protected void OnButtonEditDistrictsClicked(object sender, EventArgs e)
		{
			districtpriorityview1.Visible = !districtpriorityview1.Visible;
		}

		protected void OnButtonOpenDriverClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
			foreach(var one in selected) {
				TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<Employee>(one.Employee.Id),
					() => new EmployeeDlg(one.Employee.Id)
				);
			}
		}
		
		protected void OnButtonAddForwarderClicked(object sender, EventArgs e)
		{
			var SelectForwarder = new OrmReference(
				UoW,
				EmployeeRepository.ActiveForwarderOrderedQuery()
			) {
				Mode = OrmReferenceMode.MultiSelect
			};
			SelectForwarder.ObjectSelected += SelectForwarder_ObjectSelected;
			OpenSlaveTab(SelectForwarder);
		}
		
		protected void OnButtonRemoveForwarderClicked(object sender, EventArgs e)
		{
			var toDel = ytreeviewOnDayForwarders.GetSelectedObjects<AtWorkForwarder>();
			foreach(var forwarder in toDel) {
				if(forwarder.Id > 0)
					UoW.Delete(forwarder);
				observableForwardersAtDay.Remove(forwarder);
			}
		}
		#endregion

		#region YTreeView

		void YtreeviewDrivers_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveDriver.Sensitive = buttonDriverSelectAuto.Sensitive
				= buttonOpenDriver.Sensitive = ytreeviewAtWorkDrivers.Selection.CountSelectedRows() > 0;

			if(ytreeviewAtWorkDrivers.Selection.CountSelectedRows() != 1 && districtpriorityview1.Visible)
				districtpriorityview1.Visible = false;

			buttonOpenCar.Sensitive = false;

			if(ytreeviewAtWorkDrivers.Selection.CountSelectedRows() == 1) {
				var selected = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
				districtpriorityview1.ListParent = selected[0];
				districtpriorityview1.Districts = selected[0].ObservableDistrictsPriorities;
				buttonOpenCar.Sensitive = selected[0].Car != null;
				ChangeButtonAddRemove(selected[0].Status == AtWorkDriver.DriverStatus.NotWorking);
			}
			districtpriorityview1.Sensitive = ytreeviewAtWorkDrivers.Selection.CountSelectedRows() == 1;
		}
		
		void YtreeviewForwarders_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveForwarder.Sensitive = ytreeviewOnDayForwarders.Selection.CountSelectedRows() > 0;
		}

		void ObservableForwardersAtDay_ListChanged(object aList)
		{
			var renderer = ytreeviewAtWorkDrivers.ColumnsConfig.GetRendererMappingByTagGeneric<ComboRendererMapping<AtWorkDriver, AtWorkForwarder>>(Columns.Forwarder).First();
			renderer.FillItems(ForwardersAtDay, "без экспедитора");
		}
		#endregion
		
		protected void OnYdateAtWorksDateChanged(object sender, EventArgs e)
		{
			FillDialogAtDay();
			OnTabNameChanged();
		}

		void SelectDrivers_OnEntitySelectedResult(object sender, JournalSelectedNodesEventArgs e)
		{
			var addDrivers = e.SelectedNodes;
			logger.Info("Получаем авто для водителей...");
			var onlyNew = addDrivers.Where(x => driversAtDay.All(y => y.Employee.Id != x.Id)).ToList();
			var allCars = CarRepository.GetCarsbyDrivers(UoW, onlyNew.Select(x => x.Id).ToArray());

			foreach(var driver in addDrivers) {
				var drv = UoW.GetById<Employee>(driver.Id);

				if(driversAtDay.Any(x => x.Employee.Id == driver.Id)) {
					logger.Warn($"Водитель {drv.ShortName} уже добавлен. Пропускаем...");
					continue;
				}

				var daySchedule = GetDriverWorkDaySchedule(drv, new BaseParametersProvider());
				var atwork = new AtWorkDriver(drv, DialogAtDate, allCars.FirstOrDefault(x => x.Driver.Id == driver.Id), daySchedule);

				GetDefaultForwarder(drv, atwork);

				driversAtDay.Add(atwork);
			}
			DriversAtDay = driversAtDay.OrderBy(x => x.Employee.ShortName).ToList();
			logger.Info("Ок");
		}

		void SelectDriverCar_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var driver = e.Tag as AtWorkDriver;
			var car = e.Subject as Car;
			driversAtDay.Where(x => x.Car != null && x.Car.Id == car.Id).ToList().ForEach(x => x.Car = null);
			driver.Car = car;
		}
		
		protected void OnHideForwadersToggled(object o, Gtk.ToggledArgs args)
		{
			vboxForwarders.Visible = hideForwaders.ArrowDirection == Gtk.ArrowType.Down;
		}

		void SelectForwarder_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var addForwarder = e.GetEntities<Employee>();
			foreach(var forwarder in addForwarder) {
				if(forwardersAtDay.Any(x => x.Employee.Id == forwarder.Id)) {
					logger.Warn($"Экспедитор {forwarder.ShortName} пропущен так как уже присутствует в списке.");
					continue;
				}
				forwardersAtDay.Add(new AtWorkForwarder(forwarder, DialogAtDate));
			}
			ForwardersAtDay = forwardersAtDay.OrderBy(x => x.Employee.ShortName).ToList();
		}

		#endregion
		
		#region Fuctions

		private void ChangeButtonAddRemove(bool needRemove)
		{
			if (!CanReturnDriver)
			{
				return;
			}
			
			if (needRemove)
			{
				buttonRemoveDriver.Label = "Вернуть водителя";
				buttonRemoveDriver.Image = new Gtk.Image(){Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu)};
			}
			else
			{
				buttonRemoveDriver.Label = "Снять водителя";
				buttonRemoveDriver.Image = new Gtk.Image(){Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu)};
			}
		}

		public void SaveAndClose() { throw new NotImplementedException(); }

		private DeliveryDaySchedule GetDriverWorkDaySchedule(Employee driver, IDefaultDeliveryDaySchedule defaultDelDaySchedule)
		{
			DeliveryDaySchedule daySchedule;
			var drvDaySchedule = driver.ObservableWorkDays.SingleOrDefault(x => (int)x.WeekDay == (int)DialogAtDate.DayOfWeek);

			if(drvDaySchedule == null)
				daySchedule = UoW.GetById<DeliveryDaySchedule>(defaultDelDaySchedule.GetDefaultDeliveryDayScheduleId());
			else
				daySchedule = drvDaySchedule.DaySchedule;

			return daySchedule;
		}

		private void GetDefaultForwarder(Employee driver, AtWorkDriver atwork)
		{
			if(driver.DefaultForwarder != null) {
				var forwarder = ForwardersAtDay.FirstOrDefault(x => x.Employee.Id == driver.DefaultForwarder.Id);

				if(forwarder == null) {
					if(MessageDialogHelper.RunQuestionDialog($"Водитель {driver.ShortName} обычно ездит с экспедитором {driver.DefaultForwarder.ShortName}. Он отсутствует в списке экспедиторов. Добавить его в список?")) {
						forwarder = new AtWorkForwarder(driver.DefaultForwarder, DialogAtDate);
						observableForwardersAtDay.Add(forwarder);
					}
				}

				if(forwarder != null && DriversAtDay.All(x => x.WithForwarder != forwarder)) {
					atwork.WithForwarder = forwarder;
				}
			}
		}
		
		public bool Save()
		{
			// В случае, если вкладка сохраняется, а в списке есть Снятые водители, сделать проверку, что у каждого из них заполнена причина.
			var NotWorkingDrivers = DriversAtDay.ToList()
				.Where(driver => driver.Status == AtWorkDriver.DriverStatus.NotWorking);
			
			if (NotWorkingDrivers.Count() != 0)
				foreach (var atWorkDriver in NotWorkingDrivers)
				{
					if (!String.IsNullOrEmpty(atWorkDriver.Reason)) continue;
					MessageDialogHelper.RunWarningDialog("Не у всех снятых водителей указаны причины!");
					return false;
				}

			// Сохранение изменившихся за этот раз авторов и дат комментариев
			foreach (var atWorkDriver in driversWithCommentChanged)
			{
				atWorkDriver.CommentLastEditedAuthor = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
				atWorkDriver.CommentLastEditedDate = DateTime.Now;
			}
			driversWithCommentChanged.Clear();
			ForwardersAtDay.ToList().ForEach(x => UoW.Save(x));
			DriversAtDay.ToList().ForEach(x => UoW.Save(x));
			UoW.Commit();
			FillDialogAtDay();
			return true;
		}
		
		string RenderForwaderWithDriver(AtWorkForwarder atWork)
		{
			return string.Join(", ", driversAtDay.Where(x => x.WithForwarder == atWork).Select(x => x.Employee.ShortName));
		}

		void FillDialogAtDay()
		{
			UoW.Session.Clear();

			logger.Info("Загружаем экспедиторов на {0:d}...", DialogAtDate);
			ForwardersAtDay = new EntityRepositories.Logistic.AtWorkRepository().GetForwardersAtDay(UoW, DialogAtDate);

			logger.Info("Загружаем водителей на {0:d}...", DialogAtDate);
			DriversAtDay = new EntityRepositories.Logistic.AtWorkRepository().GetDriversAtDay(UoW, DialogAtDate);

			logger.Info("Ок");

			CheckAndCorrectDistrictPriorities();
		}

		//Если дата диалога >= даты активации набора районов и есть хотя бы один район у водителя, который не принадлежит активному набору районов
		private void CheckAndCorrectDistrictPriorities() {
			var activeDistrictsSet = UoW.Session.QueryOver<DistrictsSet>().Where(x => x.Status == DistrictsSetStatus.Active).SingleOrDefault();
			if(DialogAtDate.Date >= activeDistrictsSet.DateActivated.Value.Date) {
				var outDatedpriorities = DriversAtDay.SelectMany(x => x.DistrictsPriorities.Where(d => d.District.DistrictsSet.Id != activeDistrictsSet.Id)).ToList();
				if(!outDatedpriorities.Any()) 
					return;
				
				int deletedCount = 0;
				foreach (var priority in outDatedpriorities) {
					var newDistrict = activeDistrictsSet.ObservableDistricts.FirstOrDefault(x => x.CopyOf == priority.District);
					if(newDistrict == null) {
						priority.Driver.ObservableDistrictsPriorities.Remove(priority);
						UoW.Delete(priority);
						deletedCount++;
					}
					else {
						priority.District = newDistrict;
						UoW.Save(priority);
					}
				}
				MessageDialogHelper.RunInfoDialog($"Были найдены и исправлены устаревшие приоритеты районов.\nУдалено приоритетов, ссылающихся на несуществующий район: {deletedCount}");
				ytreeviewAtWorkDrivers.YTreeModel.EmitModelChanged();
			}
		}
		#endregion
		
		public override void Destroy()
		{
			UoW?.Dispose();
			base.Destroy();
		}
		
		
		enum Columns
		{
			Forwarder
		}
	}
}
