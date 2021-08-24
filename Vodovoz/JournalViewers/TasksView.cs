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
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TasksView : SingleUowTabBase
	{
		private CallTasksVM _callTasksVm;
		private CallTaskFilterView _callTaskFilterView;

		private readonly IEmployeeRepository _employeeRepository;
		private readonly IBottlesRepository _bottleRepository;
		private readonly ICallTaskRepository _callTaskRepository;
		private readonly IPhoneRepository _phoneRepository;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IDeliveryPointRepository _deliveryPointRepository;

		public TasksView(
			IEmployeeRepository employeeRepository,
			IBottlesRepository bottleRepository,
			ICallTaskRepository callTaskRepository,
			IPhoneRepository phoneRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			IDeliveryPointRepository deliveryPointRepository)
		{
			this.Build();

			_employeeRepository = employeeRepository;
			_bottleRepository = bottleRepository;
			_callTaskRepository = callTaskRepository;
			_phoneRepository = phoneRepository;
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));

			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.TabName = "Журнал задач для обзвона";
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			representationentryEmployee.RepresentationModel = new EmployeesVM(UoW);
			taskStatusComboBox.ItemsEnum = typeof(CallTaskStatus);
			representationtreeviewTask.Selection.Mode = SelectionMode.Multiple;
			_callTasksVm = new CallTasksVM(new BaseParametersProvider(new ParametersProvider()));
			_callTasksVm.NeedUpdate = ycheckbuttonAutoUpdate.Active;
			_callTasksVm.ItemsListUpdated += (sender, e) => UpdateStatistics();
			_callTasksVm.Filter =
				new CallTaskFilterViewModel(_employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory(), _deliveryPointRepository);
			_callTasksVm.PropertyChanged += CreateCallTaskFilterView;
			representationtreeviewTask.RepresentationModel = _callTasksVm;
			CreateCallTaskFilterView(_callTasksVm.Filter, EventArgs.Empty);
			UpdateStatistics();
		}

		private void CreateCallTaskFilterView(object sender, EventArgs e)
		{
			_callTaskFilterView?.Destroy();

			_callTaskFilterView = new CallTaskFilterView(_callTasksVm.Filter);
			hboxTasksFilter.Add(_callTaskFilterView);
		}

		private void UpdateStatistics()
		{
			var statistics = _callTasksVm.GetStatistics(_callTasksVm.Filter.Employee);

			hboxStatistics.Children.OfType<Widget>().ToList().ForEach(x => x.Destroy());

			foreach(var stats in statistics.Select(item => new Label(item.Key + item.Value)))
			{
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
			TabParent.AddTab(dlg, this);

			/*
			ClientTaskViewModel clientTaskViewModel = new ClientTaskViewModel(employeeRepository,
																				bottleRepository,
																				callTaskRepository,
																				phoneRepository,
																				EntityUoWBuilder.ForCreate(), 
																				UnitOfWorkFactory.GetDefaultFactory, 
																				ServicesConfig.CommonServices);
			TabParent.AddTab(clientTaskViewModel, this);
			*/
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			var selected = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().FirstOrDefault();
			if(selected == null)
			{
				return;
			}

			CallTaskDlg dlg = new CallTaskDlg(selected.Id);
			OpenSlaveTab(dlg);

			/*
			var selected = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().FirstOrDefault();

			if(selected == null)
				return;

			ClientTaskViewModel clientTaskViewModel = new ClientTaskViewModel(employeeRepository,
																				bottleRepository,
																				callTaskRepository,
																				phoneRepository,
																				EntityUoWBuilder.ForOpen(selected.Id),
																				UnitOfWorkFactory.GetDefaultFactory,
																				ServicesConfig.CommonServices);
			OpenSlaveTab(clientTaskViewModel);
			*/
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var selected = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>();
			foreach(var item in selected)
			{
				DeleteHelper.DeleteEntity(typeof(CallTask), item.Id);
			}

			_callTasksVm.UpdateNodes();
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e) => _callTasksVm.UpdateNodes();

		protected void OnRadiobuttonShowFilterToggled(object sender, EventArgs e)
		{
			hboxEditSelected.Visible = false;
		}

		protected void OnRadiobuttonEditSelectedToggled(object sender, EventArgs e)
		{
			_callTaskFilterView.Visible = false;
		}

		protected void OnRadiobuttonShowFilterClicked(object sender, EventArgs e)
		{
			_callTaskFilterView.Visible = !_callTaskFilterView.Visible;
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

		protected void OnAssignedEmployeeButtonClicked(object sender, EventArgs e) =>
			representationentryEmployee.OpenSelectDialog("Ответственный :");

		protected void OnRepresentationentryEmployeeChangedByUser(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.AssignedEmployee = representationentryEmployee.Subject as Employee);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			_callTasksVm.ChangeEnitity(action, tasks);
		}

		protected void OnCompleteTaskButtonClicked(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.IsTaskComplete = true);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			_callTasksVm.ChangeEnitity(action, tasks);
		}

		protected void OnTaskstateButtonEnumItemClicked(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.TaskState = (CallTaskStatus)taskStatusComboBox.SelectedItem);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			_callTasksVm.ChangeEnitity(action, tasks);
		}

		protected void OnDatepickerDeadlineChangeDateChangedByUser(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.EndActivePeriod = datepickerDeadlineChange.Date);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			_callTasksVm.ChangeEnitity(action, tasks);
			datepickerDeadlineChange.Clear();
		}

		protected void OnRepresentationtreeviewTaskButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(args.Event.Button != 3)
			{
				return;
			}

			var selectedObjects = representationtreeviewTask.GetSelectedObjects();
			if(selectedObjects == null || !selectedObjects.Any())
			{
				return;
			}

			var selectedObj = selectedObjects[0];
			var selectedNodeId = (selectedObj as CallTaskVMNode)?.Id;
			if(selectedNodeId == null)
			{
				return;
			}

			Menu popup = new Menu();
			var popupItems = _callTasksVm.PopupItems;
			foreach(var popupItem in popupItems)
			{
				var menuItem = new MenuItem(popupItem.Title)
				{
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
			_callTasksVm.NeedUpdate = ycheckbuttonAutoUpdate.Active;
		}

		#endregion
	}
}
