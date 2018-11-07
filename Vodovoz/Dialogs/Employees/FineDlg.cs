using System;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalViewers;
using Vodovoz.Repository;
using Vodovoz.Repository.Logistics;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class FineDlg : QS.Dialog.Gtk.EntityDialogBase<Fine>
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public FineDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Fine> ();
			ConfigureDlg ();
		}

		/// <summary>
		/// Создаем новый диалог штрафа с уже заполненными сотрудниками.
		/// </summary>
		public FineDlg(decimal money, params Employee[] employees) : this()
		{
			employees.ToList().ForEach(Entity.AddItem);
			Entity.TotalMoney = money;
			Entity.DivideAtAll();
		}

		public FineDlg(string reasonString) : this()
		{
			Entity.FineReasonString = reasonString;
		}

		public FineDlg(decimal money, RouteList routeList, string reasonString, DateTime date, params Employee[] employees) : this()
		{
			Entity.Fill(money, routeList, reasonString, date, employees);
		}

		public FineDlg(decimal money, RouteList routeList) : this(money, routeList.Driver)
		{
			Entity.RouteList = routeList;
			Entity.Date = routeList.Date;
		}

		public FineDlg(UndeliveredOrder undeliveredOrder) : this()
		{
			Entity.UndeliveredOrder = undeliveredOrder;
			var RouteList = RouteListItemRepository.GetRouteListItemForOrder(UoW, undeliveredOrder.OldOrder)?.RouteList;
			Entity.RouteList = RouteList;
		}

		public FineDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Fine> (id);
			ConfigureDlg ();
		}

		public FineDlg (Fine sub) : this (sub.Id)
		{
		}

		void ConfigureDlg ()
		{
			enumFineType.ItemsEnum = typeof(FineTypes);
			enumFineType.Binding.AddBinding(Entity, s => s.FineType, w => w.SelectedItem).InitializeFromSource();

			yspinLiters.Binding.AddBinding(Entity, s => s.LitersOverspending, w => w.ValueAsDecimal);

			ylabelDate.Binding.AddFuncBinding(Entity, e => e.Date.ToString("D"), w => w.LabelProp).InitializeFromSource();
			yspinMoney.Binding.AddBinding(Entity, e => e.TotalMoney, w => w.ValueAsDecimal).InitializeFromSource();
			yentryFineReasonString.Binding.AddBinding(Entity, e => e.FineReasonString, w => w.Text).InitializeFromSource();
			fineitemsview1.FineUoW = UoWGeneric;

			var filterRouteList = new RouteListsFilter(UoW);
			filterRouteList.SetFilterDates(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1));
			yentryreferenceRouteList.RepresentationModel = new ViewModel.RouteListsVM(filterRouteList);
			yentryreferenceRouteList.Binding.AddBinding(Entity, e => e.RouteList, w => w.Subject).InitializeFromSource();
			yentryreferenceRouteList.CanEditReference = QSMain.User.Permissions["can_delete"];

			Entity.ObservableItems.ListChanged += ObservableItems_ListChanged;
			var filterAuthor = new EmployeeFilter(UoW);
			yentryAuthor.RepresentationModel = new EmployeesVM(filterAuthor);
            yentryAuthor.Binding.AddBinding(Entity, e => e.Author, w => w.Subject).InitializeFromSource();
			
            UpdateControlsState();
			ShowLiters();
		}

		void ObservableItems_ListChanged(object aList)
		{
			enumFineType.Sensitive = !(Entity.ObservableItems.Count() > 1);
		}

		public override bool Save ()
		{
            Employee author;
            if (!GetAuthor(out author)) return false;

            if (Entity.Author == null)
            {
                Entity.Author = author;
            }
			var valid = new QSValidation.QSValidator<Fine> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			Entity.UpdateWageOperations(UoW);
			Entity.UpdateFuelOperations(UoW);

			logger.Info ("Сохраняем штраф...");
			UoWGeneric.Save ();
			logger.Info ("Ok.");
			return true;
		}

		protected void OnButtonDivideAtAllClicked(object sender, EventArgs e)
		{
			Entity.DivideAtAll();
		}

		protected void OnButtonGetReasonFromTemplateClicked (object sender, EventArgs e)
		{
			OrmReference SelectDialog = new OrmReference (typeof(FineTemplate), UoWGeneric);
			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ButtonMode = ReferenceButtonMode.CanAdd;
			SelectDialog.ObjectSelected += (s, ea) => {
				if(ea.Subject != null && Entity.FineType != FineTypes.FuelOverspending) {
					UoWGeneric.Root.FineReasonString = (ea.Subject as FineTemplate).Reason;
					UoWGeneric.Root.TotalMoney = (ea.Subject as FineTemplate).FineMoney;
				}
			};
			TabParent.AddSlaveTab (this, SelectDialog);
		}

		protected void OnEnumFineTypeChangedByUser(object sender, EventArgs e)
		{
			UpdateControlsState();
		}

		/// <summary>
		/// Обновляет состояние контролов по выбранному типу штрафа
		/// </summary>
		private void UpdateControlsState()
		{
			switch(Entity.FineType) {
				case FineTypes.Standart:
					fineitemsview1.IsFuelOverspending = false;
					buttonDivideAtAll.Visible = true;
					yspinMoney.IsEditable = true;
					yspinMoney.Sensitive = true;
					labelOverspending.Visible = false;
					yspinLiters.Visible = false;
					labelRequestRouteList.Visible = false;
					buttonGetReasonFromTemplate.Visible = true;
					break;
				case FineTypes.FuelOverspending:
					fineitemsview1.IsFuelOverspending = true;
					buttonDivideAtAll.Visible = false;
					yspinMoney.Sensitive = false;
					labelOverspending.Visible = true;
					yspinLiters.Visible = true;
					buttonGetReasonFromTemplate.Visible = false;
					yentryFineReasonString.Text = Entity.FineType.GetEnumTitle();
					labelRequestRouteList.Visible = yentryreferenceRouteList.Subject == null;
					if(Entity.RouteList != null) {
						ClearItems(Entity.RouteList.Driver);
					}else {
						ClearItems();
					}
					break;
				default:
					break;
			}

			btnShowUndelivery.Visible = Entity.UndeliveredOrder != null;
		}

		private void ShowLiters()
		{
			if(Entity.FineType == FineTypes.FuelOverspending && Entity.ObservableItems.Count() > 0){
				Entity.LitersOverspending = Entity.ObservableItems[0].LitersOverspending;
			}
		}

		private void CalculateMoneyFromLiters()
		{
			if(Entity.ObservableItems.Count() > 1) {
				throw new Exception("При типе штрафа \"Перерасход топлива\" недопустимо наличие более одного сотрудника в списке.");
			}
			if(yentryreferenceRouteList.Subject != null) {
				decimal fuelCost = (yentryreferenceRouteList.Subject as RouteList).Car.FuelType.Cost;
				Entity.TotalMoney = Math.Round(Entity.LitersOverspending * fuelCost, 0, MidpointRounding.ToEven);
				var item = Entity.ObservableItems.FirstOrDefault();
				if(item != null) {
					item.Money = Entity.TotalMoney;
					item.LitersOverspending = Entity.LitersOverspending;
				}
			}
		}

		protected void OnYspinLitersValueChanged(object sender, EventArgs e)
		{
			UpdateControlsState();
			CalculateMoneyFromLiters();

		}

		private void ClearItems(Employee driver = null)
		{
			FineItem item = null;
			if(driver != null) {
				item = Entity.ObservableItems.Where(x => x.Employee == driver).FirstOrDefault();
			}
			Entity.ObservableItems.Clear();
			if(driver != null) {
				if(item != null) {
					Entity.ObservableItems.Add(item);
				}else {
					Entity.AddItem(driver);
				}
			}
		}

		protected void OnYentryreferenceRouteListChangedByUser(object sender, EventArgs e)
		{
			if(Entity.FineType == FineTypes.FuelOverspending && Entity.RouteList != null) {
				ClearItems(Entity.RouteList.Driver);
				CalculateMoneyFromLiters();
			}
		}

        private bool GetAuthor(out Employee cashier)
        {
            cashier = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
            if (cashier == null)
            {
                MessageDialogWorks.RunErrorDialog(
                    "Ваш пользователь не привязан к действующему сотруднику.");
                return false;
            }
            return true;
        }

		protected void OnBtnShowUndeliveryClicked(object sender, EventArgs e)
		{
			UndeliveriesView dlg = new UndeliveriesView();
			dlg.HideFilterAndControls();
			dlg.GetUndeliveryFilter.SetAndRefilterAtOnce(
				x => x.RestrictOldOrder = Entity.UndeliveredOrder.OldOrder,
				x => x.RestrictOldOrderStartDate = Entity.UndeliveredOrder.OldOrder.DeliveryDate,
				x => x.RestrictOldOrderEndDate = Entity.UndeliveredOrder.OldOrder.DeliveryDate,
				x => x.RestrictUndeliveryStatus = Entity.UndeliveredOrder.UndeliveryStatus
			);
			TabParent.AddSlaveTab(this, dlg);
		}
	}
}