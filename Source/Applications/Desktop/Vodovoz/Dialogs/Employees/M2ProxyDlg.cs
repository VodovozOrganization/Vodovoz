using Autofac;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.DocTemplates;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class M2ProxyDlg : QS.Dialog.Gtk.EntityDialogBase<M2ProxyDocument>, IEditableDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();

		private IDocTemplateRepository _docTemplateRepository;

		private List<OrderEquipment> equipmentList;
		public bool IsEditable { get; set; } = true;

		public M2ProxyDlg()
		{
			ResolveDependancies();
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<M2ProxyDocument>();
			TabName = "Новая доверенность М-2";
			ConfigureDlg();
		}

		public M2ProxyDlg(M2ProxyDocument sub) : this(sub.Id) { }

		public M2ProxyDlg(int id)
		{
			ResolveDependancies();
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<M2ProxyDocument>(id);
			TabName = "Изменение доверенности М-2";
			ConfigureDlg();
		}

		public M2ProxyDlg(Order order)
		{
			ResolveDependancies();
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<M2ProxyDocument>();
			TabName = "Новая доверенность М-2";
			Entity.Order = order;

			ConfigureDlg();
		}

		private void ResolveDependancies()
		{
			_docTemplateRepository = _lifetimeScope.Resolve<IDocTemplateRepository>();
		}

		void ConfigureDlg()
		{
			if(Entity.EmployeeDocument == null && Entity.Employee != null)
				GetDocument();

			ylabelNumber.Binding.AddBinding(Entity, x => x.Title, x => x.LabelProp).InitializeFromSource();
			var orderFactory = _lifetimeScope.Resolve<IOrderSelectorFactory>();
			evmeOrder.SetEntityAutocompleteSelectorFactory(orderFactory.CreateOrderAutocompleteSelectorFactory());
			evmeOrder.Binding.AddBinding(Entity, x => x.Order, x => x.Subject).InitializeFromSource();
			evmeOrder.Changed += (sender, e) => {
				FillForOrder();
			};
			evmeOrder.CanEditReference = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete");


			var organizationViewModel = new LegacyEEVMBuilderFactory<M2ProxyDocument>(
				this,
				Entity,
				UoW,
				Startup.MainWin.NavigationManager,
				_lifetimeScope)
				.ForProperty(x => x.Organization)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();

			organizationViewModel.Changed += (sender, e) => UpdateStates();

			entryOrganization.ViewModel = organizationViewModel;

			FillForOrder();

			yDPDatesRange.Binding.AddBinding(Entity, x => x.Date, x => x.StartDate).InitializeFromSource();
			yDPDatesRange.Binding.AddBinding(Entity, x => x.ExpirationDate, x => x.EndDate).InitializeFromSource();

			var employeeFactory = _lifetimeScope.Resolve<IEmployeeJournalFactory>();
			evmeEmployee.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.Binding.AddBinding(Entity, x => x.Employee, x => x.Subject).InitializeFromSource();

			var supplierFactory = _lifetimeScope.Resolve<ICounterpartyJournalFactory>();
			evmeSupplier.SetEntityAutocompleteSelectorFactory(supplierFactory.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope));
			evmeSupplier.Binding.AddBinding(Entity, x => x.Supplier, x => x.Subject).InitializeFromSource();

			yETicketNr.Binding.AddBinding(Entity, x => x.TicketNumber, w => w.Text).InitializeFromSource();

			yDTicketDate.Binding.AddBinding(Entity, x => x.TicketDate, x => x.DateOrNull).InitializeFromSource();

			RefreshParserRootObject();

			templatewidget.CanRevertCommon = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_common_additionalagreement");
			templatewidget.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();

			templatewidget.BeforeOpen += Templatewidget_BeforeOpen;

			yTWEquipment.ColumnsConfig = ColumnsConfigFactory.Create<OrderEquipment>()
				.AddColumn("Наименование").AddTextRenderer(node => node.FullNameString)
				.AddColumn("Направление").AddTextRenderer(node => node.DirectionString)
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Count).WidthChars(10)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
				.AddColumn("")
				.Finish();

			UpdateStates();
		}

		void GetDocument()
		{
			var doc = Entity.Employee.GetMainDocuments();
			if(doc.Any())
				Entity.EmployeeDocument = doc[0];
		}

		void FillForOrder()
		{
			Order order = Entity.Order;
			if(order != null) {
				equipmentList = Entity.Order.ObservableOrderEquipments.Where(eq => eq.Direction == Core.Domain.Orders.Direction.PickUp).ToList<OrderEquipment>();
				Entity.Date = order.DeliveryDate != null ? order.DeliveryDate.Value : DateTime.Now;
				Entity.ExpirationDate = Entity.Date.AddDays(10);
				Entity.Supplier = order.Client;

				if(Entity.Id == 0)
				{
					Entity.Organization = order.Contract.Organization;
				}
				
				yTWEquipment.ItemsDataSource = equipmentList;
			} else {
				yDPDatesRange.StartDateOrNull = DateTime.Today;
				yDPDatesRange.EndDateOrNull = DateTime.Today.AddDays(10);
			}
		}

		void Templatewidget_BeforeOpen(object sender, EventArgs e)
		{
			var organizationVersion = Entity.Organization.OrganizationVersionOnDate(Entity.Date);

			if(organizationVersion == null)
			{
				MessageDialogHelper.RunErrorDialog($"На дату М-2 доверенности {Entity.Date.ToString("G")} отсутствует версия организации. Создайте версию организации.");
				templatewidget.CanOpenDocument = false;
				return;
			}

			if(organizationVersion.Leader == null)
			{
				MessageDialogHelper.RunErrorDialog($"Не выбран руководитель в версии №{organizationVersion.Id} организации \"{Entity.Organization.Name}\"");
				templatewidget.CanOpenDocument = false;
				return;
			}

			if(organizationVersion.Accountant == null)
			{
				MessageDialogHelper.RunErrorDialog($"Не выбран бухгалтер в версии №{organizationVersion.Id} организации \"{Entity.Organization.Name}\"");
				templatewidget.CanOpenDocument = false;
				return;
			}

			if(UoW.HasChanges) {
				if(MessageDialogHelper.RunQuestionDialog("Необходимо сохранить документ перед открытием печатной формы, сохранить?")) {
					UoWGeneric.Save();
					RefreshParserRootObject();
				} else {
					templatewidget.CanOpenDocument = false;
				}
			}
		}

		void RefreshParserRootObject()
		{
			if(Entity.DocumentTemplate == null)
				return;
			M2ProxyDocumentParser parser = (Entity.DocumentTemplate.DocParser as M2ProxyDocumentParser);
			parser.RootObject = Entity;
			parser.AddTableEquipmentFromClient(equipmentList);
		}

		void UpdateStates()
		{
			bool isNewDoc = !(Entity.Id > 0);
			evmeOrder.Sensitive = yDPDatesRange.Sensitive = evmeEmployee.Sensitive = evmeSupplier.Sensitive = yETicketNr.Sensitive
				= yDTicketDate.Sensitive = yTWEquipment.Sensitive = entryOrganization.Sensitive = isNewDoc;

			if(Entity.Organization == null || !isNewDoc) {
				return;
			}
			templatewidget.AvailableTemplates = _docTemplateRepository.GetAvailableTemplates(UoW, TemplateType.M2Proxy, Entity.Organization);
			templatewidget.Template = templatewidget.AvailableTemplates.FirstOrDefault();
		}

		public override bool Save()
		{
			if(Entity.Order == null)
				return true;

			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			if(Entity.Order != null) {
				List<OrderDocument> list = new List<OrderDocument> {
					new OrderM2Proxy {
						AttachedToOrder = Entity.Order,
						M2Proxy = Entity,
						Order = Entity.Order
					}
				};
				Entity.Order.AddAdditionalDocuments(list);
			}

			return true;
		}

		public override void Destroy()
		{
			_docTemplateRepository = null;
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
