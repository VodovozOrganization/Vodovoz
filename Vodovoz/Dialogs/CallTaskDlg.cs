using System;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Repositories.Client;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Repository.Operations;
using Vodovoz.ViewModel;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CallTaskDlg : SingleUowTabBase
	{
		public bool SaveDlgState;
		public CallTask Entity;

		private string lastComment;
		private Employee employee;

		public CallTaskDlg(IUnitOfWork uow)
		{
			this.Build();
			UoW = uow;
			TabName = "Новая задача";
			Entity = new CallTask {
				CreationDate = DateTime.Now,
				TaskCreator = EmployeeRepository.GetEmployeeForCurrentUser(UoW) ,
				EndActivePeriod = DateTime.Now.AddDays(1)
			};
			createTaskButton.Sensitive = false;
			ConfigureDlg();
		}

		public CallTaskDlg(IUnitOfWork uow ,CallTask callTask)
		{
			this.Build();
			UoW = uow;
			Entity =callTask;
			TabName = Entity.Client?.Name;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			TaskStateComboBox.ItemsEnum = typeof(CallTaskStatus);
			TaskStateComboBox.Binding.AddBinding(Entity, s => s.TaskState, w => w.SelectedItemOrNull).InitializeFromSource();
			IsTaskCompleteButton.Binding.AddBinding(Entity, s => s.IsTaskComplete, w => w.Active).InitializeFromSource();
			IsTaskCompleteButton.Label += Entity.CompleteDate?.ToString("dd / MM / yyyy  HH:mm");
			deadlineYdatepicker.Binding.AddBinding(Entity, s => s.EndActivePeriod, w => w.Date).InitializeFromSource();
			ytextviewComments.Binding.AddBinding(Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();
			yentryTareReturn.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yentryTareReturn.Text = Entity.TareReturn.ToString();

			EmployeesVM employeeVM = new EmployeesVM();
			employeeVM.Filter.RestrictCategory = EmployeeCategory.office;
			EmployeeyEntryreferencevm.RepresentationModel = employeeVM;
			EmployeeyEntryreferencevm.Binding.AddBinding(Entity, s => s.AssignedEmployee, w => w.Subject).InitializeFromSource();

			deliveryPointYentryreferencevm.RepresentationModel = new DeliveryPointsVM();
			deliveryPointYentryreferencevm.Binding.AddBinding(Entity, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();

			employee = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			ClientPhonesview.UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ClientPhonesview.IsReadOnly = true;

			UpdateAddressFields();
		}

		public bool Save()
		{
			var valid = new QSValidator<CallTask>(Entity);
			valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel);
			Entity.TaskCreator = employee;
			if(valid.IsValid) {
				if(Entity.Id > 0) {
					UoW.Session.Merge(Entity);
					UoW.Save(UoW.GetById<CallTask>(Entity.Id));
				} else {
					UoW.Save(Entity);
				}
				UoW.Commit();
				SaveDlgState = true;
				OnCloseTab(false);
			}
			return valid.IsValid;
		}

		private void UpdateAddressFields()
		{
			if(Entity.DeliveryPoint != null) 
			{
				yentryCounterparty.Text = Entity.Client?.Name;
				debtByAddressEntry.Text = BottlesRepository.GetBottlesAtDeliveryPoint(UoW, Entity.DeliveryPoint).ToString();
				debtByClientEntry.Text = BottlesRepository.GetBottlesAtCounterparty(UoW, Entity.Client).ToString();
				entryReserve.Text = Entity.DeliveryPoint.BottleReserv.ToString();
				ClientPhonesview.Phones = Entity.DeliveryPoint.Phones;
				ytextviewOldComments.Buffer.Text = CallTasksRepository.GetCommentsByDeliveryPoint(UoW, Entity.DeliveryPoint, Entity);
			} 
			else 
			{
				debtByAddressEntry.Text = String.Empty;
				debtByClientEntry.Text = String.Empty;
				entryReserve.Text = String.Empty;
				ClientPhonesview.Phones = null;
				ytextviewOldComments.Buffer.Text = Entity.Comment;
			}
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e) => Save();

		protected void OnButtonCancelClicked(object sender, EventArgs e) => OnCloseTab(false);

		protected void OnDeliveryPointYentryreferencevmChangedByUser(object sender, EventArgs e) => UpdateAddressFields();

		protected void OnButtonSplitClicked(object sender, EventArgs e)
		{
			vboxOldComments.Visible = !vboxOldComments.Visible;
			buttonSplit.Label = vboxOldComments.Visible ? "<<" : ">>";
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
			lastComment = textviewLastComment.Buffer.Text;
			lastComment = lastComment.Insert(0, employee.ShortName + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + ": ");
			ytextviewComments.Buffer.Text += lastComment;
			ytextviewComments.Buffer.Text += Environment.NewLine;
			textviewLastComment.Buffer.Text = String.Empty;
		}
		#endregion

		protected void OnButtonCreateOrderClicked(object sender, EventArgs e)
		{
			if(Entity.DeliveryPoint == null)
				return;
			OrderDlg orderDlg = new OrderDlg();
			TabParent.AddSlaveTab(this, orderDlg);
		}

		protected void OnCreateTaskButtonClicked(object sender, EventArgs e)
		{
			CallTask task = Entity.CreateNewTask();
			CallTaskDlg newTask = new CallTaskDlg(UnitOfWorkFactory.CreateWithoutRoot(), task);
			OpenSlaveTab(newTask);
		}

		protected void OnYentryTareReturnChanged(object sender, EventArgs e)
		{
			if(Int32.TryParse(yentryTareReturn.Text, out int result)) {
				Entity.TareReturn = result;
			}
		}

	}
}