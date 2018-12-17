using System;
using System.Collections.Generic;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository.Cash;
using Vodovoz.ViewModel;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashExpenseSelfDeliveryDlg : EntityDialogBase<Expense>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public CashExpenseSelfDeliveryDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Expense>();
			Entity.Casher = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Casher == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
				FailInitialize = true;
				return;
			}
			Entity.Date = DateTime.Now;
			ConfigureDlg();
		}

		public CashExpenseSelfDeliveryDlg(Order order) : this()
		{
			Entity.Order = UoW.GetById<Order>(order.Id);
		}

		public CashExpenseSelfDeliveryDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Expense>(id);
			ConfigureDlg();
		}

		public CashExpenseSelfDeliveryDlg(Expense sub) : this(sub.Id) { }

		void ConfigureDlg()
		{
			TabName = "Расходный кассовый ордер самовывоза";

			Entity.TypeDocument = ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery;

			enumcomboOperation.ItemsEnum = typeof(ExpenseType);
			enumcomboOperation.Binding.AddBinding(Entity, s => s.TypeOperation, w => w.SelectedItem).InitializeFromSource();
			enumcomboOperation.Sensitive = false;
			Entity.TypeOperation = ExpenseType.ExpenseSelfDelivery;

			var filterOrders = new OrdersFilter(UoW);
			filterOrders.SetAndRefilterAtOnce(
				x => x.RestrictStatus = OrderStatus.WaitForPayment,
				x => x.RestrictSelfDelivery = true,
				x => x.RestrictWithoutSelfDelivery = false,
				x => x.RestrictHideService = true,
				x => x.RestrictOnlyService = false
			);
			yentryOrder.RepresentationModel = new OrdersVM(filterOrders);
			yentryOrder.Binding.AddBinding(Entity, x => x.Order, x => x.Subject).InitializeFromSource();

			var filterCasher = new EmployeeFilter(UoW);
			filterCasher.ShowFired = false;
			yentryCasher.RepresentationModel = new EmployeesVM(filterCasher);
			yentryCasher.Binding.AddBinding(Entity, s => s.Casher, w => w.Subject).InitializeFromSource();
			yentryCasher.Sensitive = false;

			ydateDocument.Binding.AddBinding(Entity, s => s.Date, w => w.Date).InitializeFromSource();

			OrmMain.GetObjectDescription<ExpenseCategory>().ObjectUpdated += OnExpenseCategoryUpdated;
			OnExpenseCategoryUpdated(null, null);
			comboExpense.Binding.AddBinding(Entity, s => s.ExpenseCategory, w => w.SelectedItem).InitializeFromSource();

			yspinMoney.Binding.AddBinding(Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource();

			ytextviewDescription.Binding.AddBinding(Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource();

		}

		void OnExpenseCategoryUpdated(object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedEventArgs e)
		{
			comboExpense.ItemsList = CategoryRepository.ExpenseSelfDeliveryCategories(UoW);
		}

		public override bool Save()
		{
			var validationContext = new Dictionary<object, object>();
			validationContext.Add("IsSelfDelivery", true);
			var valid = new QSValidator<Expense>(UoWGeneric.Root, validationContext);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.AcceptSelfDeliveryPaid();

			logger.Info("Сохраняем расходный ордер...");
			UoWGeneric.Save();
			logger.Info("Ok");
			return true;

		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Expense), "квитанции"))
				Save();

			var reportInfo = new QS.Report.ReportInfo {
				Title = String.Format("Квитанция №{0} от {1:d}", Entity.Id, Entity.Date),
				Identifier = "Cash.Expense",
				Parameters = new Dictionary<string, object> {
					{ "id",  Entity.Id }
				}
			};

			var report = new QSReport.ReportViewDlg(reportInfo);
			TabParent.AddTab(report, this, false);
		}

		protected void OnYentryOrderChanged(object sender, EventArgs e)
		{
			Entity.FillFromOrder(UoW);
		}
	}
}
