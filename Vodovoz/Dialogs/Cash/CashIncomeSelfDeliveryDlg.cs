using System;
using System.Collections.Generic;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QSOrmProject;
using QS.Validation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.ViewModel;
using QS.Services;
using Vodovoz.EntityRepositories;
using Vodovoz.Tools.CallTasks;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Core.DataService;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Documents;
using Vodovoz.Parameters;
using Vodovoz.PermissionExtensions;
using Vodovoz.Repository.Cash;
using Vodovoz.Tools;

namespace Vodovoz.Dialogs.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashIncomeSelfDeliveryDlg : EntityDialogBase<Income>
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private bool canEdit = true;
		private readonly bool canCreate;
		private readonly bool canEditRectroactively;
		private SelfDeliveryCashOrganisationDistributor selfDeliveryCashOrganisationDistributor = 
			new SelfDeliveryCashOrganisationDistributor(
				new CashDistributionCommonOrganisationProvider(new OrganisationParametersProvider()),
				new SelfDeliveryCashDistributionDocumentRepository(),
				OrderSingletonRepository.GetInstance());

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						OrderSingletonRepository.GetInstance(),
						EmployeeSingletonRepository.GetInstance(),
						new BaseParametersProvider(),
						ServicesConfig.CommonServices.UserService,
						SingletonErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		public CashIncomeSelfDeliveryDlg(IPermissionService permissionService)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Income>();
			Entity.Casher = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Casher == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать кассовые документы, так как некого указывать в качестве кассира.");
				FailInitialize = true;
				return;
			}

			var userPermission = permissionService.ValidateUserPermission(typeof(Income), UserSingletonRepository.GetInstance().GetCurrentUser(UoW).Id);
			canCreate = userPermission.CanCreate;
			if(!userPermission.CanCreate) {
				MessageDialogHelper.RunErrorDialog("Отсутствуют права на создание приходного ордера");
				FailInitialize = true;
				return;
			}

			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(Income))) {

				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}
			accessfilteredsubdivisionselectorwidget.OnSelected += Accessfilteredsubdivisionselectorwidget_OnSelected;

			Entity.Date = DateTime.Now;
			ConfigureDlg();
		}

		public CashIncomeSelfDeliveryDlg(Order forOrder, IPermissionService permissionService) : this(permissionService)
		{
			Entity.Order = UoW.GetById<Order>(forOrder.Id);
		}

		public CashIncomeSelfDeliveryDlg(int id, IPermissionService permissionService)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Income>(id);

			if(!accessfilteredsubdivisionselectorwidget.Configure(UoW, false, typeof(Income))) {

				MessageDialogHelper.RunErrorDialog(accessfilteredsubdivisionselectorwidget.ValidationErrorMessage);
				FailInitialize = true;
				return;
			}
			accessfilteredsubdivisionselectorwidget.OnSelected += Accessfilteredsubdivisionselectorwidget_OnSelected;

			var userPermission = permissionService.ValidateUserPermission(typeof(Income), UserSingletonRepository.GetInstance().GetCurrentUser(UoW).Id);
			if(!userPermission.CanRead) {
				MessageDialogHelper.RunErrorDialog("Отсутствуют права на просмотр приходного ордера");
				FailInitialize = true;
				return;
			}
			canEdit = userPermission.CanUpdate;

			var permmissionValidator = new EntityExtendedPermissionValidator(PermissionExtensionSingletonStore.GetInstance(), EmployeeSingletonRepository.GetInstance());
			canEditRectroactively = permmissionValidator.Validate(typeof(Income), UserSingletonRepository.GetInstance().GetCurrentUser(UoW).Id, nameof(RetroactivelyClosePermission));

			ConfigureDlg();
		}

		public CashIncomeSelfDeliveryDlg(Income sub, IPermissionService permissionService) : this(sub.Id, permissionService) { }

		private bool CanEdit => (UoW.IsNew && canCreate) ||
		                        (canEdit && Entity.Date.Date == DateTime.Now.Date) ||
		                        canEditRectroactively;
		
		void ConfigureDlg()
		{
			TabName = "Приходный кассовый ордер самовывоза";

			Entity.TypeDocument = IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery;

			permissioncommentview.UoW = UoW;
			permissioncommentview.Title = "Комментарий по проверке закрытия МЛ: ";
			permissioncommentview.Comment = Entity.CashierReviewComment;
			permissioncommentview.PermissionName = "can_edit_cashier_review_comment";
			permissioncommentview.Comment = Entity.CashierReviewComment;
			permissioncommentview.CommentChanged += (comment) => Entity.CashierReviewComment = comment;

			enumcomboOperation.ItemsEnum = typeof(IncomeType);
			enumcomboOperation.Binding.AddBinding(Entity, s => s.TypeOperation, w => w.SelectedItem).InitializeFromSource();
			enumcomboOperation.Sensitive = false;
			Entity.TypeOperation = IncomeType.Payment;

			var filterCasher = new EmployeeFilterViewModel();
			filterCasher.Status = Domain.Employees.EmployeeStatus.IsWorking;
			yentryCasher.RepresentationModel = new EmployeesVM(filterCasher);
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

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<IncomeCategory>(
				s => comboCategory.ItemsList = CategoryRepository.SelfDeliveryIncomeCategories(UoW)
			);
			comboCategory.ItemsList = CategoryRepository.SelfDeliveryIncomeCategories(UoW);
			comboCategory.Binding.AddBinding(Entity, s => s.IncomeCategory, w => w.SelectedItem).InitializeFromSource();

			yspinMoney.Binding.AddBinding(Entity, s => s.Money, w => w.ValueAsDecimal).InitializeFromSource();

			ytextviewDescription.Binding.AddBinding(Entity, s => s.Description, w => w.Buffer.Text).InitializeFromSource();

			if(!CanEdit) {
				table1.Sensitive = false;
				ytextviewDescription.Editable = false;
				buttonSave.Sensitive = false;
				accessfilteredsubdivisionselectorwidget.Sensitive = false;
			}

			UpdateSubdivision();
		}

		void Accessfilteredsubdivisionselectorwidget_OnSelected(object sender, EventArgs e)
		{
			UpdateSubdivision();
		}

		private void UpdateSubdivision()
		{
			Entity.RelatedToSubdivision = accessfilteredsubdivisionselectorwidget.SelectedSubdivision;
		}

		public override bool Save()
		{
			var validationContext = new Dictionary<object, object> {{"IsSelfDelivery", true}};
			var valid = new QSValidator<Income>(UoWGeneric.Root, validationContext);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			Entity.AcceptSelfDeliveryPaid(CallTaskWorker);

			if (UoW.IsNew) {
				selfDeliveryCashOrganisationDistributor.DistributeIncomeCash(UoW, Entity.Order, Entity);
			}
			else { 
				selfDeliveryCashOrganisationDistributor.UpdateRecords(UoW, Entity.Order, Entity,
					EmployeeSingletonRepository.GetInstance().GetEmployeeForCurrentUser(UoW));
			}

			logger.Info("Сохраняем Приходный ордер самовывоза...");
			UoWGeneric.Save();
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
			if (Entity.Order != null)
			{
				Entity.FillFromOrder(UoW);
			}
		}
		
		public override void Destroy()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			base.Destroy();
		}
	}
}
