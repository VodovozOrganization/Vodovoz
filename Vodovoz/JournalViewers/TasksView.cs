using System;
using System.Linq;
using Gtk;
using QS.Deletion;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Representations;
using Vodovoz.ViewModel;
using Vodovoz.Filters.ViewModels;
using CallTaskFilterView = Vodovoz.Filters.GtkViews.CallTaskFilterView;
using QS.Project.Services;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TasksView : SingleUowTabBase
	{
		CallTasksVM callTasksVM;
		CallTaskFilterView callTaskFilterView;

		public TasksView()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.TabName = "Журнал задач для обзвона";
			ConfigureDlg();
		}

		public void ConfigureDlg()
		{ 
			representationentryEmployee.RepresentationModel = new EmployeesVM(UoW);
			taskStatusComboBox.ItemsEnum = typeof(CallTaskStatus);
			representationtreeviewTask.Selection.Mode = SelectionMode.Multiple;
			callTasksVM = new CallTasksVM(new BaseParametersProvider());
			callTasksVM.NeedUpdate = ycheckbuttonAutoUpdate.Active;
			callTasksVM.ItemsListUpdated += (sender, e) => UpdateStatistics();
			callTasksVM.Filter = new CallTaskFilterViewModel();
			callTasksVM.PropertyChanged += CreateCallTaskFilterView;
			representationtreeviewTask.RepresentationModel = callTasksVM;
			CreateCallTaskFilterView(callTasksVM.Filter, EventArgs.Empty);
			UpdateStatistics();
		}

		void CreateCallTaskFilterView(object sender, EventArgs e)
		{
			if(callTaskFilterView != null)
				callTaskFilterView.Destroy();

			callTaskFilterView = new CallTaskFilterView(callTasksVM.Filter);
			hboxTasksFilter.Add(callTaskFilterView);
		}

		public void UpdateStatistics()
		{
			var statistics = callTasksVM.GetStatistics(callTasksVM.Filter.Employee);

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
			callTaskFilterView.Visible = false;
		}

		protected void OnRadiobuttonShowFilterClicked(object sender, EventArgs e)
		{
			callTaskFilterView.Visible = !callTaskFilterView.Visible;
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

		protected void OnAssignedEmployeeButtonClicked(object sender, EventArgs e) => representationentryEmployee.OpenSelectDialog("Ответственный :");

		protected void OnRepresentationentryEmployeeChangedByUser(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.AssignedEmployee = representationentryEmployee.Subject as Employee);
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

			var selectedObjects = representationtreeviewTask.GetSelectedObjects();
			if(selectedObjects == null || !selectedObjects.Any()) {
				return;
			}
			var selectedObj = selectedObjects[0];
			var selectedNodeId = (selectedObj as CallTaskVMNode)?.Id;
			if(selectedNodeId == null)
				return;

			Menu popup = new Menu();
			var popupItems = callTasksVM.PopupItems;
			foreach(var popupItem in popupItems) {
				var menuItem = new MenuItem(popupItem.Title) {
					Sensitive = popupItem.SensitivityFunc.Invoke(selectedObjects)
				};
				menuItem.Activated += (sender, e) => { popupItem.ExecuteAction.Invoke(selectedObjects); };
				popup.Add(menuItem);
			}

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