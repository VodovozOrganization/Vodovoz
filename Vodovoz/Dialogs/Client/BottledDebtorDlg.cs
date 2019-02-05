using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.ViewModel;

namespace Vodovoz.Dialogs.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottledDebtorDlg : QS.Dialog.Gtk.EntityDialogBase<BottleDebtor>
	{
		private string lastComment;
		private Employee employee;

		public BottledDebtorDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<BottleDebtor>();
			TabName = "Новый обзвон";
			Entity.DateOfTaskCreation = DateTime.Now;
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
			phonesview1.IsEditable = false;
			creationDate.Text = Entity.DateOfTaskCreation.ToString("dd/MM/yyyy HH:mm");
			referenceDeliveryPoint.Binding.AddBinding(Entity, s => s.Address, w => w.Subject).InitializeFromSource();
		}


		protected void OnClientReferenceChanged(object sender, EventArgs e)
		{
			referenceDeliveryPoint.RepresentationModel = new ClientDeliveryPointsVM(UoW, Entity.Client);
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
			yCommentsTextView.Buffer.Text=yCommentsTextView.Buffer.Text.Remove(yCommentsTextView.Buffer.Text.Length-lastComment.Length-1,lastComment.Length+1);
			lastComment = String.Empty;
		}

		protected void OnAddCommentButtonClicked(object sender, EventArgs e)
		{
			lastComment = txtAddComment.Buffer.Text;
			lastComment=lastComment.Insert(0,employee.ShortName + " " + DateTime.Now.ToString() + ":  ");
			yCommentsTextView.Buffer.Text += lastComment;
			yCommentsTextView.Buffer.Text += Environment.NewLine;
			txtAddComment.Buffer.Text = String.Empty;
		}

		protected void OnClientAddressChanged(object sender, EventArgs e)
		{
			yBottleReserv.Text = Entity.Address.BottleReserv.ToString();
			phonesview1.Phones = Entity.Address.Phones;
		}

		public override bool Save()
		{
			UoWGeneric.Save();
			return true;
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

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			Save();
		}
	}



}
