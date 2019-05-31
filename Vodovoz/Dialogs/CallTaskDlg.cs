using System;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
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
	public partial class CallTaskDlg : EntityDialogBase<CallTask>
	{
		private string lastComment;
		private Employee employee;

		public CallTaskDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<CallTask>();
			TabName = "Новая задача";
			Entity.CreationDate = DateTime.Now;
			Entity.TaskCreator = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			Entity.EndActivePeriod = DateTime.Now.AddDays(1);
			createTaskButton.Sensitive = false;
			ConfigureDlg();
		}

		public CallTaskDlg(CallTask task) : this(task.Id) { }

		public CallTaskDlg(int callTaskId)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<CallTask>(callTaskId);
			TabName = Entity.Counterparty?.Name;
			labelCreator.Text = String.Format("Создатель : {0}", Entity.TaskCreator?.ShortName);
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			comboboxImpotanceType.ItemsEnum = typeof(ImportanceDegreeType);
			comboboxImpotanceType.Binding.AddBinding(Entity, s => s.ImportanceDegree, w => w.SelectedItemOrNull).InitializeFromSource();
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

			ClientPhonesView.UoW = UnitOfWorkFactory.CreateWithoutRoot();
			ClientPhonesView.IsReadOnly = true;

			DeliveryPointPhonesview.UoW = UnitOfWorkFactory.CreateWithoutRoot();
			DeliveryPointPhonesview.IsReadOnly = true;

			UpdateAddressFields();
		}

		public void UpdateAddressFields()
		{
			if(Entity.DeliveryPoint != null) 
			{
				yentryCounterparty.Text = Entity.Counterparty?.Name;
				debtByAddressEntry.Text = BottlesRepository.GetBottlesAtDeliveryPoint(UoW, Entity.DeliveryPoint).ToString();

				if(Entity.Counterparty != null)
					debtByClientEntry.Text = BottlesRepository.GetBottlesAtCounterparty(UoW, Entity.Counterparty).ToString();

				entryReserve.Text = Entity.DeliveryPoint.BottleReserv.ToString();
				DeliveryPointPhonesview.Phones = Entity.DeliveryPoint.Phones;
				ClientPhonesView.Phones = Entity.Counterparty?.Phones;
				ytextviewOldComments.Buffer.Text = CallTasksRepository.GetCommentsByDeliveryPoint(UoW, Entity.DeliveryPoint, Entity);
			} 
			else 
			{
				debtByAddressEntry.Text = String.Empty;
				debtByClientEntry.Text = String.Empty;
				entryReserve.Text = String.Empty;
				ClientPhonesView.Phones = null;
				ytextviewOldComments.Buffer.Text = Entity.Comment;
			}
		}

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
			Entity.AddComment(UoW, textviewLastComment.Buffer.Text,out lastComment);
			textviewLastComment.Buffer.Text = String.Empty;
		}
		#endregion

		protected void OnButtonCreateOrderClicked(object sender, EventArgs e)
		{
			if(Entity.DeliveryPoint == null)
				return;
			OrderDlg orderDlg = new OrderDlg();
			orderDlg.Entity.Client = Entity.Counterparty;
			orderDlg.Entity.UpdateClientDefaultParam();
			orderDlg.Entity.DeliveryPoint = Entity.DeliveryPoint;
			TabParent.AddTab(orderDlg , this);
		}

		protected void OnCreateTaskButtonClicked(object sender, EventArgs e)
		{
			CallTaskDlg newTask = new CallTaskDlg();
			newTask.Entity.CopyTask(Entity);
			newTask.UpdateAddressFields();
			TabParent.AddTab(newTask,this);
		}

		protected void OnYentryTareReturnChanged(object sender, EventArgs e)
		{
			if(Int32.TryParse(yentryTareReturn.Text, out int result))
				Entity.TareReturn = result;
		}

		public override bool Save()
		{
			var valid = new QSValidator<CallTask>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)Toplevel))
				return false;
			UoWGeneric.Save();
			return true;
		}

	}
}