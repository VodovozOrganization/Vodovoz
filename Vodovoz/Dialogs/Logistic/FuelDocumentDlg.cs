using System;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using System.Linq;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz
{
	public partial class FuelDocumentDlg : OrmGtkDialogBase<FuelDocument>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		GenericObservableList<TicketsRow> rows;

		public RouteList RouteList { get; set; }

		public decimal FuelBalance { get; set; }

		public FuelDocumentDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<FuelDocument>();
			ConfigureDlg ();
		}

		public FuelDocumentDlg (RouteList routeList, int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<FuelDocument> (id);
			RouteList = routeList;
			ConfigureDlg ();
		}

		//public FuelDocumentDlg (FuelDocument sub) : this (sub.Id) {}

		public FuelDocumentDlg(RouteList routeList)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<FuelDocument> ();

			RouteList = routeList;

			UoWGeneric.Root.Date = DateTime.Now;
			UoWGeneric.Root.Driver = routeList.Car.Driver;
			UoWGeneric.Root.Fuel = routeList.Car.FuelType;
			UoWGeneric.Root.LiterCost = routeList.Car.FuelType.Cost;

			ConfigureDlg();
		}

		private void ConfigureDlg ()
		{
			ydatepicker.Binding.AddBinding(Entity, e => e.Date, w => w.Date).InitializeFromSource();

			yentrydriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			yentrydriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Subject).InitializeFromSource();

			yentryfuel.SubjectType = typeof(FuelType);
			yentryfuel.Binding.AddBinding(Entity, e => e.Fuel, w => w.Subject).InitializeFromSource();

			ytreeTickets.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<TicketsRow>()
				.AddColumn("Талон").AddTextRenderer(x => x.GasTicket.Name)
				.AddColumn("Количество").AddNumericRenderer(x => x.Count).Editing()
				.Adjustment(new Gtk.Adjustment(0, 0, 100, 1, 1, 1))
				.Finish();

			var tikets = Repository.Logistics.GasTicketRepository.GetGasTickets(UoW, Entity.Fuel);
			var list = tikets.Select(x => new TicketsRow{ GasTicket = x }).ToList();
			rows = new GenericObservableList<TicketsRow>(list);
			rows.ListContentChanged += Rows_ListContentChanged;

			yspinDistance.Text = RouteList.ActualDistance.ToString();

			disablespinMoney.Binding.AddBinding(Entity, e => e.PayedForFuel, w => w.ValueAsDecimal).InitializeFromSource();

			UpdateFuelInfo();
			UpdateResutlInfo();
			LoadTicketsFromEntiry();

			ytreeTickets.ItemsDataSource = rows;
		}

		void Rows_ListContentChanged (object sender, EventArgs e)
		{
			Entity.UpdateOperation(rows.ToDictionary(k => k.GasTicket, v => v.Count));
			UpdateResutlInfo();
		}

		private void UpdateFuelInfo() {
			var text = new List<string>();
			decimal fc = (decimal)RouteList.Car.FuelConsumption;

			var track = Repository.Logistics.TrackRepository.GetTrackForRouteList(UoW, RouteList.Id);
			bool hasTrack = track != null && track.Distance.HasValue;

			if(hasTrack)
				text.Add(string.Format("Расстояние по треку: {0:f1} км.", track.Distance));
			
			if(RouteList.ActualDistance > 0)
				text.Add(string.Format("Оплачиваемое расстояние {0}", RouteList.ActualDistance));

			if (RouteList.Car.FuelType != null)
				text.Add(string.Format("Вид топлива: {0}", RouteList.Car.FuelType.Name));
			else
				text.Add("Не указан вид топлива");

			text.Add(string.Format("Израсходовано топлива: {0:f2} л. ({1:f2} л/100км)",
				fc / 100 * RouteList.ActualDistance, fc));

			if (RouteList.Car.FuelType != null)
			{
				decimal totalBalance = 0;
				if (Entity.Operation == null || Entity.Operation.Id == 0)
				{
					FuelBalance = Repository.Operations.FuelRepository.GetFuelBalance(
						UoW, RouteList.Driver, RouteList.Car.FuelType);
				}
				else
				{
					FuelBalance = Repository.Operations.FuelRepository.GetFuelBalance(
						UoW, RouteList.Driver, RouteList.Car.FuelType, null, Entity.Operation.Id);
					totalBalance -= Entity.Operation.LitersGived;
				}

				totalBalance += FuelBalance;
				text.Add(string.Format("Текущий остаток топлива {0:F2} л.", totalBalance));
			}

			ytextviewFuelInfo.Buffer.Text = String.Join("\n", text);
		}

		private void UpdateResutlInfo () 
		{
			var text = new List<string>();
			decimal litersGived = Entity.Operation?.LitersGived ?? default(decimal);
			text.Add(string.Format("Итого выдано {0:N2} литров", litersGived));
			text.Add(string.Format("Баланс после выдачи {0:N2}", FuelBalance + litersGived));
			labelResultInfo.Text = string.Join("\n", text);
		}

		public override bool Save ()
		{
			Entity.UpdateRowList(rows.ToDictionary(k => k.GasTicket, v => v.Count));
			var valid = new QSValidator<FuelDocument> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем топливный документ...");
			UoWGeneric.Save();
			return true;
		}

		protected void OnDisablespinMoneyValueChanged (object sender, EventArgs e)
		{
			Entity.UpdateOperation(rows.ToDictionary(k => k.GasTicket, v => v.Count));
			UpdateResutlInfo();

		}

		private void LoadTicketsFromEntiry() 
		{
			foreach (var ticket in Entity.FuelTickets)
			{
				var item = rows.FirstOrDefault(x => x.GasTicket.Id == ticket.GasTicket.Id);
				if (item == null)
				{
					item = new TicketsRow();
					item.GasTicket = ticket.GasTicket;
					rows.Add(item);
				}
				item.Count = ticket.TicketsCount;
			}
		}


		protected void OnButtonSetRemainClicked (object sender, EventArgs e)
		{
			decimal balance = 0;
			decimal litersGived = Entity.Operation?.LitersGived ?? default(decimal);
			balance = FuelBalance + litersGived;

			decimal moneyToPay = -balance * Entity.Fuel.Cost;

			if (Entity.PayedForFuel == null && moneyToPay > 0)
				Entity.PayedForFuel = 0;
			
			Entity.PayedForFuel += moneyToPay;

			if (Entity.PayedForFuel <= 0)
				Entity.PayedForFuel = null;
		}

		private class TicketsRow : PropertyChangedBase
		{
			public GazTicket GasTicket  { get; set; }
			int count;
			public int Count
			{
				get
				{
					return count;
				}
				set
				{
					SetField(ref count, value, () => Count);
				}
			}
		}
	}


}

