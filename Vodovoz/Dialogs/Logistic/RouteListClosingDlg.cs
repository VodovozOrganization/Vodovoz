using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Gtk;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository;
using Vodovoz.Repository.Logistics;

namespace Vodovoz
{
	public partial class RouteListClosingDlg : OrmGtkDialogBase<RouteList>
	{
		#region поля

		private static Logger logger = LogManager.GetCurrentClassLogger();

		private Track 	 track 				= null;
		private decimal  balanceBeforeOp 	= default(decimal);
		private bool 	 editing 			= QSMain.User.Permissions ["money_manage"];
		private Employee previousForwarder  = null;

		List<RouteListRepository.ReturnsNode> allReturnsToWarehouse;
		int bottlesReturnedToWarehouse;
		int bottlesReturnedTotal;

		enum RouteListActions {
			[Display (Name = "Новый штраф")]
			CreateNewFine,
			[Display (Name = "Перенести разгрузку в другой МЛ")]
			TransferReceptionToAnotherRL,
			[Display (Name = "Перенести разгрузку в этот МЛ")]
			TransferReceptionToThisRL,
			[Display (Name = "Перенести адреса в этот МЛ")]
			TransferAddressesToThisRL,
			[Display (Name = "Перенести адреса из этого МЛ")]
			TransferAddressesToAnotherRL

		}

		#endregion

		#region Конструкторы и конфигурирование диалога

		public RouteListClosingDlg(RouteList routeList) : this(routeList.Id){}

		public RouteListClosingDlg(int routeListId)
		{
			this.Build();

			PerformanceHelper.StartMeasurement();

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
			yspinActualDistance.Sensitive = editing;

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = editing;

			ycheckConfirmDifferences.Binding.AddBinding(Entity, e => e.DifferencesConfirmed, w => w.Active).InitializeFromSource();
			ycheckConfirmDifferences.Sensitive = editing;

			ytextClosingComment.Binding.AddBinding(Entity, e => e.ClosingComment, w => w.Buffer.Text).InitializeFromSource();

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
			routeListAddressesView.IsEditing = editing;
			routeListAddressesView.ColumsVisibility = !ycheckHideCells.Active;
			PerformanceHelper.AddTimePoint("заполнили список адресов");
			ReloadReturnedToWarehouse();
			var returnableOrderItems = routeListAddressesView.Items
				.Where(address => address.IsDelivered())
				.SelectMany(address => address.Order.OrderItems)
				.Where(orderItem => !orderItem.Nomenclature.Serial)
				.Where(orderItem => Nomenclature.GetCategoriesForShipment().Any(nom => nom == orderItem.Nomenclature.Category));
			foreach (var item in returnableOrderItems)
			{
				if (allReturnsToWarehouse.All(r => r.NomenclatureId != item.Nomenclature.Id))
					allReturnsToWarehouse.Add(new RouteListRepository.ReturnsNode {
							Name = item.Nomenclature.Name,
							Trackable = item.Nomenclature.Serial,
							NomenclatureId = item.Nomenclature.Id,
							Amount = 0
						});
			}
			PerformanceHelper.AddTimePoint("Получили возврат на склад");
			//FIXME Убрать из этого места первоначальное заполнение. Сейчас оно вызывается при переводе статуса на сдачу. После того как не нормально не переведенных в закрытие маршрутников, тут заполение можно убрать.
			if(!Entity.ClosingFilled)
				Entity.FirstFillClosing();

			PerformanceHelper.AddTimePoint("Закончено первоначальное заполнение");

			hbox6.Remove(vboxHidenPanel);
			rightsidepanel1.Panel = vboxHidenPanel;

			routelistdiscrepancyview.FindDiscrepancies(Entity.Addresses, allReturnsToWarehouse);
			routelistdiscrepancyview.FineChanged += Routelistdiscrepancyview_FineChanged;
			routelistdiscrepancyview.Sensitive = editing;
			PerformanceHelper.AddTimePoint("Заполнили расхождения");

			buttonAddTicket.Sensitive = Entity.Car?.FuelType?.Cost != null && Entity.Driver != null;

			enummenuRLActions.ItemsEnum = typeof(RouteListActions);
			enummenuRLActions.EnumItemClicked += EnummenuRLActions_EnumItemClicked;

			LoadDataFromFine();
			OnItemsUpdated();
			PerformanceHelper.AddTimePoint("Загрузка штрафов");
			GetFuelInfo();
			UpdateFuelInfo();
			PerformanceHelper.AddTimePoint("Загрузка бензина");

			PerformanceHelper.Main.PrintAllPoints(logger);

			//Подписки на обновления
			OrmMain.GetObjectDescription<CarUnloadDocument>().ObjectUpdatedGeneric += OnCalUnloadUpdated;
		}

		void OnCalUnloadUpdated (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<CarUnloadDocument> e)
		{
			if (e.UpdatedSubjects.Any(x => x.RouteList.Id == Entity.Id))
				ReloadDiscrepancies();
		}

		#endregion

		#region Методы

		void ReferenceForwarder_Changed (object sender, EventArgs e)
		{
			var newForwarder = Entity.Forwarder;

			if ((previousForwarder == null && newForwarder != null)
			 || (previousForwarder != null && newForwarder == null))
				foreach (var item in Entity.Addresses)
					item.RecalculateWages();
			
			previousForwarder = Entity.Forwarder;
		}

		void EnummenuRLActions_EnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			switch ((RouteListActions)e.ItemEnum)
			{
				case RouteListActions.CreateNewFine:
					this.TabParent.AddSlaveTab(
						this, new FineDlg(default(decimal), Entity)
					);
					break;
				case RouteListActions.TransferReceptionToAnotherRL:
					this.TabParent.AddSlaveTab(
						this, new TransferGoodsBetweenRLDlg(Entity, TransferGoodsBetweenRLDlg.OpenParameter.Sender)
					);
					break;
				case RouteListActions.TransferReceptionToThisRL:
					this.TabParent.AddSlaveTab(
						this, new TransferGoodsBetweenRLDlg(Entity, TransferGoodsBetweenRLDlg.OpenParameter.Receiver)
					);
					break;
				case RouteListActions.TransferAddressesToThisRL:
					if (UoW.HasChanges) {
						if(MessageDialogWorks.RunQuestionDialog ("Необходимо сохранить документ.\nСохранить?"))
							this.Save ();
						else
							return;
					}
                    this.TabParent.AddSlaveTab(
                        this, new RouteListAddressesTransferringDlg (Entity, RouteListAddressesTransferringDlg.OpenParameter.Sender)
					);
					break;
				case RouteListActions.TransferAddressesToAnotherRL:
					if (UoW.HasChanges) {
						if(!MessageDialogWorks.RunQuestionDialog ("Необходимо сохранить документ.\nСохранить?"))
							 this.Save ();
						else
							return;
					}
                    this.TabParent.AddSlaveTab(
                        this, new RouteListAddressesTransferringDlg (Entity, RouteListAddressesTransferringDlg.OpenParameter.Receiver)
					);
					break;
				default:
					break;
			}
		}


		void Routelistdiscrepancyview_FineChanged(object sender, EventArgs e)
		{
			CalculateTotal();
		}

		void OnRouteListItemActivated(object sender, RowActivatedArgs args)
		{
			var node = routeListAddressesView.GetSelectedRouteListItem();
			var dlg = new OrderReturnsView(node);
			dlg.UoW = UoW;
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
			labelAddressCount.Text = String.Format("Адресов: {0}", Entity.UniqueAddressCount);
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
				totalCollected + depositsCollectedTotal,
				CurrencyWorks.CurrencyShortName
			);
			labelTotalCollected.Text = String.Format(
				"Итоговая сумма: {0} {1}", 
				totalCollected + depositsCollectedTotal - Entity.PhoneSum,
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

			Entity.UpdateFuelOperation(UoW);

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


		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if (!MessageDialogWorks.RunQuestionDialog("Перед печатью необходимо сохранить документ.\nСохранить?"))
				return;
			UoW.Save();

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
			TabParent.AddSlaveTab(this, fineDlg);
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
			buttonGetDistFromTrack.Sensitive = hasTrack && editing;

			if (hasTrack)
				text.Add(string.Format("Расстояние по треку: {0:F1} км.", track.TotalDistance));
			
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
			Entity.ActualDistance = (decimal)track.TotalDistance.Value;
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

		protected void OnYcheckHideCellsToggled (object sender, EventArgs e)
		{
			routeListAddressesView.ColumsVisibility = !ycheckHideCells.Active;
		}

		protected void OnButtonReturnedRefreshClicked(object sender, EventArgs e)
		{
			ReloadDiscrepancies();
		}

		private void ReloadDiscrepancies()
		{
			ReloadReturnedToWarehouse();
			routelistdiscrepancyview.FindDiscrepancies(Entity.Addresses, allReturnsToWarehouse);
			CalculateTotal();
		}

		private void ReloadReturnedToWarehouse()
		{
			allReturnsToWarehouse = RouteListRepository.GetReturnsToWarehouse(UoW, Entity.Id, Nomenclature.GetCategoriesForShipment());
			bottlesReturnedToWarehouse = (int)RouteListRepository.GetReturnsToWarehouse(UoW, Entity.Id, new []{ NomenclatureCategory.bottle })
				.Sum(item => item.Amount);
		}

		public override void Destroy()
		{
			OrmMain.GetObjectDescription<CarUnloadDocument>().ObjectUpdatedGeneric -= OnCalUnloadUpdated;
			base.Destroy();
		}

		#endregion
	}

}
