using System;
using System.Collections.Generic;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Representations;
using Vodovoz.ViewModel;

namespace Vodovoz.Dialogs.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottledDebtorDlg : QS.Dialog.Gtk.EntityDialogBase<BottleDebtor>
	{
		public event System.Action BeforeChanges;
		public event System.Action TaskChanges;

		private string lastComment;
		private Employee employee;

		public BottledDebtorDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<BottleDebtor>();
			TabName = "Новый обзвон";
			Entity.DateOfTaskCreation = DateTime.Now;
			Entity.NextCallDate = DateTime.Now.AddDays(1);
			createTaskButton.Sensitive = false; 
			ConfigureDlg();
		}

		public BottledDebtorDlg(int id)
		{
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<BottleDebtor>(id);
			TabName = Entity.Client.Name;
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			SetActive();
			yCommentsTextView.Binding.AddBinding(Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();
			CounterpartyReference.RepresentationModel = new CounterpartyVM();
			CounterpartyReference.Binding.AddBinding(Entity, s => s.Client, w => w.Subject).InitializeFromSource();
			nextCallDatePicker.Binding.AddBinding(Entity, s => s.NextCallDate, w => w.DateOrNull).InitializeFromSource();
			debtByAddressEntry.Text = Entity.DebtByAdress.ToString();
			debtByClientEntry.Text = Entity.DebtByClient.ToString();
			TaskStateComboBox.ItemsEnum = typeof(DebtorStatus);
			TaskStateComboBox.Binding.AddBinding(Entity, s => s.TaskState, w => w.SelectedItemOrNull).InitializeFromSource();
			IsTaskCompleteButton.Binding.AddBinding(Entity, s => s.IsTaskComplete, w => w.Active).InitializeFromSource();

			EmployeesVM employeeVM = new EmployeesVM();
			employeeVM.Filter.RestrictCategory = EmployeeCategory.office;
			EmployeeReference.RepresentationModel = employeeVM;
			EmployeeReference.Binding.AddBinding(Entity, s => s.AssignedEmployee, w => w.Subject).InitializeFromSource();

			employee = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			phonesview1.IsReadOnly = true;
			creationDate.Text = Entity.DateOfTaskCreation.ToString("dd/MM/yyyy HH:mm");
			referenceDeliveryPoint.Binding.AddBinding(Entity, s => s.Address, w => w.Subject).InitializeFromSource();
		}

		public override bool Save()
		{
			if(Entity.AssignedEmployee == null || Entity.Client == null || Entity.Address == null) {
				MessageDialogHelper.RunInfoDialog("Необходимо заполнить все поля отмеченные *");
				return false ;
			}
			BeforeChanges?.Invoke();
			UoWGeneric.Save();
			TaskChanges?.Invoke();
			return true;
		}

		protected void SetActive()
		{
			if(Entity.Id != 0) {
				CounterpartyReference.IsEditable = false;
				referenceDeliveryPoint.IsEditable = false;
				nextCallDatePicker.IsEditable = false;
			}
			if(Entity.IsTaskComplete) {
				TaskStateComboBox.Sensitive = false;
				EmployeeReference.IsEditable = false;
				IsTaskCompleteButton.Sensitive = false;
				buttonCreateOrder.Sensitive = false;
				createTaskButton.Sensitive = false;
			}

		}

		protected void CreateNewTask()
		{
			var debtor = new BottledDebtorDlg();
			debtor.Entity.Client = Entity.Client;
			debtor.Entity.Address = Entity.Address;
			debtor.Entity.DateOfTaskCreation = DateTime.Now;
			debtor.Entity.NextCallDate = Entity.NextCallDate;
			debtor.Entity.Comment = Entity.Comment;
			debtor.Entity.AssignedEmployee = Entity.AssignedEmployee;
			OpenSlaveTab(debtor);
			Entity.IsTaskComplete = true;
			SetActive();
		}

		#region WidgetEventHeandler

		//protected void OnButtonSaveClicked(object sender, EventArgs e) //FIXME : Проверить подписку на событие 
		//{
		//	if(!this.HasChanges || Save()) {
		//		OnEntitySaved(true);
		//		OnCloseTab(false);
		//	}
		//}
		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(HasChanges);
		}


		protected void OnClientReferenceChanged(object sender, EventArgs e)
		{
			referenceDeliveryPoint.RepresentationModel = new ClientDeliveryPointsVM(UoW, Entity.Client);
		}
		protected void OnClientAddressChanged(object sender, EventArgs e)
		{
			yBottleReserv.Text = Entity.Address.BottleReserv.ToString();
			phonesview1.Phones = Entity.Address.Phones;
		}

		protected void OnHideLeftPanelButtonClicked(object sender, EventArgs e)
		{
			mainBox.Visible = !mainBox.Visible;
			if(mainBox.Visible)
				hideLeftPanelButton.Label = "<<";
			else
				hideLeftPanelButton.Label = ">>";
		}


		protected void OnCancelLastCommentButtonClicked(object sender, EventArgs e)
		{
			if(String.IsNullOrEmpty(lastComment))
				return;
			yCommentsTextView.Buffer.Text = yCommentsTextView.Buffer.Text.Remove(yCommentsTextView.Buffer.Text.Length - lastComment.Length - 1, lastComment.Length + 1);
			lastComment = String.Empty;
		}
		protected void OnAddCommentButtonClicked(object sender, EventArgs e)
		{
			if(String.IsNullOrEmpty(txtAddComment.Buffer.Text))
				return;
			lastComment = txtAddComment.Buffer.Text;
			lastComment = lastComment.Insert(0, employee.ShortName + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + ": ");
			yCommentsTextView.Buffer.Text += lastComment;
			yCommentsTextView.Buffer.Text += Environment.NewLine;
			txtAddComment.Buffer.Text = String.Empty;
		}

		protected void OnButtonCreateOrderClicked(object sender, EventArgs e)
		{
			if(Entity.Address == null && Entity.Client == null)
				return;
			OrderDlg orderDlg = new OrderDlg();
			orderDlg.Entity.Client = Entity.Client;
			orderDlg.Entity.DeliveryPoint = Entity.Address;
			TabParent.AddSlaveTab(this, orderDlg);
		}

		protected void OnIsTaskCompleteButtonToggled(object sender, EventArgs e)
		{
			SetActive();
		}

		protected void OnNextCallDatePickerDateChangedByUser(object sender, EventArgs e)
		{
		}

		protected void OnCreateTaskButtonClicked(object sender, EventArgs e)
		{
			CreateNewTask();
		}
		#endregion
	}
}
