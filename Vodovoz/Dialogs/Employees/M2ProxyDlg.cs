using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QS.DomainModel.UoW;
using QS.Dialog;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.DocTemplates;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.ViewModel;

namespace Vodovoz.Dialogs.Employees
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class M2ProxyDlg : QS.Dialog.Gtk.EntityDialogBase<M2ProxyDocument>, IEditableDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private List<OrderEquipment> equipmentList;
		public IUnitOfWorkGeneric<Order> UoWOrder { get; private set; }

		bool isEditable = true;

		public bool IsEditable {
			get => isEditable;
			set {
				isEditable = value;
			}
		}


		public M2ProxyDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<M2ProxyDocument>();
			//Entity.Date = DateTime.Now;
			TabName = "Новая доверенность М-2";
			ConfigureDlg();
		}

		public M2ProxyDlg(M2ProxyDocument sub) : this(sub.Id)
		{ }

		public M2ProxyDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<M2ProxyDocument>(id);
			TabName = "Изменение доверенности М-2";
			ConfigureDlg();
		}

		public M2ProxyDlg(IUnitOfWorkGeneric<Order> uow) : this()
		{
			UoWOrder = uow;
			Entity.Order = UoWOrder.Root;
		}

		void ConfigureDlg()
		{
			if(Entity.EmployeeDocument == null && Entity.Employee!=null)
				GetDocument();

			ylabelNumber.Binding.AddBinding(Entity, x => x.Title, x => x.LabelProp).InitializeFromSource();
			var filterOrders = new OrdersFilter(UoW);
			yEForOrder.RepresentationModel = new OrdersVM(filterOrders);
			yEForOrder.Binding.AddBinding(Entity, x => x.Order, x => x.Subject).InitializeFromSource();
			yEForOrder.Changed += (sender, e) => {
				FillForOrder();
			};
			yEForOrder.CanEditReference = QSMain.User.Permissions["can_delete"];
			          
			yentryOrganization.SubjectType = typeof(Organization);
			yentryOrganization.Binding.AddBinding(Entity, x => x.Organization, x => x.Subject).InitializeFromSource();
			yentryOrganization.Changed += (sender, e) => {
				UpdateStates();
			};

			FillForOrder();

			yDPDatesRange.Binding.AddBinding(Entity, x => x.Date, x => x.StartDate).InitializeFromSource();
			yDPDatesRange.Binding.AddBinding(Entity, x => x.ExpirationDate, x => x.EndDate).InitializeFromSource();

			var filterEmployee = new EmployeeFilter(UoW);
			yEEmployee.RepresentationModel = new EmployeesVM(filterEmployee);
			yEEmployee.Binding.AddBinding(Entity, x => x.Employee, x => x.Subject).InitializeFromSource();

			var filterSupplier = new CounterpartyFilter(UoW);
			yESupplier.RepresentationModel = new CounterpartyVM(filterSupplier);
			yESupplier.Binding.AddBinding(Entity, x => x.Supplier, x => x.Subject).InitializeFromSource();

			yETicketNr.Binding.AddBinding(Entity, x => x.TicketNumber, w => w.Text).InitializeFromSource();

			yDTicketDate.Binding.AddBinding(Entity, x => x.TicketDate, x => x.Date).InitializeFromSource();

			RefreshParserRootObject();

			templatewidget.CanRevertCommon = QSMain.User.Permissions["can_set_common_additionalagreement"];
			templatewidget.Binding.AddBinding(Entity, e => e.DocumentTemplate, w => w.Template).InitializeFromSource();
			templatewidget.Binding.AddBinding(Entity, e => e.ChangedTemplateFile, w => w.ChangedDoc).InitializeFromSource();

			templatewidget.BeforeOpen += Templatewidget_BeforeOpen;

			yTWEquipment.ColumnsConfig = ColumnsConfigFactory.Create<OrderEquipment>()
				.AddColumn("Наименование").SetDataProperty(node => node.FullNameString)
				.AddColumn("Направление").SetDataProperty(node => node.DirectionString)
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Count).WidthChars(10)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
				.AddColumn("")
				.Finish();

			UpdateStates();
		}

		void GetDocument()
		{
			var doc = Entity.Employee.GetMainDocuments();
			if(doc.Count > 0)
				Entity.EmployeeDocument = doc[0];
		}

		void FillForOrder()
		{
			Order order = Entity.Order;
			if(order != null) {
				equipmentList = Entity.Order.ObservableOrderEquipments.Where(eq => eq.Direction == Domain.Orders.Direction.PickUp).ToList<OrderEquipment>();
				Entity.Date = order.DeliveryDate != null ? order.DeliveryDate.Value : DateTime.Now;
				Entity.ExpirationDate = Entity.Date.AddDays(10);
				Entity.Supplier = order.Client;
				Entity.Organization = order.Contract.Organization;
				yTWEquipment.ItemsDataSource = equipmentList;
			} else {
				yDPDatesRange.StartDateOrNull = DateTime.Today;
				yDPDatesRange.EndDateOrNull = DateTime.Today.AddDays(10);
				}
		}

		void Templatewidget_BeforeOpen(object sender, EventArgs e)
		{
			if(UoW.HasChanges) {
				if(MessageDialogWorks.RunQuestionDialog("Необходимо сохранить документ перед открытием печатной формы, сохранить?")) {
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
			yEForOrder.Sensitive = yDPDatesRange.Sensitive = yEEmployee.Sensitive = yESupplier.Sensitive = yETicketNr.Sensitive
				= yDTicketDate.Sensitive = yTWEquipment.Sensitive = yentryOrganization.Sensitive = isNewDoc;

			if(Entity.Organization == null || !isNewDoc) {
				return;
			}
			templatewidget.AvailableTemplates = Repository.Client.DocTemplateRepository.GetAvailableTemplates(UoW, TemplateType.M2Proxy, Entity.Organization);
			templatewidget.Template = templatewidget.AvailableTemplates.FirstOrDefault();
		}

		public override bool Save()
		{
			var valid = new QSValidator<M2ProxyDocument>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;
			UoWGeneric.Save();

			if(Entity.Order != null) {
				List<OrderDocument> list = new List<OrderDocument>();
				list.Add(new OrderM2Proxy {
					AttachedToOrder = Entity.Order,
					M2Proxy = Entity,
					Order = Entity.Order
				});
				UoWOrder.Root.AddAdditionalDocuments(list);
			}

			return true;
		}
	}
}
