using System;
using System.Collections.Generic;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModel;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashIncomeSelfDeliveryDlg : EntityDialogBase<Income>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public CashIncomeSelfDeliveryDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Income>();
			Entity.Casher = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Casher == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
				FailInitialize = true;
				return;
			}
			Entity.Date = DateTime.Now;
			ConfigureDlg();
		}

		public CashIncomeSelfDeliveryDlg(Order forOrder) : this()
		{
			Entity.Order = UoW.GetById<Order>(forOrder.Id);
		}

		public CashIncomeSelfDeliveryDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income>(id);
			ConfigureDlg();
		}

		public CashIncomeSelfDeliveryDlg(Income sub) : this(sub.Id) { }

		void ConfigureDlg()
		{
			TabName = "Приходный кассовый ордер самовывоза";

			Entity.TypeDocument = IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery;

			enumcomboOperation.ItemsEnum = typeof(IncomeType);
			enumcomboOperation.Binding.AddBinding(Entity, s => s.TypeOperation, w => w.SelectedItem).InitializeFromSource();
			enumcomboOperation.Sensitive = false;
			Entity.TypeOperation = IncomeType.Payment;

			var filterCasher = new EmployeeFilter(UoW);
			filterCasher.ShowFired = false;
			yentryCasher.RepresentationModel = new ViewModel.EmployeesVM(filterCasher);
			yentryCasher.Binding.AddBinding(Entity, s => s.Casher, w => w.Subject).InitializeFromSource();
			yentryCasher.Sensitive = false;

			var filterOrders = new OrdersFilter(UoW);
			filterOrders.SetAndRefilterAtOnce(
				x => x.RestrictStatus = OrderStatus.WaitForPayment,
				x => x.AllowPaymentTypes = new PaymentType[] { PaymentType.cash, PaymentType.BeveragesWorld },
				x => x.RestrictSelfDelivery = true,
				x => x.RestrictWithoutSelfDelivery = false,
				x => x.RestrictHideService = true,
				x => x.RestrictOnlyService = false
			);
			yentryOrder.RepresentationModel = new OrdersVM(filterOrders);
			yentryOrder.Binding.AddBinding(Entity, x => x.Order, x => x.Subject).InitializeFromSource();

			ydateDocument.Binding.AddBinding(Entity, s => s.Date, w => w.Date).InitializeFromSource();

			OrmMain.GetObjectDescription<IncomeCategory>().ObjectUpdated += OnIncomeCategoryUpdated;
			OnIncomeCategoryUpdated(null, null);
			comboCategory.Binding.AddBinding(Entity, s => s.IncomeCategory, w => w.SelectedItem).InitializeFromSource();

			yspinMoney.Binding.AddBinding(Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource();

			ytextviewDescription.Binding.AddBinding(Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource();
		}

		void OnIncomeCategoryUpdated(object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedEventArgs e)
		{
			comboCategory.ItemsList = Repository.Cash.CategoryRepository.SelfDeliveryIncomeCategories(UoW);
		}

		public override bool Save()
		{
			var validationContext = new Dictionary<object, object>();
			validationContext.Add("IsSelfDelivery", true);
			var valid = new QSValidator<Income>(UoWGeneric.Root, validationContext);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.AcceptSelfDeliveryPaid();

			logger.Info("Сохраняем Приходный ордер самовывоза...");
			UoWGeneric.Save();
			OrmMain.NotifyObjectUpdated(new object[] { Entity.Order });
			logger.Info("Ok");
			return true;
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Income), "квитанции"))
				Save();

			var reportInfo = new QS.Report.ReportInfo {
				Title = String.Format("Квитанция №{0} от {1:d}", Entity.Id, Entity.Date),
				Identifier = "Cash.ReturnTicket",
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
