using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NLog;
using QS.Project.Services;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QSOrmProject;
using QSProjectsLib;
using System;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Equipments;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.TempAdapters;
using IDeliveryPointInfoProvider = Vodovoz.ViewModels.Infrastructure.InfoProviders.IDeliveryPointInfoProvider;

namespace Vodovoz
{
	public partial class ServiceClaimDlg
		: QS.Dialog.Gtk.EntityDialogBase<ServiceClaim>,
		ICounterpartyInfoProvider,
		IDeliveryPointInfoProvider,
		ICustomWidthInfoProvider
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();

		private IEmployeeRepository _employeeRepository;
		private IEquipmentRepository _equipmentRepository;
		private INomenclatureRepository _nomenclatureRepository;

		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilterViewModel =
			new DeliveryPointJournalFilterViewModel();

		#region IPanelInfoProvider implementation

		public int? WidthRequest => 420;
		public PanelViewType[] InfoWidgets
		{
			get
			{
				return new[]{
					PanelViewType.CounterpartyView,
					PanelViewType.DeliveryPointView
				};
			}
		}

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		#endregion

		#region ICounterpartyInfoProvider implementation

		public Counterparty Counterparty => evmeClient.Subject as Counterparty;

		#endregion

		public DeliveryPoint DeliveryPoint => evmeDeliveryPoint.Subject as DeliveryPoint;

		public OrderAddressType? TypeOfAddress => null;

		protected static Logger logger = LogManager.GetCurrentClassLogger();

		bool isEditable = true;

		public ServiceClaimDlg(Order order)
		{
			ResolveDependencies();
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<ServiceClaim>(new ServiceClaim (order));
			ConfigureDlg ();
		}

		public ServiceClaimDlg (ServiceClaim sub) : this (sub.Id)
		{
		}

		public ServiceClaimDlg(int id)
		{
			ResolveDependencies();
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<ServiceClaim> (id);
		}

		public ServiceClaimDlg(ServiceClaimType type)
		{
			ResolveDependencies();
			this.Build ();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<ServiceClaim>(new ServiceClaim (type));
			if (type == ServiceClaimType.RegularService)
				EntitySaved += (sender,args)=>CreateOrder();
			Entity.ServiceStartDate = DateTime.Today;
			Entity.ServiceStartDate = DateTime.Now.AddDays(1);
			ConfigureDlg();
		}

		private void ResolveDependencies()
		{
			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_equipmentRepository = _lifetimeScope.Resolve<IEquipmentRepository>();
			_nomenclatureRepository = _lifetimeScope.Resolve<INomenclatureRepository>();
		}

		void CreateOrder()
		{
			var employee = _employeeRepository.GetEmployeeForCurrentUser(UoWGeneric);
			var order = Order.CreateFromServiceClaim(Entity, employee);
			UoWGeneric.Save(order);
			UoWGeneric.Commit();
			var orderDlg = new OrderDlg(order);
			TabParent.AddTab(orderDlg, this);
		}

		void ConfigureDlg()
		{
			enumStatus.Sensitive = enumType.Sensitive = false;
			enumStatusEditable.Sensitive = true;
			notebook1.ShowTabs = false;
			notebook1.CurrentPage = 0;

			enumcomboWithSerial.ItemsEnum = typeof(ServiceClaimEquipmentSerialType);
			enumcomboWithSerial.Binding.AddBinding(Entity, e => e.WithSerial, w => w.SelectedItem).InitializeFromSource();
			enumStatus.ItemsEnum = typeof(ServiceClaimStatus);
			enumStatus.Binding.AddBinding(Entity, e => e.Status, w => w.SelectedItem).InitializeFromSource();
			enumType.ItemsEnum = typeof(ServiceClaimType);
			enumType.Binding.AddBinding(Entity, e => e.ServiceClaimType, w => w.SelectedItem).InitializeFromSource();
			enumPaymentType.ItemsEnum = typeof(PaymentType);
			enumPaymentType.Binding.AddBinding(Entity, e => e.Payment, w => w.SelectedItem).InitializeFromSource();

			checkRepeated.Binding.AddBinding(Entity, e => e.RepeatedService, w => w.Active).InitializeFromSource();
			dataNumber.Binding.AddBinding(Entity, e => e.Id, w => w.LabelProp, new IdToStringConverter()).InitializeFromSource();
			labelTotalPrice.Binding.AddFuncBinding(Entity, e => e.TotalPrice.ToString("C"), w => w.LabelProp).InitializeFromSource();
			datePickUpDate.Binding.AddBinding(Entity, e => e.ServiceStartDate, w => w.Date).InitializeFromSource();
			textReason.Binding.AddBinding(Entity, e => e.Reason, w => w.Buffer.Text).InitializeFromSource();
			textKit.Binding.AddBinding(Entity, e => e.Kit, w => w.Buffer.Text).InitializeFromSource();
			textDiagnosticsResult.Binding.AddBinding(Entity, e => e.DiagnosticsResult, w => w.Buffer.Text).InitializeFromSource();

			var clientFactory = new CounterpartyJournalFactory();
			evmeClient.SetEntityAutocompleteSelectorFactory(clientFactory.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope));
			evmeClient.Binding.AddBinding(Entity, e => e.Counterparty, w => w.Subject).InitializeFromSource();
			evmeClient.Changed += OnReferenceCounterpartyChanged;

			var employeeFactory = _lifetimeScope.Resolve<IEmployeeJournalFactory>();
			evmeEngineer.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEngineer.Binding.AddBinding(Entity, e => e.Engineer, w => w.Subject).InitializeFromSource();

			yentryEquipmentReplacement.ItemsQuery = _equipmentRepository.AvailableOnDutyEquipmentQuery();
			yentryEquipmentReplacement.SetObjectDisplayFunc<Equipment>(e => e.Title);
			yentryEquipmentReplacement.Binding
				.AddBinding(UoWGeneric.Root, serviceClaim => serviceClaim.ReplacementEquipment, widget => widget.Subject)
				.InitializeFromSource();

			evmeDeliveryPoint.Sensitive = (UoWGeneric.Root.Counterparty != null);
			evmeDeliveryPoint.Binding.AddBinding(Entity, e => e.DeliveryPoint, w => w.Subject).InitializeFromSource();
			evmeDeliveryPoint.Changed += OnReferenceDeliveryPointChanged;
			var dpFactory = _lifetimeScope.Resolve<IDeliveryPointJournalFactory>();
			dpFactory.SetDeliveryPointJournalFilterViewModel(_deliveryPointJournalFilterViewModel);
			evmeDeliveryPoint.SetEntityAutocompleteSelectorFactory(dpFactory.CreateDeliveryPointByClientAutocompleteSelectorFactory());

			var entityEntryViewModel = new LegacyEEVMBuilderFactory<ServiceClaim>(this, Entity, UoW, Startup.MainWin.NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Nomenclature)
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel, NomenclatureFilterViewModel>(filter =>
				{
					filter.RestrictCategory = NomenclatureCategory.equipment;
					filter.RestrictArchive = false;
					filter.HidenByDefault = true;
				})
				.UseViewModelDialog<NomenclatureViewModel>()
				.Finish();

			entryNomenclature.ViewModel = entityEntryViewModel;

			entryNomenclature.ViewModel.Changed += OnNomenclatureVMEntryChanged;

			referenceEquipment.SubjectType = typeof(Equipment);
			referenceEquipment.Sensitive = (UoWGeneric.Root.Nomenclature != null);
			referenceEquipment.Binding.AddBinding(Entity, e => e.Equipment, w => w.Subject).InitializeFromSource();

			treePartsAndServices.ItemsDataSource = UoWGeneric.Root.ObservableServiceClaimItems;
			treeHistory.ItemsDataSource = UoWGeneric.Root.ObservableServiceClaimHistory;

			treePartsAndServices.ColumnsConfig = FluentColumnsConfig<ServiceClaimItem>.Create()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Name : "-")
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Count)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
				.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
				.AddSetter((c, i) => c.Editable = isEditable)
				.WidthChars(10)
				.AddColumn("Цена").AddNumericRenderer(node => node.Price).Digits(2)
				.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Сумма").AddNumericRenderer(node => node.Total).Digits(2)
				.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.Finish();

			treeHistory.ColumnsConfig = FluentColumnsConfig<ServiceClaimHistory>.Create()
				.AddColumn("Дата").AddTextRenderer(node => node.Date.ToShortDateString())
				.AddColumn("Время").AddTextRenderer(node => node.Date.ToString("HH:mm"))
				.AddColumn("Статус").AddTextRenderer(node => node.Status.GetEnumTitle())
				.AddColumn("Сотрудник").AddTextRenderer(node => node.Employee == null ? " - " : node.Employee.FullName)
				.AddColumn("Комментарий").AddTextRenderer(node => node.Comment)
				.Finish();

			UoWGeneric.Root.ObservableServiceClaimItems.ElementChanged += (aList, aIdx) => FixPrice(aIdx[0]);
			configureAvailableNextStatus();
			Entity.PropertyChanged += Entity_PropertyChanged;

			if(UoWGeneric.Root.ServiceClaimType == ServiceClaimType.JustService)
			{
				evmeDeliveryPoint.Visible = false;
				labelDeliveryPoint.Visible = false;
				yentryEquipmentReplacement.Visible = false;
				labelReplacement.Visible = false;
			}
			datePickUpDate.IsEditable = Entity.InitialOrder == null;
			Menu menu = new Menu();
			var menuItemInitial = new MenuItem("Перейти к начальному заказу");
			menuItemInitial.Sensitive = Entity.InitialOrder != null;
			menuItemInitial.Activated += MenuInitialOrderActivated;
			menu.Add(menuItemInitial);
			var menuItemFinal = new MenuItem("Перейти к финальному заказу");
			menuItemFinal.Sensitive = Entity.FinalOrder != null;
			menuItemFinal.Activated += MenuFinalOrderActivated;
			menu.Add(menuItemFinal);
			menuActions.Menu = menu;
			menu.ShowAll();
		}

		void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == Entity.GetPropertyName(x => x.Status))
			{
				configureAvailableNextStatus();
			}
		}

		public void MenuInitialOrderActivated(object sender, EventArgs args)
		{

			var orderDlg = new OrderDlg(Entity.InitialOrder);
			TabParent.AddTab(orderDlg, this);
		}

		public void MenuFinalOrderActivated(object sender, EventArgs args)
		{
			var orderDlg = new OrderDlg(Entity.FinalOrder);
			TabParent.AddTab(orderDlg, this);
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			throw new NotImplementedException();

			CounterpartyContract contract;
			if(contract == null)
			{
				RunContractCreateDialog();
				return false;
			}

			UoWGeneric.Session.Refresh(contract);

			if(UoWGeneric.Root.InitialOrder != null)
				UoWGeneric.Root.InitialOrder.AddServiceClaimAsInitial(UoWGeneric.Root);

			if(UoWGeneric.IsNew)
				UoWGeneric.Root.AddHistoryRecord(
					UoWGeneric.Root.Status,
					string.IsNullOrWhiteSpace(textComment.Buffer.Text) ? "Заявка зарегистрирована" : textComment.Buffer.Text,
					_employeeRepository);

			logger.Info("Сохраняем заявку на обслуживание...");
			UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}

		#endregion

		protected void OnNomenclatureVMEntryChanged(object sender, EventArgs e)
		{
			FixNomenclatureAndEquipmentSensitivity();

			if(UoWGeneric.Root.Equipment != null &&
				UoWGeneric.Root.Equipment.Nomenclature.Id != UoWGeneric.Root.Nomenclature.Id)
			{
				UoWGeneric.Root.Equipment = null;
			}
		}

		protected void OnReferenceCounterpartyChanged(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Counterparty));
			FixNomenclatureAndEquipmentSensitivity();

			evmeDeliveryPoint.Sensitive = Entity.Counterparty != null;
			evmeDeliveryPoint.Subject = null;
			if(Entity.Counterparty == null)
			{
				return;
			}

			_deliveryPointJournalFilterViewModel.Counterparty = Entity.Counterparty;
		}

		void RunContractCreateDialog()
		{
			ITdiTab dlg;
			string paymentTypeString = "";
			switch(UoWGeneric.Root.Payment)
			{
				case PaymentType.Cash:
					paymentTypeString = "наличной";
					break;
				case PaymentType.Cashless:
					paymentTypeString = "безналичной";
					break;
				case PaymentType.Barter:
					paymentTypeString = "бартерной";
					break;
			}
			string question = "Отсутствует договор с клиентом для " +
							  paymentTypeString +
							  " формы оплаты. Создать?";
			if(MessageDialogWorks.RunQuestionDialog(question))
			{

				Organization organization = null;
				throw new NotImplementedException();

				dlg = new CounterpartyContractDlg(UoWGeneric.Root.Counterparty, organization);
				(dlg as IContractSaved).ContractSaved += (sender, e) =>
				{
					if(UoWGeneric.Root.InitialOrder != null)
						UoWGeneric.Root.InitialOrder.ObservableOrderDocuments.Add(new OrderContract
						{
							Order = UoWGeneric.Root.InitialOrder,
							AttachedToOrder = UoWGeneric.Root.InitialOrder,
							Contract = e.Contract
						});
				};
				TabParent.AddSlaveTab(this, dlg);
			}
		}

		protected void OnButtonAddServiceClicked(object sender, EventArgs e)
		{
			OpenDialog(_nomenclatureRepository.NomenclatureOfServices());
		}

		protected void OnButtonAddPartClicked(object sender, EventArgs e)
		{
			OpenDialog(_nomenclatureRepository.NomenclatureOfPartsForService());
		}

		void OpenDialog(NHibernate.Criterion.QueryOver<Nomenclature> nomenclatureType)
		{
		}

		void NomenclatureSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			UoWGeneric.Root.ObservableServiceClaimItems.Add(new ServiceClaimItem
			{
				ServiceClaim = UoWGeneric.Root,
				Nomenclature = e.Subject as Nomenclature,
				Price = (e.Subject as Nomenclature).GetPrice(1),
				Count = 1
			});
		}

		void FixPrice(int id)
		{
			ServiceClaimItem item = UoWGeneric.Root.ObservableServiceClaimItems[id];
			item.Price = item.Nomenclature.GetPrice((int)item.Count);
		}

		protected void OnToggleInfoToggled(object sender, EventArgs e)
		{
			if(toggleInfo.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnToggleServicesAndWorksToggled(object sender, EventArgs e)
		{
			if(toggleServicesAndWorks.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnToggleHistoryToggled(object sender, EventArgs e)
		{
			if(toggleHistory.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnButtonAddClicked(object sender, EventArgs e)
		{
			if(!String.IsNullOrWhiteSpace(textComment.Buffer.Text) || MessageDialogWorks.RunQuestionDialog("Вы не заполнили комментарий. Продолжить?"))
			{
				ServiceClaimStatus newStatus = (ServiceClaimStatus)(enumStatusEditable.SelectedItem ?? UoWGeneric.Root.Status);
				UoWGeneric.Root.AddHistoryRecord(newStatus, textComment.Buffer.Text, _employeeRepository);
			}
		}

		void configureAvailableNextStatus()
		{
			var enumList = UoWGeneric.Root.GetAvailableNextStatusList();
			enumStatusEditable.SetEnumItems<ServiceClaimStatus>(enumList);
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		protected void OnReferenceEquipmentChanged(object sender, EventArgs e)
		{
			Equipment selectedEquipment = referenceEquipment.Subject as Equipment;
			entryNomenclature.ViewModel.Entity = selectedEquipment != null ? selectedEquipment.Nomenclature : null;
		}

		protected void OnEnumcomboWithSerialEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			FixNomenclatureAndEquipmentSensitivity();
		}

		protected void OnReferenceDeliveryPointChanged(object sender, EventArgs e)
		{
			if(CurrentObjectChanged != null)
				CurrentObjectChanged(this, new CurrentObjectChangedArgs(DeliveryPoint));

			UpdateEquipmentState();
			FixNomenclatureAndEquipmentSensitivity();
			referenceEquipment.ItemsQuery = _equipmentRepository.GetEquipmentAtDeliveryPointQuery(UoWGeneric.Root.Counterparty, UoWGeneric.Root.DeliveryPoint);
		}

		void UpdateEquipmentState()
		{
			int equipmentsCounts =
				_equipmentRepository.GetEquipmentAtDeliveryPointQuery(
					Entity.Counterparty, Entity.DeliveryPoint).GetExecutableQueryOver(UoW.Session).RowCount();

			if(equipmentsCounts == 0 && Entity.Equipment == null)
			{
				enumcomboWithSerial.SelectedItem = ServiceClaimEquipmentSerialType.WithoutSerial;
			}
			ylabelEquipmentInfo.LabelProp = RusNumber.FormatCase(equipmentsCounts, "На точке числится {0} единица оборудования", "На точке числится {0} единицы оборудования", "На точке числится {0} единиц оборудования");
			enumcomboWithSerial.Sensitive = equipmentsCounts > 0;
		}

		protected void FixNomenclatureAndEquipmentSensitivity()
		{
			bool withSerial = ((ServiceClaimEquipmentSerialType)enumcomboWithSerial.SelectedItem) == ServiceClaimEquipmentSerialType.WithSerial;
			referenceEquipment.Sensitive = withSerial && UoWGeneric.Root.Counterparty != null &&
				(UoWGeneric.Root.DeliveryPoint != null || UoWGeneric.Root.ServiceClaimType == ServiceClaimType.JustService);
			entryNomenclature.Sensitive = !withSerial && UoWGeneric.Root.Counterparty != null;
		}

		public override void Destroy()
		{
			_employeeRepository = null;
			_equipmentRepository = null;
			_nomenclatureRepository = null;
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
