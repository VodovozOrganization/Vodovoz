using System;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;
using Vodovoz.Filters.ViewModels;
using QSReport;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using QSWidgetLib;
using QSOrmProject;
using Vodovoz.Dialogs.Phones;
using Vodovoz.Infrastructure.Services;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Parameters;
using Vodovoz.EntityRepositories;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CallTaskDlg : EntityDialogBase<CallTask>
	{
		private string lastComment;
		private Employee employee;

		private IEmployeeRepository employeeRepository { get; set; } = EmployeeSingletonRepository.GetInstance();
		private IBottlesRepository bottleRepository { get; set; } = new BottlesRepository();
		private ICallTaskRepository callTaskRepository { get; set; } = new CallTaskRepository();
		private IPhoneRepository phoneRepository { get; set; } = new PhoneRepository();

		public CallTaskDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CallTask>();
			TabName = "Новая задача";
			Entity.CreationDate = DateTime.Now;
			Entity.Source = TaskSource.Handmade;
			Entity.TaskCreator = employeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.EndActivePeriod = DateTime.Now.AddDays(1);
			createTaskButton.Sensitive = false;
			ConfigureDlg();
		}

		public CallTaskDlg(int counterpartyId, int deliveryPointId) : this()
		{
			Entity.Counterparty = UoW.GetById<Counterparty>(counterpartyId);
			Entity.DeliveryPoint = UoW.GetById<DeliveryPoint>(deliveryPointId);
		}

		public CallTaskDlg(CallTask task) : this(task.Id) { }

		public CallTaskDlg(int callTaskId)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CallTask>(callTaskId);
			TabName = Entity.Counterparty?.Name;
			labelCreator.Text = $"Создатель : {Entity.TaskCreator?.ShortName}";
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
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


			EmployeeFilterViewModel employeeFilterViewModel = new EmployeeFilterViewModel();
			employeeFilterViewModel.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.office);
			EmployeesVM employeeVM = new EmployeesVM(employeeFilterViewModel);
			EmployeeyEntryreferencevm.RepresentationModel = employeeVM;

			EmployeeyEntryreferencevm.Binding.AddBinding(Entity, s => s.AssignedEmployee, w => w.Subject).InitializeFromSource();

			deliveryPointYentryreferencevm.RepresentationModel = new DeliveryPointsVM();
			deliveryPointYentryreferencevm.Binding.AddBinding(Entity, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();

			counterpartyYentryreferencevm.RepresentationModel = new CounterpartyVM();
			counterpartyYentryreferencevm.Binding.AddBinding(Entity, s => s.Counterparty, w => w.Subject).InitializeFromSource();

			employee = employeeRepository.GetEmployeeForCurrentUser(UoW);

			ClientPhonesView.ViewModel = new PhonesViewModel(phoneRepository, UoW, ContactParametersProvider.Instance);
			ClientPhonesView.ViewModel.ReadOnly = true;

			DeliveryPointPhonesView.ViewModel = new PhonesViewModel(phoneRepository, UoW, ContactParametersProvider.Instance);
			DeliveryPointPhonesView.ViewModel.ReadOnly = true;

			if(Entity.Counterparty != null)
				(deliveryPointYentryreferencevm.RepresentationModel.JournalFilter as DeliveryPointFilter).Client = Entity.Counterparty;

			UpdateAddressFields();
		}

		public void UpdateAddressFields()
		{
			if(Entity.DeliveryPoint != null) 
			{
				debtByAddressEntry.Text = bottleRepository.GetBottlesAtDeliveryPoint(UoW, Entity.DeliveryPoint).ToString();
				entryReserve.Text = Entity.DeliveryPoint.BottleReserv.ToString();
				DeliveryPointPhonesView.ViewModel.PhonesList = Entity.DeliveryPoint.ObservablePhones;
				ytextviewOldComments.Buffer.Text = callTaskRepository.GetCommentsByDeliveryPoint(UoW, Entity.DeliveryPoint, Entity);
			} 
			else 
			{
				debtByAddressEntry.Text = String.Empty;
				entryReserve.Text = String.Empty;
				ytextviewOldComments.Buffer.Text = Entity.Comment;
			}

			UpdateClienInfoFields();
		}

		protected void UpdateClienInfoFields()
		{
			if(Entity.Counterparty != null) 
			{
				debtByClientEntry.Text = bottleRepository.GetBottlesAtCounterparty(UoW, Entity.Counterparty).ToString();
				ClientPhonesView.ViewModel.PhonesList = Entity.Counterparty?.ObservablePhones;
				if(Entity.DeliveryPoint == null)
					debtByAddressEntry.Text = bottleRepository.GetBottleDebtBySelfDelivery(UoW, Entity.Counterparty).ToString();
			} 
			else 
			{
				debtByClientEntry.Text = String.Empty;
				ClientPhonesView.ViewModel.PhonesList = null;
			}
		}

		protected void OnDeliveryPointYentryreferencevmChangedByUser(object sender, EventArgs e) 
		{
			if(Entity.DeliveryPoint != null && Entity.Counterparty == null)
				Entity.Counterparty = Entity.DeliveryPoint.Counterparty;
			UpdateAddressFields();
		}

		protected void OnCounterpartyYentryreferencevmChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Counterparty == null)
				(deliveryPointYentryreferencevm.RepresentationModel.JournalFilter as DeliveryPointFilter).Client = null;
			else {
				(deliveryPointYentryreferencevm.RepresentationModel.JournalFilter as DeliveryPointFilter).Client = Entity.Counterparty;
				if(Entity.Counterparty.Id != Entity.DeliveryPoint?.Counterparty.Id) {
					if(Entity.Counterparty.DeliveryPoints.Count == 1)
						Entity.DeliveryPoint = Entity.Counterparty.DeliveryPoints[0];
					else
						Entity.DeliveryPoint = null;
				}
			}
			UpdateAddressFields();
		}

		protected void OnButtonSplitClicked(object sender, EventArgs e)
		{
			vboxOldComments.Visible = !vboxOldComments.Visible;
			buttonSplit.Label = vboxOldComments.Visible ? ">>" : "<<";
		}

		#region Comments
		protected void OnCancelLastCommentButtonClicked(object sender, EventArgs e)
		{
			if(String.IsNullOrEmpty(lastComment))
				return;
			ytextviewComments.Buffer.Text = ytextviewComments.Buffer.Text.Remove(ytextviewComments.Buffer.Text.Length - lastComment.Length - 1, lastComment.Length + 1);
			lastComment = String.Empty;
		}

		protected void OnAddCommentButtonClicked(object sender, EventArgs e)
		{
			if(String.IsNullOrEmpty(textviewLastComment.Buffer.Text))
				return;
			Entity.AddComment(UoW, textviewLastComment.Buffer.Text,out lastComment, employeeRepository);
			textviewLastComment.Buffer.Text = String.Empty;
		}
		#endregion

		protected void OnButtonCreateOrderClicked(object sender, EventArgs e)
		{
			if(Entity.DeliveryPoint == null)
				return;

			OrderDlg orderDlg = new OrderDlg();
			orderDlg.Entity.Client = orderDlg.UoW.GetById<Counterparty>(Entity.Counterparty.Id);
			orderDlg.Entity.UpdateClientDefaultParam();
			orderDlg.Entity.DeliveryPoint = orderDlg.UoW.GetById<DeliveryPoint>(Entity.DeliveryPoint.Id);

			orderDlg.CallTaskWorker.TaskCreationInteractive = new GtkTaskCreationInteractive();
			TabParent.AddTab(orderDlg , this);
		}

		protected void OnCreateTaskButtonClicked(object sender, EventArgs e)
		{
			CallTaskDlg newTask = new CallTaskDlg();
			CallTaskSingletonFactory.GetInstance().CopyTask(UoW, employeeRepository, Entity, newTask.Entity);
			newTask.UpdateAddressFields();
			TabParent.AddTab(newTask,this);
		}

		public override bool Save()
		{
			var valid = new QSValidator<CallTask>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)Toplevel))
				return false;
			UoWGeneric.Save();
			return true;
		}

		protected void OnButtonReportByDPClicked(object sender, EventArgs e)
		{
			TabParent.AddTab(new ReportViewDlg(Entity.CreateReportInfoByDeliveryPoint()), this);
		}

		protected void OnButtonReportByClientClicked(object sender, EventArgs e)
		{
			TabParent.AddTab(new ReportViewDlg(Entity.CreateReportInfoByClient()),this);
		}

		protected void OnDeliveryPointYentryreferencevmChanged(object sender, EventArgs e)
		{
			buttonReportByDP.Sensitive = Entity.DeliveryPoint != null;
		}

		protected void OnCounterpartyYentryreferencevmChanged(object sender, EventArgs e)
		{
			buttonReportByClient.Sensitive = Entity.Counterparty != null;
		}
	}
}