﻿using Autofac;
using QS.Dialog.Gtk;
using QS.Project.Services;
using QS.Services;
using QSReport;
using QSWidgetLib;
using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Models;
using Vodovoz.Settings.Contacts;
using Vodovoz.Settings.Orders;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Contacts;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CallTaskDlg : EntityDialogBase<CallTask>
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private IOrganizationProvider _organizationProvider;
		private ICounterpartyContractRepository _counterpartyContractRepository;
		private ICounterpartyContractFactory _counterpartyContractFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IBottlesRepository _bottleRepository;
		private readonly ICallTaskRepository _callTaskRepository;
		private readonly IPhoneRepository _phoneRepository;
		private readonly DeliveryPointJournalFilterViewModel _deliveryPointJournalFilterViewModel;
		private string _lastComment;
		private readonly ICommonServices _commonServices;
		private IContactSettings _contactsSettings;

		public CallTaskDlg()
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<CallTask>();
			_employeeRepository = new EmployeeRepository();
			_bottleRepository = new BottlesRepository();
			_callTaskRepository = new CallTaskRepository();
			_phoneRepository = new PhoneRepository();
			_deliveryPointJournalFilterViewModel = new DeliveryPointJournalFilterViewModel();
			_commonServices = ServicesConfig.CommonServices;
			TabName = "Новая задача";
			Entity.CreationDate = DateTime.Now;
			Entity.Source = TaskSource.Handmade;
			Entity.TaskCreator = _employeeRepository.GetEmployeeForCurrentUser(UoW);;
			Entity.EndActivePeriod = DateTime.Now.AddDays(1);
			createTaskButton.Sensitive = false;
			ConfigureDlg();
		}

		public CallTaskDlg(int counterpartyId, int deliveryPointId) : this()
		{
			Entity.Counterparty = UoW.GetById<Counterparty>(counterpartyId);
			Entity.DeliveryPoint = UoW.GetById<DeliveryPoint>(deliveryPointId);
		}

		public CallTaskDlg(CallTask task) : this(task.Id)
		{
		}

		public CallTaskDlg(int callTaskId)
		{
			this.Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<CallTask>(callTaskId);
			_employeeRepository = new EmployeeRepository();
			_bottleRepository = new BottlesRepository();
			_callTaskRepository = new CallTaskRepository();
			_phoneRepository = new PhoneRepository();
			_deliveryPointJournalFilterViewModel = new DeliveryPointJournalFilterViewModel();
			_commonServices = ServicesConfig.CommonServices;
			TabName = Entity.Counterparty?.Name;
			labelCreator.Text = $"Создатель : {Entity.TaskCreator?.ShortName}";
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			var orderOrganizationProviderFactory = new OrderOrganizationProviderFactory(ScopeProvider.Scope);
			_contactsSettings = ScopeProvider.Scope.Resolve<IContactSettings>();
			_organizationProvider = orderOrganizationProviderFactory.CreateOrderOrganizationProvider();
			var orderSettings = ScopeProvider.Scope.Resolve<IOrderSettings>();
			var cashReceiptRepository = new CashReceiptRepository(ServicesConfig.UnitOfWorkFactory, orderSettings);
			_counterpartyContractRepository = new CounterpartyContractRepository(_organizationProvider, cashReceiptRepository);
			_counterpartyContractFactory = new CounterpartyContractFactory(_organizationProvider, _counterpartyContractRepository);

			buttonReportByClient.Sensitive = Entity.Counterparty != null;
			buttonReportByDP.Sensitive = Entity.DeliveryPoint != null;

			comboboxImpotanceType.ItemsEnum = typeof(ImportanceDegreeType);
			comboboxImpotanceType.Binding.AddBinding(Entity, s => s.ImportanceDegree, w => w.SelectedItemOrNull).InitializeFromSource();
			TaskStateComboBox.ItemsEnum = typeof(CallTaskStatus);
			TaskStateComboBox.Binding.AddBinding(Entity, s => s.TaskState, w => w.SelectedItemOrNull).InitializeFromSource();
			IsTaskCompleteButton.Binding.AddBinding(Entity, s => s.IsTaskComplete, w => w.Active).InitializeFromSource();
			IsTaskCompleteButton.Label += Entity.CompleteDate?.ToString("dd / MM / yyyy  HH:mm");
			deadlineYdatepicker.Binding.AddBinding(Entity, s => s.EndActivePeriod, w => w.Date).InitializeFromSource();
			ytextviewComments.Binding.AddBinding(Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();
			yentryTareReturn.ValidationMode = ValidationType.numeric;
			yentryTareReturn.Binding.AddBinding(Entity, s => s.TareReturn, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			textViewCommentAboutClient.Binding
				.AddFuncBinding(Entity, e => e.Counterparty != null ? e.Counterparty.Comment : "",
				w => w.Buffer.Text).InitializeFromSource();
			vboxOldComments.Visible = true;

			var employeeFilterViewModel = new EmployeeFilterViewModel { RestrictCategory = EmployeeCategory.office };
			var employeeJournalFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, employeeFilterViewModel);
			entryAttachedEmployee.SetEntityAutocompleteSelectorFactory(employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory());
			entryAttachedEmployee.Binding.AddBinding(Entity, e => e.AssignedEmployee, w => w.Subject).InitializeFromSource();

			var deliveryPointJournalFactory = _lifetimeScope.Resolve<IDeliveryPointJournalFactory>(new TypedParameter(typeof(DeliveryPointJournalFilterViewModel), _deliveryPointJournalFilterViewModel));
			entityVMEntryDeliveryPoint
				.SetEntityAutocompleteSelectorFactory(deliveryPointJournalFactory.CreateDeliveryPointAutocompleteSelectorFactory());
			entityVMEntryDeliveryPoint.Binding.AddBinding(Entity, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();

			var counterpartyJournalFactory = new CounterpartyJournalFactory();
			entityVMEntryCounterparty
				.SetEntityAutocompleteSelectorFactory(counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope));
			entityVMEntryCounterparty.Binding.AddBinding(Entity, s => s.Counterparty, w => w.Subject).InitializeFromSource();

			var phoneTypeSettings = ScopeProvider.Scope.Resolve<IPhoneTypeSettings>();

			ClientPhonesView.ViewModel = new PhonesViewModel(phoneTypeSettings, _phoneRepository, UoW, _contactsSettings,  _commonServices);
			ClientPhonesView.ViewModel.ReadOnly = true;

			DeliveryPointPhonesView.ViewModel = new PhonesViewModel(phoneTypeSettings, _phoneRepository, UoW, _contactsSettings, _commonServices);
			DeliveryPointPhonesView.ViewModel.ReadOnly = true;

			if(Entity.Counterparty != null)
			{
				if(_deliveryPointJournalFilterViewModel != null)
				{
					_deliveryPointJournalFilterViewModel.Counterparty = Entity.Counterparty;
				}
			}

			UpdateAddressFields();
		}

		public void UpdateAddressFields()
		{
			if(Entity.DeliveryPoint != null)
			{
				debtByAddressEntry.Text = _bottleRepository.GetBottlesDebtAtDeliveryPoint(UoW, Entity.DeliveryPoint).ToString();
				entryReserve.Text = Entity.DeliveryPoint.BottleReserv.ToString();
				DeliveryPointPhonesView.ViewModel.PhonesList = Entity.DeliveryPoint.ObservablePhones;
				ytextviewOldComments.Buffer.Text = _callTaskRepository.GetCommentsByDeliveryPoint(UoW, Entity.DeliveryPoint, Entity);
			}
			else
			{
				debtByAddressEntry.Text = string.Empty;
				entryReserve.Text = string.Empty;
				ytextviewOldComments.Buffer.Text = Entity.Comment;
			}

			UpdateClienInfoFields();
		}

		protected void UpdateClienInfoFields()
		{
			if(Entity.Counterparty != null)
			{
				debtByClientEntry.Text = _bottleRepository.GetBottlesDebtAtCounterparty(UoW, Entity.Counterparty).ToString();
				ClientPhonesView.ViewModel.PhonesList = Entity.Counterparty?.ObservablePhones;
				if(Entity.DeliveryPoint == null)
				{
					debtByAddressEntry.Text = _bottleRepository.GetBottleDebtBySelfDelivery(UoW, Entity.Counterparty).ToString();
				}
			}
			else
			{
				debtByClientEntry.Text = string.Empty;
				ClientPhonesView.ViewModel.PhonesList = null;
			}
		}

		protected void OnButtonSplitClicked(object sender, EventArgs e)
		{
			tablePreviousComments.Visible = !tablePreviousComments.Visible;
			buttonSplit.Label = tablePreviousComments.Visible ? ">>" : "<<";
		}

		#region Comments

		protected void OnCancelLastCommentButtonClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrEmpty(_lastComment))
			{
				return;
			}

			ytextviewComments.Buffer.Text =
				ytextviewComments.Buffer.Text.Remove(ytextviewComments.Buffer.Text.Length - _lastComment.Length - 1, _lastComment.Length + 1);
			_lastComment = string.Empty;
		}

		protected void OnAddCommentButtonClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrEmpty(textviewLastComment.Buffer.Text))
			{
				return;
			}

			Entity.AddComment(UoW, textviewLastComment.Buffer.Text, out _lastComment, _employeeRepository);
			textviewLastComment.Buffer.Text = string.Empty;
		}

		#endregion

		protected void OnButtonCreateOrderClicked(object sender, EventArgs e)
		{
			if(Entity.DeliveryPoint == null)
			{
				return;
			}

			var orderDlg = new OrderDlg();
			orderDlg.Entity.Client = orderDlg.UoW.GetById<Counterparty>(Entity.Counterparty.Id);
			orderDlg.Entity.UpdateClientDefaultParam(UoW, _counterpartyContractRepository, _organizationProvider,
				_counterpartyContractFactory);
			orderDlg.Entity.DeliveryPoint = orderDlg.UoW.GetById<DeliveryPoint>(Entity.DeliveryPoint.Id);

			orderDlg.CallTaskWorker.TaskCreationInteractive = new GtkTaskCreationInteractive();
			TabParent.AddTab(orderDlg, this);
		}

		protected void OnCreateTaskButtonClicked(object sender, EventArgs e)
		{
			var newTask = new CallTaskDlg();
			CallTaskSingletonFactory.GetInstance().CopyTask(UoW, _employeeRepository, Entity, newTask.Entity);
			newTask.UpdateAddressFields();
			TabParent.AddTab(newTask, this);
		}

		public override bool Save()
		{
			var validator = ServicesConfig.ValidationService;
			if(!validator.Validate(Entity))
			{
				return false;
			}

			UoWGeneric.Save();
			return true;
		}

		protected void OnButtonReportByDPClicked(object sender, EventArgs e)
		{
			TabParent.AddTab(new ReportViewDlg(Entity.CreateReportInfoByDeliveryPoint()), this);
		}

		protected void OnButtonReportByClientClicked(object sender, EventArgs e)
		{
			TabParent.AddTab(new ReportViewDlg(Entity.CreateReportInfoByClient()), this);
		}

		protected void OnEntityVMEntryDeliveryPointChanged(object sender, EventArgs e)
		{
			buttonReportByDP.Sensitive = Entity.DeliveryPoint != null;
		}

		protected void OnEntityVMEntryDeliveryPointChangedByUser(object sender, EventArgs e)
		{
			if(Entity.DeliveryPoint != null && Entity.Counterparty == null)
			{
				Entity.Counterparty = Entity.DeliveryPoint.Counterparty;
			}

			UpdateAddressFields();
		}

		protected void OnEntityVMEntryCounterpartyChanged(object sender, EventArgs e)
		{
			buttonReportByClient.Sensitive = Entity.Counterparty != null;
		}

		protected void OnEntityVMEntryCounterpartyChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Counterparty == null)
			{
				if(_deliveryPointJournalFilterViewModel != null)
				{
					_deliveryPointJournalFilterViewModel.Counterparty = null;
				}
			}
			else
			{
				if(_deliveryPointJournalFilterViewModel != null)
				{
					_deliveryPointJournalFilterViewModel.Counterparty = Entity.Counterparty;
				}

				if(Entity.Counterparty.Id != Entity.DeliveryPoint?.Counterparty.Id)
				{
					if(Entity.Counterparty.DeliveryPoints.Count == 1)
					{
						Entity.DeliveryPoint = Entity.Counterparty.DeliveryPoints[0];
					}
					else
					{
						Entity.DeliveryPoint = null;
					}
				}
			}

			UpdateAddressFields();
		}

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
