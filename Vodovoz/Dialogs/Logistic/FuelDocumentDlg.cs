using System;
using System.Collections.Generic;
using System.Linq;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository;

namespace Vodovoz
{
	public partial class FuelDocumentDlg : OrmGtkDialogBase<FuelDocument>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public RouteList RouteListClosing { get; set; }

		public decimal FuelBalance { get; set; }

		public decimal FuelOutlayed { get; set; }

		public FuelDocumentDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<FuelDocument>();
			ConfigureDlg ();
		}

		public FuelDocumentDlg (RouteList routeListClosing, int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<FuelDocument> (id);
			RouteListClosing = routeListClosing;
			ConfigureDlg ();
		}

		//public FuelDocumentDlg (FuelDocument sub) : this (sub.Id) {}

		public FuelDocumentDlg(RouteList routeListClosing)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<FuelDocument> ();

			RouteListClosing = routeListClosing;

			UoWGeneric.Root.Date 	  = DateTime.Now;
			UoWGeneric.Root.Car 	  = RouteListClosing.Car;
			UoWGeneric.Root.Driver 	  = RouteListClosing.Driver;
			UoWGeneric.Root.Fuel 	  = RouteListClosing.Car.FuelType;
			UoWGeneric.Root.LiterCost = RouteListClosing.Car.FuelType.Cost;

			ConfigureDlg();
		}

		private void ConfigureDlg ()
		{
			ydatepicker.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			yentrydriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			yentrydriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			yentryCar.SubjectType = typeof(Car);
			yentryCar.Binding.AddBinding(Entity, e => e.Car, w => w.Subject).InitializeFromSource();

			yentryfuel.SubjectType = typeof(FuelType);
			yentryfuel.Binding.AddBinding(Entity, e => e.Fuel, w => w.Subject).InitializeFromSource();

			yspinFuelTicketLiters.Binding.AddBinding (Entity, e => e.FuelCoupons, w => w.ValueAsInt).InitializeFromSource ();

			disablespinMoney.Binding.AddBinding(Entity, e => e.PayedForFuel, w => w.ValueAsDecimal).InitializeFromSource();

			UpdateFuelInfo();
			UpdateResutlInfo();
			Entity.PropertyChanged += FuelDocument_PropertyChanged;
		}


		private void UpdateFuelInfo() {
			var text = new List<string>();
			decimal fc = (decimal)RouteListClosing.Car.FuelConsumption;

			var track = Repository.Logistics.TrackRepository.GetTrackForRouteList(UoW, RouteListClosing.Id);
			bool hasTrack = track != null && track.Distance.HasValue;

			if(hasTrack)
				text.Add(string.Format("Расстояние по треку: {0:f1}({1:N1}+{2:N1}) км.", track.TotalDistance, track.Distance ?? 0, track.DistanceToBase ?? 0));
			
			if(RouteListClosing.ActualDistance > 0)
				text.Add(string.Format("Оплачиваемое расстояние {0}", RouteListClosing.ActualDistance));

			if (RouteListClosing.Car.FuelType != null)
			{
				var fuelOtlayedOp = RouteListClosing.FuelOutlayedOperation;
				var entityOp = Entity.Operation;

				text.Add(string.Format("Вид топлива: {0}", RouteListClosing.Car.FuelType.Name));

				var exclude = new List<int>();
				if (entityOp != null && entityOp.Id != 0)
				{
					exclude.Add(Entity.Operation.Id);
				}
				if (fuelOtlayedOp != null && fuelOtlayedOp.Id != 0)
				{
					exclude.Add(RouteListClosing.FuelOutlayedOperation.Id);
				}
				if (exclude.Count == 0)
					exclude = null;

				Car car = RouteListClosing.Car;
				Employee driver = RouteListClosing.Driver;
				if (car.IsCompanyHavings)
					driver = null;
				else
					car = null;
				
				FuelBalance = Repository.Operations.FuelRepository.GetFuelBalance(
					UoW, driver, car, RouteListClosing.Car.FuelType, null, exclude?.ToArray());

				text.Add(string.Format("Остаток без документа {0:F2} л.", FuelBalance));
			} else {
				text.Add("Не указан вид топлива");
			}

			FuelOutlayed = fc / 100 * RouteListClosing.ActualDistance;

			text.Add(string.Format("Израсходовано топлива: {0:f2} л. ({1:f2} л/100км)",
			                       FuelOutlayed, fc));

			ytextviewFuelInfo.Buffer.Text = String.Join("\n", text);
		}

		private void UpdateResutlInfo () 
		{
			decimal litersGived = Entity.Operation?.LitersGived ?? default(decimal);
			decimal spentFuel = (decimal)RouteListClosing.Car.FuelConsumption
								 / 100 * RouteListClosing.ActualDistance;
			
			var text = new List<string>();
			text.Add(string.Format("Итого выдано {0:N2} литров", litersGived));
			text.Add(string.Format("Баланс после выдачи {0:N2}", FuelBalance + litersGived - spentFuel));

			labelResultInfo.Text = string.Join("\n", text);
		}

		public override bool Save ()
		{
			var cashier = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if (cashier == null)
			{
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, Вы не можете выдавать денежные средства, так как некого указывать в качестве кассира.");
				return false;
			}
			if (Entity.FuelCashExpense != null)
			{
				if(Entity.Author == null)
				{
					Entity.Author = cashier;
				}
				Entity.FuelCashExpense.Casher = Entity.LastEditor = cashier;
				Entity.LastEditDate = DateTime.Now;
			}

			var valid = new QSValidator<FuelDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			var routeList = UoW.GetById<RouteList>(RouteListClosing.Id);
			if(routeList.FuelGivedDocument == null) {
				routeList.FuelGivedDocument = UoWGeneric.Root;
				RouteListClosing.FuelGivedDocument = UoWGeneric.Root;
			}

			logger.Info ("Сохраняем топливный документ...");
			UoWGeneric.Save();
			return true;
		}

		protected void OnDisablespinMoneyValueChanged (object sender, EventArgs e)
		{
			OnFuelUpdated ();
		}

		private void OnFuelUpdated()
		{
			Entity.UpdateOperation();
			Entity.UpdateFuelCashExpense(UoW, RouteListClosing.Cashier, RouteListClosing.Id);
			UpdateResutlInfo();
			UpdateFuelCashExpenseInfo();
		}

		void FuelDocument_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (Entity.FuelCoupons))
				OnFuelUpdated ();
		}

		private void UpdateFuelCashExpenseInfo()
		{
			if (Entity.FuelCashExpense == null && !Entity.PayedForFuel.HasValue)
			{
				buttonOpenExpense.Sensitive = false;
				labelExpenseInfo.Text = "";
			}
			if (Entity.PayedForFuel.HasValue) {
				if (Entity.FuelCashExpense.Id <= 0) {
					buttonOpenExpense.Sensitive = false;
					labelExpenseInfo.Text = "Расходный ордер будет создан";
				}
				if (Entity.FuelCashExpense.Id > 0) {
					buttonOpenExpense.Sensitive = true;
					labelExpenseInfo.Text = "";
				}
			}
		}

		protected void OnButtonSetRemainClicked (object sender, EventArgs e)
		{
			decimal litersBalance = 0;
			decimal litersGived = Entity.Operation?.LitersGived ?? default(decimal);
			decimal spentFuel = (decimal)RouteListClosing.Car.FuelConsumption
				/ 100 * RouteListClosing.ActualDistance;
			litersBalance = FuelBalance + litersGived - spentFuel;

			decimal moneyToPay = -litersBalance * Entity.Fuel.Cost;

			if (Entity.PayedForFuel == null && moneyToPay > 0)
				Entity.PayedForFuel = 0;
			
			Entity.PayedForFuel += moneyToPay;

			if (Entity.PayedForFuel <= 0)
				Entity.PayedForFuel = null;
		}

		protected void OnButtonOpenExpenseClicked (object sender, EventArgs e)
		{
			if (Entity.FuelCashExpense?.Id > 0)
				TabParent.AddSlaveTab(this, new CashExpenseDlg(Entity.FuelCashExpense.Id));
		}

		protected void OnButtonFuelBy20Clicked (object sender, EventArgs e)
		{
			var needLiters = FuelOutlayed - FuelBalance;

			Entity.FuelCoupons = (((int)needLiters / 20) * 20);
		}

		protected void OnButtonFuelBy10Clicked (object sender, EventArgs e)
		{
			var needLiters = FuelOutlayed - FuelBalance;

			Entity.FuelCoupons = (((int)needLiters / 10) * 10);
		}
	}
}

