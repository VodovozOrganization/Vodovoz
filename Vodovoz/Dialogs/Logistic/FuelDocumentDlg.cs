using System;
using System.Collections.Generic;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class FuelDocumentDlg : TdiTabBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public IUnitOfWork UoW { get; set; }

		public FuelDocument FuelDocument { get; set; }

		RouteList routeList;

		bool autoCommit = false;

		public decimal FuelBalance { get; set; }

		public decimal FuelOutlayed { get; set; }

		decimal spentFuel;
		decimal spentFuelConfirmed;

		/// <summary>
		/// Открывает диалог выдачи топлива, с коммитом изменений в родительском UoW
		/// </summary>
		public FuelDocumentDlg(IUnitOfWork uow, RouteList rl)
		{
			this.Build ();
			UoW = uow;
			FuelDocument = new FuelDocument();
			autoCommit = false;
			routeList = rl;

			FuelDocument.Date 	  = DateTime.Now;
			FuelDocument.Car 	  = routeList.Car;
			FuelDocument.Driver 	  = routeList.Driver;
			FuelDocument.Fuel 	  = routeList.Car.FuelType;
			FuelDocument.LiterCost = routeList.Car.FuelType.Cost;
			FuelDocument.RouteList = routeList;
			ConfigureDlg();
		}

		/// <summary>
		/// Открывает диалог выдачи топлива, с автоматическим коммитом всех изменений
		/// </summary>
		public FuelDocumentDlg(RouteList rl)
		{
			this.Build();
			var uow = UnitOfWorkFactory.CreateWithNewRoot<FuelDocument>();
			UoW = uow;
			FuelDocument = uow.Root;
			autoCommit = true;

			routeList = UoW.GetById<RouteList>(rl.Id);

			FuelDocument.Date = DateTime.Now;
			FuelDocument.Car = routeList.Car;
			FuelDocument.Driver = routeList.Driver;
			FuelDocument.Fuel = routeList.Car.FuelType;
			FuelDocument.LiterCost = routeList.Car.FuelType.Cost;
			FuelDocument.RouteList = routeList;
			ConfigureDlg();
		}

		private void ConfigureDlg ()
		{
			TabName = "Выдача топлива";
			ydatepicker.Binding.AddBinding(FuelDocument, e => e.Date, w => w.Date).InitializeFromSource();

			var filterDriver = new EmployeeFilter(UoW);
			filterDriver.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.driver);
			yentrydriver.RepresentationModel = new EmployeesVM(filterDriver);
			yentrydriver.Binding.AddBinding(FuelDocument, e => e.Driver, w => w.Subject).InitializeFromSource();

			yentryCar.SubjectType = typeof(Car);
			yentryCar.Binding.AddBinding(FuelDocument, e => e.Car, w => w.Subject).InitializeFromSource();

			yentryfuel.SubjectType = typeof(FuelType);
			yentryfuel.Binding.AddBinding(FuelDocument, e => e.Fuel, w => w.Subject).InitializeFromSource();

			yspinFuelTicketLiters.Binding.AddBinding (FuelDocument, e => e.FuelCoupons, w => w.ValueAsInt).InitializeFromSource ();

			disablespinMoney.Binding.AddBinding(FuelDocument, e => e.PayedForFuel, w => w.ValueAsDecimal).InitializeFromSource();
			spinFuelPrice.Binding.AddBinding(FuelDocument, e => e.LiterCost, w => w.ValueAsDecimal).InitializeFromSource();

			UpdateFuelInfo();
			UpdateResutlInfo();
			FuelDocument.PropertyChanged += FuelDocument_PropertyChanged;
		}


		private void UpdateFuelInfo() {
			var text = new List<string>();
			decimal fc = (decimal)routeList.Car.FuelConsumption;

			var track = Repository.Logistics.TrackRepository.GetTrackForRouteList(UoW, routeList.Id);
			bool hasTrack = track != null && track.Distance.HasValue;

			if(hasTrack)
				text.Add(string.Format("Расстояние по треку: {0:f1}({1:N1}+{2:N1}) км.", track.TotalDistance, track.Distance ?? 0, track.DistanceToBase ?? 0));
			
			if(routeList.ActualDistance > 0)
				text.Add(string.Format("Оплачиваемое расстояние {0}", routeList.ActualDistance));

			if (routeList.Car.FuelType != null)
			{
				var fuelOtlayedOp = routeList.FuelOutlayedOperation;
				var entityOp = FuelDocument.Operation;

				text.Add(string.Format("Вид топлива: {0}", routeList.Car.FuelType.Name));

				var exclude = new List<int>();
				if (entityOp != null && entityOp.Id != 0)
				{
					exclude.Add(FuelDocument.Operation.Id);
				}
				if (fuelOtlayedOp != null && fuelOtlayedOp.Id != 0)
				{
					exclude.Add(routeList.FuelOutlayedOperation.Id);
				}
				if (exclude.Count == 0)
					exclude = null;

				Car car = routeList.Car;
				Employee driver = routeList.Driver;
				if (car.IsCompanyHavings)
					driver = null;
				else
					car = null;
				
				FuelBalance = Repository.Operations.FuelRepository.GetFuelBalance(
					UoW, driver, car, routeList.Car.FuelType, null, exclude?.ToArray());

				text.Add(string.Format("Остаток без документа {0:F2} л.", FuelBalance));
			} else {
				text.Add("Не указан вид топлива");
			}

			spentFuel = FuelOutlayed = fc / 100 * routeList.ActualDistance;

			text.Add(string.Format("Израсходовано топлива: {0:f2} л. ({1:f2} л/100км)",
			                       FuelOutlayed, fc));

			if(routeList.ConfirmedDistance != 0 && routeList.ConfirmedDistance != routeList.ActualDistance) {

				spentFuelConfirmed = (decimal)routeList.Car.FuelConsumption
				                                              / 100 * routeList.ConfirmedDistance;

				text.Add(string.Format("Топливо подтвержденное логистами: {0:F2} л.", spentFuelConfirmed));
			}


			ytextviewFuelInfo.Buffer.Text = String.Join("\n", text);
		}

		private void UpdateResutlInfo () 
		{
			decimal litersGived = FuelDocument.Operation?.LitersGived ?? default(decimal);

			var text = new List<string>();

			if(routeList.ConfirmedDistance != 0 && routeList.ConfirmedDistance != routeList.ActualDistance) {
				spentFuel = spentFuelConfirmed;
			}

			text.Add(string.Format("Итого выдано {0:N2} литров", litersGived));
			text.Add(string.Format("Баланс после выдачи {0:N2}", FuelBalance + litersGived - spentFuel));

			labelResultInfo.Text = string.Join("\n", text);
		}

		public bool Save ()
		{
			Employee cashier = FuelDocument.GetActualCashier(UoW);
			if(cashier == null) return false;

			if(FuelDocument.Author == null) {
				FuelDocument.Author = cashier;
			}

			FuelDocument.LastEditor = cashier;

			FuelDocument.LastEditDate = DateTime.Now;

			if (FuelDocument.FuelCashExpense != null)
			{
				FuelDocument.FuelCashExpense.Casher = cashier;
			}

			var valid = new QSValidator<FuelDocument>(FuelDocument);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем топливный документ...");
			routeList.ObservableFuelDocuments.Add(FuelDocument);
			if(autoCommit) {
				UoW.Save();
			} else {
				UoW.Save(FuelDocument);
			}
			return true;
		}

		protected void OnDisablespinMoneyValueChanged (object sender, EventArgs e)
		{
			OnFuelUpdated ();
		}

		private void OnFuelUpdated()
		{
			Employee cashier = FuelDocument.GetActualCashier(UoW);
			if(cashier == null) {
				MessageDialogWorks.RunErrorDialog(
					"Ваш пользователь не привязан к действующему сотруднику, Вы не можете выдавать денежные средства, так как некого указывать в качестве кассира.");
				return;
			}

			FuelDocument.Fuel.Cost = spinFuelPrice.ValueAsDecimal;
			FuelDocument.UpdateOperation();
			FuelDocument.UpdateFuelCashExpense(UoW, cashier);
			UpdateResutlInfo();
			UpdateFuelCashExpenseInfo();
		}

		void FuelDocument_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (FuelDocument.FuelCoupons))
				OnFuelUpdated ();
		}

		private void UpdateFuelCashExpenseInfo()
		{
			spinFuelPrice.Sensitive = disablespinMoney.Active;
			if (FuelDocument.FuelCashExpense == null && !FuelDocument.PayedForFuel.HasValue)
			{
				buttonOpenExpense.Sensitive = false;
				labelExpenseInfo.Text = "";
			}
			if (FuelDocument.PayedForFuel.HasValue) {
				if (FuelDocument.FuelCashExpense.Id <= 0) {
					buttonOpenExpense.Sensitive = false;
					labelExpenseInfo.Text = "Расходный ордер будет создан";
				}
				if (FuelDocument.FuelCashExpense.Id > 0) {
					buttonOpenExpense.Sensitive = true;
					labelExpenseInfo.Text = "";
				}
			}
		}

		protected void OnButtonSetRemainClicked (object sender, EventArgs e)
		{
			decimal litersBalance = 0;
			decimal litersGived = FuelDocument.Operation?.LitersGived ?? default(decimal);

			litersBalance = FuelBalance + litersGived - spentFuel;

			decimal moneyToPay = -litersBalance * spinFuelPrice.ValueAsDecimal;

			if (FuelDocument.PayedForFuel == null && moneyToPay > 0)
				FuelDocument.PayedForFuel = 0;
			
			FuelDocument.PayedForFuel += moneyToPay;

			if (FuelDocument.PayedForFuel <= 0)
				FuelDocument.PayedForFuel = null;
		}

		protected void OnButtonOpenExpenseClicked (object sender, EventArgs e)
		{
			if (FuelDocument.FuelCashExpense?.Id > 0)
				TabParent.AddSlaveTab(this, new CashExpenseDlg(FuelDocument.FuelCashExpense.Id));
		}

		protected void OnButtonFuelBy20Clicked (object sender, EventArgs e)
		{
			var needLiters = FuelOutlayed - FuelBalance;

			FuelDocument.FuelCoupons = (((int)needLiters / 20) * 20);
		}

		protected void OnButtonFuelBy10Clicked (object sender, EventArgs e)
		{
			var needLiters = FuelOutlayed - FuelBalance;

			FuelDocument.FuelCoupons = (((int)needLiters / 10) * 10);
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			Save();
			OnCloseTab(false);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}
	}
}