using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using Vodovoz.Repository.Logistics;
using System.Text;

namespace Vodovoz
{
	public partial class RouteListClosingDlg : OrmGtkDialogBase<RouteList>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private Track track = null;
		private decimal balanceBeforeOp = default(decimal);
		private bool editing = true;
		private Employee previousForwarder = null;

		List<ReturnsNode> allReturnsToWarehouse;
		int bottlesReturnedToWarehouse;
		int bottlesReturnedTotal;

		public RouteListClosingDlg(RouteList routeList) : this(routeList.Id){}

		public RouteListClosingDlg(int routeListId, bool canEdit = true)
		{
			this.Build();

			PerformanceHelper.StartMeasurement();

			editing = canEdit;
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(routeListId);

			TabName = String.Format("Закрытие маршрутного листа №{0}", Entity.Id);
			PerformanceHelper.AddTimePoint("Создан UoW");
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{			
			referenceCar.Binding.AddBinding(Entity, rl => rl.Car, widget => widget.Subject).InitializeFromSource();
			referenceCar.Sensitive = editing;

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery();
			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.SetObjectDisplayFunc<Employee>(r => StringWorks.PersonNameWithInitials(r.LastName, r.Name, r.Patronymic));
			referenceDriver.Sensitive = editing;

			previousForwarder = Entity.Forwarder;
			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery();
			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.SetObjectDisplayFunc<Employee>(r => StringWorks.PersonNameWithInitials(r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Sensitive = editing;
			referenceForwarder.Changed += ReferenceForwarder_Changed;

			referenceLogistican.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery();
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.SetObjectDisplayFunc<Employee>(r => StringWorks.PersonNameWithInitials(r.LastName, r.Name, r.Patronymic));
			referenceLogistican.Sensitive = editing;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = editing;

			yspinActualDistance.Binding.AddBinding(Entity, rl => rl.ActualDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinActualDistance.IsEditable = editing;

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = editing;

			ycheckConfirmDifferences.Binding.AddBinding(Entity, e => e.DifferencesConfirmed, w => w.Active).InitializeFromSource();
			ycheckConfirmDifferences.Sensitive = editing;

			PerformanceHelper.AddTimePoint("Создан диалог");

			//Предварительная загрузка объектов для ускорения.
/*			Vodovoz.Domain.Orders.Order orderAlias = null;
			var clients = UoW.Session.QueryOver<RouteListItem>()
				.Where(rli => rli.RouteList.Id == Entity.Id)
				.JoinAlias(rli => rli.Order, () => orderAlias)
				.Fetch(rli => rli.Order.Client).Eager
				.List();
				//.Select(Projections. a => orderAlias.Client).Future();
				//.List<Counterparty>();
*/
			PerformanceHelper.AddTimePoint("Предварительная загрузка");

			routeListAddressesView.UoW = UoW;
			routeListAddressesView.RouteList = Entity;
			foreach (RouteListItem item in routeListAddressesView.Items)
			{
				item.Order.ObservableOrderItems.ElementChanged += OnOrderReturnsChanged;
				item.Order.ObservableOrderEquipments.ElementChanged += OnOrderReturnsChanged;
			}
			routeListAddressesView.Items.ElementChanged += OnRouteListItemChanged;
			routeListAddressesView.OnClosingItemActivated += OnRouteListItemActivated;
			routeListAddressesView.Sensitive = editing;
			routeListAddressesView.ColumsVisibility = !ycheckHideCells.Active;
			PerformanceHelper.AddTimePoint("заполнили список адресов");
			allReturnsToWarehouse = GetReturnsToWarehouseByCategory(Entity.Id, Nomenclature.GetCategoriesForShipment());
			bottlesReturnedToWarehouse = (int)GetReturnsToWarehouseByCategory(Entity.Id, new []{ NomenclatureCategory.bottle })
				.Sum(item => item.Amount);
			var returnableOrderItems = routeListAddressesView.Items
				.Where(address => address.IsDelivered())
				.SelectMany(address => address.Order.OrderItems)
				.Where(orderItem => !orderItem.Nomenclature.Serial)
				.Where(orderItem => Nomenclature.GetCategoriesForShipment().Any(nom => nom == orderItem.Nomenclature.Category));
			foreach (var item in returnableOrderItems)
			{
				if (allReturnsToWarehouse.All(r => r.NomenclatureId != item.Nomenclature.Id))
					allReturnsToWarehouse.Add(new ReturnsNode
						{
							Name = item.Nomenclature.Name,
							Trackable = item.Nomenclature.Serial,
							NomenclatureId = item.Nomenclature.Id,
							Amount = 0
						});
			}
			PerformanceHelper.AddTimePoint("Получили возврат на склад");
			if(!Entity.ClosingFilled)
				FirstFillClosing();

			PerformanceHelper.AddTimePoint("Закончено первоначальное заполнение");

			hbox6.Remove(vboxHidenPanel);
			rightsidepanel1.Panel = vboxHidenPanel;

			routelistdiscrepancyview.FindDiscrepancies(Entity.Addresses, allReturnsToWarehouse);
			routelistdiscrepancyview.FineChanged += Routelistdiscrepancyview_FineChanged;
			routelistdiscrepancyview.Sensitive = editing;
			PerformanceHelper.AddTimePoint("Заполнили расхождения");

			buttonAddTicket.Sensitive = Entity.Car?.FuelType?.Cost != null && Entity.Driver != null;

			LoadDataFromFine();
			OnItemsUpdated();
			PerformanceHelper.AddTimePoint("Загрузка штрафов");
			GetFuelInfo();
			UpdateFuelInfo();
			PerformanceHelper.AddTimePoint("Загрузка бензина");

			PerformanceHelper.Main.PrintAllPoints(logger);
		}

		void ReferenceForwarder_Changed (object sender, EventArgs e)
		{
			var newForwarder = Entity.Forwarder;

			if ((previousForwarder == null && newForwarder != null)
			 || (previousForwarder != null && newForwarder == null))
				FirstFillClosing();

			previousForwarder = Entity.Forwarder;
		}

		protected void FirstFillClosing()
		{
			//PerformanceHelper.StartPointsGroup("Первоначальное заполнение");
			//var all = UoW.GetAll<Nomenclature>();

			foreach (var routeListItem in Entity.Addresses)
			{
				PerformanceHelper.StartPointsGroup($"Заказ {routeListItem.Order.Id}");
//				var nomenclatures = routeListItem.Order.OrderItems
//					.Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
//					.Where(item => !item.Nomenclature.Serial).ToList();
				foreach (var item in routeListItem.Order.OrderItems)
				{
					item.ActualCount = routeListItem.IsDelivered() ? item.Count : 0;
				}
				PerformanceHelper.AddTimePoint(logger, "Обработали номенклатуры");
				var equipments = routeListItem.Order.OrderEquipments.Where(orderEq => orderEq.Equipment != null);
				foreach (var item in equipments)
				{
					var returnedToWarehouse = allReturnsToWarehouse.Any(ret => ret.Id == item.Equipment.Id && ret.Amount > 0);
					item.Confirmed = routeListItem.IsDelivered()
						&& (item.Direction == Vodovoz.Domain.Orders.Direction.Deliver && !returnedToWarehouse
							|| item.Direction == Vodovoz.Domain.Orders.Direction.PickUp && returnedToWarehouse);
				}
				PerformanceHelper.AddTimePoint("Обработали оборудование");
				routeListItem.BottlesReturned = routeListItem.IsDelivered()
					? (routeListItem.DriverBottlesReturned ?? routeListItem.Order.BottlesReturn) : 0;
				routeListItem.TotalCash = routeListItem.IsDelivered() &&
					routeListItem.Order.PaymentType == PaymentType.cash
					? routeListItem.Order.SumToReceive : 0;
				var bottleDepositPrice = NomenclatureRepository.GetBottleDeposit(UoW).GetPrice(routeListItem.Order.BottlesReturn);
				routeListItem.DepositsCollected = routeListItem.IsDelivered()
					? routeListItem.Order.GetExpectedBottlesDepositsCount() * bottleDepositPrice : 0;
				PerformanceHelper.AddTimePoint("Получили прайс");
				routeListItem.RecalculateWages();
				PerformanceHelper.AddTimePoint("Пересчет");
				PerformanceHelper.EndPointsGroup();
			}

//			PerformanceHelper.EndPointsGroup();
			Entity.ClosingFilled = true;
		}

		void Routelistdiscrepancyview_FineChanged(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		void OnRouteListItemActivated(object sender, RowActivatedArgs args)
		{
			var node = routeListAddressesView.GetSelectedRouteListItem();
			var dlg = new OrderReturnsView(node);
			TabParent.AddSlaveTab(this, dlg);
		}

		void OnRouteListItemChanged(object aList, int[] aIdx)
		{			
			var item = routeListAddressesView.Items[aIdx[0]];
			item.RecalculateWages();
			item.RecalculateTotalCash();
			if (!item.IsDelivered())
				foreach (var itm in item.Order.OrderItems) {
					itm.ActualCount = 0;
				}
			routelistdiscrepancyview.FindDiscrepancies(Entity.Addresses, allReturnsToWarehouse);
			OnItemsUpdated();
		}

		void OnOrderReturnsChanged(object aList, int[] aIdx)
		{
			foreach (var item in routeListAddressesView.Items)
			{
				var rli = item as RouteListItem;
				rli.RecalculateWages();
				rli.RecalculateTotalCash();
			}
			routelistdiscrepancyview.FindDiscrepancies(Entity.Addresses, allReturnsToWarehouse);
			OnItemsUpdated();
		}

		void OnItemsUpdated()
		{
			CalculateTotal();
			UpdateButtonState();
		}

		void UpdateButtonState()
		{
			buttonAccept.Sensitive = isConsistentWithUnloadDocument();
		}

		private bool buttonFineEditState = false;
		Nomenclature defaultBottle;

		void CalculateTotal()
		{
			var items = routeListAddressesView.Items.Where(item => item.IsDelivered());
			bottlesReturnedTotal = items.Sum(item => item.BottlesReturned);
			int fullBottlesTotal = items.SelectMany(item => item.Order.OrderItems).Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(item => item.ActualCount);
			decimal depositsCollectedTotal = items.Sum(item => item.DepositsCollected);
			decimal totalCollected = items.Sum(item => item.TotalCash);
			decimal driverWage = items.Sum(item => item.DriverWage);
			decimal forwarderWage = items.Sum(item => item.ForwarderWage);
			labelAddressCount.Text = String.Format("Адресов: {0}", Entity.AddressCount);
			labelPhone.Text = String.Format(
				"Сот. связь: {0} {1}",
				Entity.PhoneSum,
				CurrencyWorks.CurrencyShortName
			);
			labelFullBottles.Text = String.Format("Полных бутылей: {0}", fullBottlesTotal);
			labelEmptyBottles.Text = String.Format("Пустых бутылей: {0}", bottlesReturnedTotal);
			labelDeposits.Text = String.Format(
				"Залогов: {0} {1}",
				depositsCollectedTotal,
				CurrencyWorks.CurrencyShortName
			);
			labelCash.Text = String.Format(
				"Сдано по накладным: {0} {1}", 
				totalCollected,
				CurrencyWorks.CurrencyShortName
			);
			labelTotalCollected.Text = String.Format(
				"Итоговая сумма: {0} {1}", 
				totalCollected + depositsCollectedTotal,
				CurrencyWorks.CurrencyShortName
			);
			labelTotal.Markup = String.Format(
				"Итого сдано: <b>{0:F2}</b> {1}",
				Entity.MoneyToReturn,
				CurrencyWorks.CurrencyShortName
			);
			labelWage1.Markup = String.Format(
				"Зарплата водителя: <b>{0}</b> {2}" + "  " + "Зарплата экспедитора: <b>{1}</b> {2}", 
				driverWage,
				forwarderWage,
				CurrencyWorks.CurrencyShortName
			);
			labelEmptyBottlesFommula.Markup = String.Format("Тара: <b>{0}</b><sub>(выгружено на склад)</sub> - <b>{1}</b><sub>(по документам)</sub> =",
				bottlesReturnedToWarehouse,
				bottlesReturnedTotal
			);
			if (defaultBottle == null)
				defaultBottle = Repository.NomenclatureRepository.GetDefaultBottle(UoW);
			
			var bottleDifference = bottlesReturnedToWarehouse - bottlesReturnedTotal;
			var differenceAttributes = bottlesReturnedToWarehouse - bottlesReturnedTotal > 0 ? "background=\"#ff5555\"" : "";
			var bottleDifferenceFormat = "<span {1}><b>{0}</b><sub>(осталось)</sub></span>";
			checkUseBottleFine.Visible = bottleDifference < 0;
			if (bottleDifference != 0)
			{
				checkUseBottleFine.Label = String.Format("({0:C})", defaultBottle.SumOfDamage * (-bottleDifference));
			}
			labelBottleDifference.Markup = String.Format(bottleDifferenceFormat, bottleDifference, differenceAttributes);

			//Штрафы
			decimal totalSumOfDamage = 0;
			if (checkUseBottleFine.Active)
				totalSumOfDamage += defaultBottle.SumOfDamage * (-bottleDifference);
			totalSumOfDamage += routelistdiscrepancyview.Items.Where(x => x.UseFine).Sum(x => x.SumOfDamage);

			StringBuilder fineText = new StringBuilder();
			if (totalSumOfDamage != 0)
			{
				fineText.AppendLine(String.Format("Выб. ущерб: {0:C}", totalSumOfDamage));
			}
			if (Entity.BottleFine != null)
			{
				fineText.AppendLine(String.Format("Штраф: {0:C}", Entity.BottleFine.TotalMoney));
			}
			labelBottleFine.LabelProp = fineText.ToString().TrimEnd('\n');
			buttonBottleAddEditFine.Sensitive = totalSumOfDamage != 0;
			buttonBottleDelFine.Sensitive = Entity.BottleFine != null;
			if (buttonFineEditState != (Entity.BottleFine != null))
			{
				(buttonBottleAddEditFine.Image as Image).Pixbuf = new Gdk.Pixbuf(System.Reflection.Assembly.GetExecutingAssembly(),
					Entity.BottleFine != null ? "Vodovoz.icons.buttons.edit.png" : "Vodovoz.icons.buttons.add.png"
				);
				buttonFineEditState = Entity.BottleFine != null;
			}
		}

		protected bool isConsistentWithUnloadDocument()
		{
			var hasItemsDiscrepancies = routelistdiscrepancyview.Items.Any(discrepancy => discrepancy.Remainder != 0);
			bool hasFine = Entity.BottleFine != null;
			var items = routeListAddressesView.Items.Where(item => item.IsDelivered());
			int bottlesReturnedTotal = items.Sum(item => item.BottlesReturned);
			var hasTotalBottlesDiscrepancy = bottlesReturnedToWarehouse != bottlesReturnedTotal;
			return hasFine || (!hasTotalBottlesDiscrepancy && !hasItemsDiscrepancies) || Entity.DifferencesConfirmed;
		}

		public override bool Save()
		{
			Entity.UpdateFuelOperation(UoW);

			var valid = new QSValidator<RouteList>(Entity);
			if (valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			UoW.Save();
			return true;
		}

		protected void OnButtonAcceptClicked(object sender, EventArgs e)
		{
			var casher = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if (casher == null)
			{
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете закрыть МЛ, так как некого указывать в качестве кассира.");
				return;
			}

			if (!isConsistentWithUnloadDocument())
				return;

			var valid = new QSValidator<RouteList>(UoWGeneric.Root, 
				            new Dictionary<object, object>
				{
					{ "NewStatus", RouteListStatus.MileageCheck }
				});
			if (valid.RunDlgIfNotValid((Window)this.Toplevel))
				return;

			Entity.Cashier = casher;

			Income cashIncome = null;
			Expense cashExpense = null;

			var counterpartyMovementOperations 	= Entity.UpdateCounterpartyMovementOperations();
			var moneyMovementOperations 		= Entity.CreateMoneyMovementOperations(UoW, ref cashIncome, ref cashExpense);
			var bottleMovementOperations 		= Entity.CreateBottlesMovementOperation();
			var depositsOperations 				= Entity.CreateDepositOperations(UoW);

			counterpartyMovementOperations.ForEach(op => UoW.Save(op));
			bottleMovementOperations	  .ForEach(op => UoW.Save(op));
			depositsOperations			  .ForEach(op => UoW.Save(op));
			moneyMovementOperations		  .ForEach(op => UoW.Save(op));

			if (cashIncome  != null) UoW.Save(cashIncome);
			if (cashExpense != null) UoW.Save(cashExpense);

			if (Entity.WageOperation == null) {
				var wage = routeListAddressesView.Items
					.Where(item => item.IsDelivered()).Sum(item => item.DriverWage);
				Entity.CreateWageOperation(UoW, wage);
			}

			Entity.Confirm();

			UoW.Save();

			buttonAccept.Sensitive = false;
		}

		public List<ReturnsNode> GetReturnsToWarehouseByCategory(int routeListId, NomenclatureCategory[] categories)
		{
			List<ReturnsNode> result = new List<ReturnsNode>();		
			Nomenclature nomenclatureAlias = null;
			ReturnsNode resultAlias = null;
			Equipment equipmentAlias = null;
			CarUnloadDocumentItem carUnloadItemsAlias = null;
			WarehouseMovementOperation movementOperationAlias = null;

			var returnableItems = UoW.Session.QueryOver<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeListId)
				.JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where(Restrictions.IsNotNull(Projections.Property(() => movementOperationAlias.IncomingWarehouse)))
				.JoinAlias(() => movementOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => !nomenclatureAlias.Serial)		
				.Where(() => nomenclatureAlias.Category.IsIn(categories))
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => false).WithAlias(() => resultAlias.Trackable)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					.SelectSum(() => movementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
			                      )
				.TransformUsing(Transformers.AliasToBean<ReturnsNode>())
				.List<ReturnsNode>();

			var returnableEquipment = UoW.Session.QueryOver<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeListId)
				.JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where(Restrictions.IsNotNull(Projections.Property(() => movementOperationAlias.IncomingWarehouse)))
				.JoinAlias(() => movementOperationAlias.Equipment, () => equipmentAlias)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category.IsIn(categories))
				.SelectList(list => list
					.Select(() => equipmentAlias.Id).WithAlias(() => resultAlias.Id)				
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => nomenclatureAlias.Serial).WithAlias(() => resultAlias.Trackable)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					.SelectSum(() => movementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => nomenclatureAlias.Type).WithAlias(() => resultAlias.EquipmentType)
			                          )
				.TransformUsing(Transformers.AliasToBean<ReturnsNode>())
				.List<ReturnsNode>();

			result.AddRange(returnableItems);
			result.AddRange(returnableEquipment);
			return result;
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			var document = Vodovoz.Additions.Logistic.PrintRouteListHelper.GetRDLRouteList(UoW, Entity);
			this.TabParent.OpenTab(
				QSTDI.TdiTabBase.GenerateHashName<QSReport.ReportViewDlg>(),
				() => new QSReport.ReportViewDlg(document));
		}

		protected void OnButtonBottleAddEditFineClicked(object sender, EventArgs e)
		{
			string fineReason = "Недосдача";
			var bottleDifference = bottlesReturnedTotal - bottlesReturnedToWarehouse;
			var summ = defaultBottle.SumOfDamage * bottleDifference;
			summ += routelistdiscrepancyview.Items.Where(x => x.UseFine).Sum(x => x.SumOfDamage);
			var nomenclatures = routelistdiscrepancyview.Items.Where(x => x.UseFine)
				.ToDictionary(x => x.Nomenclature, x => -x.Remainder);
			if (checkUseBottleFine.Active)
				nomenclatures.Add(defaultBottle, bottleDifference);

			FineDlg fineDlg;
			if (Entity.BottleFine != null)
			{
				fineDlg = new FineDlg(Entity.BottleFine);

				Entity.BottleFine.UpdateNomenclature(nomenclatures);
				fineDlg.Entity.TotalMoney = summ;
				fineDlg.EntitySaved += FineDlgExist_EntitySaved;
			}
			else
			{
				fineDlg = new FineDlg(summ, Entity, fineReason, Entity.Date, Entity.Driver);
				fineDlg.Entity.AddNomenclature(nomenclatures);
				fineDlg.EntitySaved += FineDlgNew_EntitySaved;
			}
			fineDlg.EntitySaved += FineDlg_EntitySaved;
			TabParent.AddSlaveTab(this, fineDlg);
		}

		void FineDlg_EntitySaved (object sender, QSTDI.EntitySavedEventArgs e)
		{
			this.UoW.Save();
		}

		void FineDlgNew_EntitySaved(object sender, QSTDI.EntitySavedEventArgs e)
		{
			Entity.BottleFine = e.Entity as Fine;
			CalculateTotal();
			UpdateButtonState();
		}

		void FineDlgExist_EntitySaved(object sender, QSTDI.EntitySavedEventArgs e)
		{
			UoW.Session.Refresh(Entity.BottleFine);
			CalculateTotal();
		}

		protected void OnButtonBottleDelFineClicked(object sender, EventArgs e)
		{
			OrmMain.DeleteObject<Fine>(Entity.BottleFine.Id, UoW);
			Entity.BottleFine = null;
			CalculateTotal();
			UpdateButtonState();
		}

		protected void OnCheckUseBottleFineToggled(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		private void GetFuelInfo()
		{
			track = Repository.Logistics.TrackRepository.GetTrackForRouteList(UoW, Entity.Id);

			var fuelOtlayedOp = UoWGeneric.Root.FuelOutlayedOperation;
			var givedOp = Entity.FuelGivedDocument?.Operation;
			//Проверяем существование операций и исключаем их.
			var exclude = new List<int>();
			if (givedOp != null && givedOp.Id != 0)
			{
				exclude.Add(givedOp.Id);
			}
			if (fuelOtlayedOp != null && fuelOtlayedOp.Id != 0)
			{
				exclude.Add(fuelOtlayedOp.Id);
			}
			if (exclude.Count == 0)
				exclude = null;

			if (Entity.Car.FuelType != null)
			{
				Car car = Entity.Car;
				Employee driver = Entity.Driver;

				if (car.IsCompanyHavings)
					driver = null;
				else
					car = null;

				balanceBeforeOp = Repository.Operations.FuelRepository.GetFuelBalance(
					UoW, driver, car, Entity.Car.FuelType,
					null, exclude?.ToArray());
			}
		}

		private void UpdateFuelInfo()
		{
			var text = new List<string>();
			decimal spentFuel = (decimal)Entity.Car.FuelConsumption
			                    		/ 100 * Entity.ActualDistance;
			
			//Проверка существования трека и заполнения дистанции
			bool hasTrack = track?.Distance.HasValue ?? false;
			buttonGetDistFromTrack.Sensitive = hasTrack;

			if (hasTrack)
				text.Add(string.Format("Расстояние по треку: {0:F1} км.", track.Distance));
			
			if (Entity.Car.FuelType != null)
				text.Add(string.Format("Вид топлива: {0}", Entity.Car.FuelType.Name));
			else
				text.Add("Не указан вид топлива");
			
			if (Entity.FuelGivedDocument?.Operation != null || Entity.ActualDistance > 0)
				text.Add(string.Format("Остаток без выдачи {0:F2} л.", balanceBeforeOp));

			text.Add(string.Format("Израсходовано топлива: {0:F2} л. ({1:F2} л/100км)",
				spentFuel, (decimal)Entity.Car.FuelConsumption));

			if (Entity.FuelGivedDocument?.Operation != null)
			{
				text.Add(string.Format("Выдано {0:F2} литров",
						Entity.FuelGivedDocument.Operation.LitersGived));
			}

			if (Entity.Car.FuelType != null)
			{
				text.Add(string.Format("Текущий остаток топлива {0:F2} л.", balanceBeforeOp
					+ (Entity.FuelGivedDocument?.Operation.LitersGived ?? 0) - spentFuel));
			}

			buttonDeleteTicket.Sensitive = Entity.FuelGivedDocument != null;

			ytextviewFuelInfo.Buffer.Text = String.Join("\n", text);
		}

		void LoadDataFromFine()
		{
			if (Entity.BottleFine == null)
				return;

			if (defaultBottle == null)
				defaultBottle = NomenclatureRepository.GetDefaultBottle(UoW);

			foreach (var nom in Entity.BottleFine.Nomenclatures)
			{
				if (nom.Nomenclature.Id == defaultBottle.Id)
				{
					checkUseBottleFine.Active = true;
					continue;
				}

				var found = routelistdiscrepancyview.Items.FirstOrDefault(x => x.Nomenclature.Id == nom.Nomenclature.Id);
				if (found != null)
					found.UseFine = true;
			}
		}

		protected void OnYspinActualDistanceValueChanged(object sender, EventArgs e)
		{
			UpdateFuelInfo();
		}

		protected void OnButtonGetDistFromTrackClicked(object sender, EventArgs e)
		{
			var track = Repository.Logistics.TrackRepository.GetTrackForRouteList(UoW, Entity.Id);
			Entity.ActualDistance = (decimal)track.Distance.Value;
		}

		protected void OnButtonAddTicketClicked(object sender, EventArgs e)
		{
			var document = Entity.FuelGivedDocument;
			FuelDocumentDlg tab;

			if (document == null)
			{
				tab = new FuelDocumentDlg(UoWGeneric.Root);
			}
			else
			{
				tab = new FuelDocumentDlg(UoWGeneric.Root, document.Id);
			}
			tab.EntitySaved += FuelDoc_EntitySaved;
			TabParent.AddSlaveTab(this, tab);
		}

		void FuelDoc_EntitySaved(object sender, QSTDI.EntitySavedEventArgs e)
		{
			if (Entity.FuelGivedDocument == null)
			{
				Entity.FuelGivedDocument = e.Entity as FuelDocument;
			}
			else
			{
				UoW.Session.Refresh(Entity.FuelGivedDocument);
			}
			Save();
			UpdateFuelInfo();
			CalculateTotal();
		}

		protected void OnButtonDeleteTicketClicked(object sender, EventArgs e)
		{
			if (Entity.FuelGivedDocument != null)
			{
				UoW.Delete(Entity.FuelGivedDocument);
				Entity.FuelGivedDocument = null;
				this.HasChanges = true;
			}
			Save();
			UpdateFuelInfo();
			CalculateTotal();
		}

		protected void OnYcheckConfirmDifferencesToggled (object sender, EventArgs e)
		{
			UpdateButtonState();
		}

		protected void OnButtonNewFineClicked (object sender, EventArgs e)
		{
			this.TabParent.AddSlaveTab(
				this, new FineDlg (default(decimal), Entity)
			);
		}

		protected void OnYcheckHideCellsToggled (object sender, EventArgs e)
		{
			routeListAddressesView.ColumsVisibility = !ycheckHideCells.Active;
		}
	}

	public class ReturnsNode{
		public int Id{get;set;}
		public NomenclatureCategory NomenclatureCategory{ get; set; }
		public int NomenclatureId{ get; set; }
		public string Name{get;set;}
		public decimal Amount{ get; set;}
		public bool Trackable{ get; set; }
		public EquipmentType EquipmentType{get;set;}
		public string Serial{ get { 
				if (Trackable) {
					return Id > 0 ? Id.ToString () : "(не определен)";
				} else
					return String.Empty;
			}
		}
		public bool Returned {
			get {
				return Amount > 0;
			}
			set {
				Amount = value ? 1 : 0;
			}
		}
	}
}
