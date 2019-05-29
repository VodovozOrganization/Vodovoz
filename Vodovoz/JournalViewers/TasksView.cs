using System;
using System.Linq;
using Gtk;
using QS.Deletion;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Representations;
using Vodovoz.Services;
using Vodovoz.ViewModel;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TasksView : SingleUowTabBase
	{
		CallTasksVM callTasksVM;
		public TasksView()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.TabName = "Журнал задач для обзвона";
			ConfigureDlg();
		}

		public void ConfigureDlg()
		{
			EmployeesVM employeeVM = new EmployeesVM();
			employeeVM.Filter.RestrictCategory = EmployeeCategory.office;
			entryreferencevmEmployeeFilter.RepresentationModel = employeeVM;
			calltaskfilterview.Refiltered += (sender, e) => UpdateStatistics();
			taskStatusComboBox.ItemsEnum = typeof(CallTaskStatus);
			representationtreeviewTask.Selection.Mode = SelectionMode.Multiple;
			callTasksVM = new CallTasksVM(new BaseParametersProvider());
			callTasksVM.NeedUpdate = ycheckbuttonAutoUpdate.Active;
			calltaskfilterview.Refiltered += (sender, e) => callTasksVM.UpdateNodes();
			callTasksVM.ItemsListUpdated += (sender, e) => UpdateStatistics();
			callTasksVM.Filter = calltaskfilterview.GetQueryFilter();
			representationtreeviewTask.RepresentationModel = callTasksVM;
			UpdateStatistics();
		}

		public void UpdateStatistics()
		{
			var statistics = callTasksVM.GetStatistics(calltaskfilterview.Filter.Employee);

			hboxStatistics.Children.OfType<Widget>().ToList().ForEach(x => x.Destroy());

			foreach(var item in statistics) 
			{
				var stats = new Label(item.Key + item.Value);
				hboxStatistics.Add(stats);
				stats.Show();

				var sep = new VSeparator();
				hboxStatistics.Add(sep);
				sep.Show();
			}
		}

		#region BaseJournalHeandler
		protected void OnAddTaskButtonClicked(object sender, EventArgs e)
		{
			CallTaskDlg dlg = new CallTaskDlg();
			TabParent.AddTab(dlg,this);
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			var selected = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().FirstOrDefault();
			if(selected == null)
				return;
			CallTaskDlg dlg = new CallTaskDlg(selected.Id);
			OpenSlaveTab(dlg);
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var selected = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>();
			foreach(var item in selected)
				DeleteHelper.DeleteEntity(typeof(CallTask), item.Id);

			callTasksVM.UpdateNodes();

		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e) => callTasksVM.UpdateNodes();

		protected void OnRadiobuttonShowFilterToggled(object sender, EventArgs e)
		{
			hboxEditSelected.Visible = false;
		}

		protected void OnRadiobuttonEditSelectedToggled(object sender, EventArgs e)
		{
			calltaskfilterview.Visible = false;
		}

		protected void OnRadiobuttonShowFilterClicked(object sender, EventArgs e)
		{
			calltaskfilterview.Visible = !calltaskfilterview.Visible;
		}

		protected void OnRadiobuttonEditSelectedClicked(object sender, EventArgs e)
		{
			hboxEditSelected.Visible = !hboxEditSelected.Visible;
		}

		protected void OnSearchentityTextChanged(object sender, EventArgs e)
		{
			representationtreeviewTask.SearchHighlightText = searchentity.Text;
			representationtreeviewTask.RepresentationModel.SearchString = searchentity.Text;
		}

		protected void OnRepresentationtreeviewTaskRowActivated(object o, RowActivatedArgs args)
		{
			buttonEdit.Click();
		}

		#endregion

		#region ChangeEntityStateButton

		protected void OnAssignedEmployeeButtonClicked(object sender, EventArgs e) => entryreferencevmEmployeeFilter.OpenSelectDialog("Ответственный :");

		protected void OnEntryreferencevmEmployeeFilterChangedByUser(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.AssignedEmployee = entryreferencevmEmployeeFilter.Subject as Employee);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			callTasksVM.ChangeEnitity(action , tasks);
		}

		protected void OnCompleteTaskButtonClicked(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.IsTaskComplete = true);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			callTasksVM.ChangeEnitity(action, tasks);
		}

		protected void OnTaskstateButtonEnumItemClicked(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.TaskState = (CallTaskStatus)taskStatusComboBox.SelectedItem);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			callTasksVM.ChangeEnitity(action, tasks);
		}

		protected void OnDatepickerDeadlineChangeDateChangedByUser(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.EndActivePeriod = datepickerDeadlineChange.Date);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			callTasksVM.ChangeEnitity(action, tasks);
			datepickerDeadlineChange.Clear();
		}

		protected void OnRepresentationtreeviewTaskButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != 3)
				return;

			var selectedObj = representationtreeviewTask.GetSelectedObjects()?[0];
			var selectedNodeId = (selectedObj as CallTaskVMNode)?.Id;
			if(selectedNodeId == null)
				return;

			RepresentationSelectResult[] representation = { new RepresentationSelectResult(selectedNodeId.Value, selectedObj) };
			var popup = callTasksVM.GetPopupMenu(representation);
			popup.ShowAll();
			popup.Popup();
		}

		protected void OnYcheckbuttonAutoUpdateToggled(object sender, EventArgs e)
		{
			callTasksVM.NeedUpdate = ycheckbuttonAutoUpdate.Active;
		} 
		#endregion
	}
}