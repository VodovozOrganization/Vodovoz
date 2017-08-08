using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class AtWorksDlg : TdiTabBase, ITdiDialog, IOrmDialog
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		public IUnitOfWork UoW => uow;

		IList<AtWorkDriver> driversAtDay;

		GenericObservableList<AtWorkDriver> observableDriversAtDay;

		private DateTime DialogAtDate
		{
			get { return ydateAtWorks.Date; }
		}

		private IList<AtWorkDriver> DriversAtDay
		{
			set
			{
				driversAtDay = value;
				observableDriversAtDay = new GenericObservableList<AtWorkDriver>(driversAtDay);
				ytreeviewAtWorkDrivers.SetItemsSource(observableDriversAtDay);
			}
			get
			{
				return driversAtDay;
			}
		}

		public override string TabName
		{
			get
			{
				return String.Format("Работают {0:d}", ydateAtWorks.Date);
			}
			protected set
			{
				throw new InvalidOperationException("Установка протеворечит логике работы.");
			}
		}

		Gdk.Pixbuf vodovozCarIcon = Gdk.Pixbuf.LoadFromResource("Vodovoz.icons.buttons.vodovoz-logo.png");

		public AtWorksDlg()
		{
			this.Build();

			ytreeviewAtWorkDrivers.ColumnsConfig = FluentColumnsConfig<AtWorkDriver>.Create()
				.AddColumn("Приоритет").AddNumericRenderer(x => x.PriorityAtDay).Editing(new Gtk.Adjustment(6, 1, 10,1,1,1))
				.AddColumn("Водитель").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Поездок").AddNumericRenderer(x => x.Trips).Editing(new Gtk.Adjustment(1, 0, 10, 1, 1, 1))
				.AddColumn("Оконч. работы").AddTextRenderer(x => x.EndOfDayText).Editable()
				.AddColumn("Автомобиль")
					.AddPixbufRenderer(x => x.Car != null && x.Car.IsCompanyHavings ? vodovozCarIcon : null)
					.AddTextRenderer(x => x.Car != null ? x.Car.RegistrationNumber : "нет")
				.AddColumn("Грузоп.").AddTextRenderer(x => x.Car != null ? x.Car.MaxWeight.ToString("D") : null)
				.AddColumn("Районы доставки").AddTextRenderer(x => String.Join(", ", x.Districts.Select(d => d.District.Name)))
				.Finish();
			ytreeviewAtWorkDrivers.Selection.Mode = Gtk.SelectionMode.Multiple;

			ytreeviewAtWorkDrivers.Selection.Changed += YtreeviewDrivers_Selection_Changed;

			ydateAtWorks.Date = DateTime.Today;
		}

		void FillDialogAtDay()
		{
			uow.Session.Clear();

			logger.Info("Загружаем водителей на {0:d}...", DialogAtDate);
			DriversAtDay = Repository.Logistics.AtWorkRepository.GetDriversAtDay(uow, DialogAtDate);
			logger.Info("Ок");
		}

		protected void OnYdateAtWorksDateChanged(object sender, EventArgs e)
		{
			FillDialogAtDay();
			OnTabNameChanged();
		}

		protected void OnButtonSaveChangesClicked(object sender, EventArgs e)
		{
			Save();
		}

		protected void OnButtonCancelChangesClicked(object sender, EventArgs e)
		{
			uow.Session.Clear();
			FillDialogAtDay();
		}

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public bool Save()
		{
			DriversAtDay.ToList().ForEach(x => uow.Save(x));
			uow.Commit();
			FillDialogAtDay();
			return true;
		}

		public void SaveAndClose()
		{
			throw new NotImplementedException();
		}

		public bool HasChanges
		{
			get
			{
				return uow.HasChanges;
			}
		}

		public object EntityObject => throw new NotImplementedException();

		protected void OnButtonAddDriverClicked(object sender, EventArgs e)
		{
			var SelectDrivers = new OrmReference(
				uow,
				Repository.EmployeeRepository.ActiveDriversOrderedQuery()
			);
			SelectDrivers.Mode = OrmReferenceMode.MultiSelect;
			SelectDrivers.ObjectSelected += SelectDrivers_ObjectSelected;
			TabParent.AddSlaveTab(this, SelectDrivers);
		}

		void SelectDrivers_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var addDrivers = e.GetEntities<Employee>().ToList();
			logger.Info("Получаем авто для водителей...");
			MainClass.MainWin.ProgressStart(2);
			var onlyNew = addDrivers.Where(x => driversAtDay.All(y => y.Employee.Id != x.Id)).ToList();
			var allCars = CarRepository.GetCarsbyDrivers(uow, onlyNew.Select(x => x.Id).ToArray());
			MainClass.MainWin.ProgressAdd();

			foreach (var driver in addDrivers)
			{
				driversAtDay.Add(new AtWorkDriver(driver, DialogAtDate,
												  allCars.FirstOrDefault(x => x.Driver.Id == driver.Id)
												 ));
			}
			MainClass.MainWin.ProgressAdd();
			DriversAtDay = driversAtDay.OrderBy(x => x.Employee.ShortName).ToList();
			logger.Info("Ок");
			MainClass.MainWin.ProgressClose();
		}

		protected void OnButtonRemoveDriverClicked(object sender, EventArgs e)
		{
			var toDel = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
			foreach (var driver in toDel)
			{
				if (driver.Id > 0)
					uow.Delete(driver);
				observableDriversAtDay.Remove(driver);
			}
		}

		protected void OnButtonDriverSelectAutoClicked(object sender, EventArgs e)
		{
			var SelectDriverCar = new OrmReference(
				uow,
				Repository.Logistics.CarRepository.ActiveCompanyCarsQuery()
			);
			var driver = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>().First();
			SelectDriverCar.Tag = driver;
			SelectDriverCar.Mode = OrmReferenceMode.Select;
			SelectDriverCar.ObjectSelected += SelectDriverCar_ObjectSelected;
			TabParent.AddSlaveTab(this, SelectDriverCar);
		}

		void SelectDriverCar_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var driver = e.Tag as AtWorkDriver;
			var car = e.Subject as Car;
			driversAtDay.Where(x => x.Car != null && x.Car.Id == car.Id).ToList().ForEach(x => x.Car = null);
			driver.Car = car;
			if (car.TypeOfUse == CarTypeOfUse.Largus)
				driver.Trips = 2;
		}

		void YtreeviewDrivers_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveDriver.Sensitive = buttonDriverSelectAuto.Sensitive 
				= buttonOpenDriver.Sensitive = ytreeviewAtWorkDrivers.Selection.CountSelectedRows() > 0;

			if (ytreeviewAtWorkDrivers.Selection.CountSelectedRows() != 1 && districtpriorityview1.Visible)
				districtpriorityview1.Visible = false;

			if(ytreeviewAtWorkDrivers.Selection.CountSelectedRows() == 1)
			{
				var selected = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
				districtpriorityview1.ListParent = selected[0];
				districtpriorityview1.Districts = selected[0].ObservableDistricts;
			}
		}
		
		protected void OnButtonEditDistrictsClicked(object sender, EventArgs e)
		{
			districtpriorityview1.Visible = !districtpriorityview1.Visible;
		}

		protected void OnButtonOpenDriverClicked(object sender, EventArgs e)
		{
			var selected = ytreeviewAtWorkDrivers.GetSelectedObjects<AtWorkDriver>();
			foreach(var one in selected)
			{
				TabParent.OpenTab(OrmMain.GenerateDialogHashName<Employee>(one.Employee.Id),
								  () => new EmployeeDlg(one.Employee.Id)
				                 );
			}
		}
	}
}
