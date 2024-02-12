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
using Vodovoz.Filters.ViewModels;
using CallTaskFilterView = Vodovoz.Filters.GtkViews.CallTaskFilterView;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using QS.Dialog.GtkUI.FileDialog;
using QS.Extensions;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TasksView : SingleUowTabBase
	{
		private CallTasksVM _callTasksVm;
		private CallTaskFilterView _callTaskFilterView;

		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IDeliveryPointRepository _deliveryPointRepository;

		public TasksView(
			IEmployeeJournalFactory employeeJournalFactory,
			IDeliveryPointRepository deliveryPointRepository)
		{
			Build();

			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_deliveryPointRepository = deliveryPointRepository ?? throw new ArgumentNullException(nameof(deliveryPointRepository));

			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			TabName = "Журнал задач для обзвона";
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			evmeEmployee.SetEntityAutocompleteSelectorFactory(_employeeJournalFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeEmployee.ChangedByUser += OnEvmeEmployeeChangedByUser;
			taskStatusComboBox.ItemsEnum = typeof(CallTaskStatus);
			representationtreeviewTask.Selection.Mode = SelectionMode.Multiple;
			_callTasksVm = new CallTasksVM(new BaseParametersProvider(new ParametersProvider()), new FileDialogService());
			_callTasksVm.NeedUpdate = ycheckbuttonAutoUpdate.Active;
			_callTasksVm.ItemsListUpdated += (sender, e) => UpdateStatistics();
			_callTasksVm.Filter =
				new CallTaskFilterViewModel(_employeeJournalFactory, _deliveryPointRepository);
			_callTasksVm.PropertyChanged += ReCreateCallTaskFilterView;
			representationtreeviewTask.RepresentationModel = _callTasksVm;
			buttonExport.Clicked += (sender, args) => Export();
			CreateCallTaskFilterView();
			UpdateStatistics();
		}

		private void ReCreateCallTaskFilterView(object sender, EventArgs e)
		{
			_callTaskFilterView?.Destroy();
			CreateCallTaskFilterView();
		}
		
		private void CreateCallTaskFilterView()
		{
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

		private void Export()
		{
			var fileName = $"{TabName} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

			_callTasksVm.ExportTasks(fileName);

		}

		#region BaseJournalHeandler

		protected void OnAddTaskButtonClicked(object sender, EventArgs e)
		{
			CallTaskDlg dlg = new CallTaskDlg();
			TabParent.AddTab(dlg, this);
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
			evmeEmployee.OpenSelectDialog("Ответственный :");

		void OnEvmeEmployeeChangedByUser(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.AssignedEmployee = evmeEmployee.Subject as Employee);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			_callTasksVm.ChangeEnitity(action, tasks, false);
		}

		protected void OnCompleteTaskButtonClicked(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.IsTaskComplete = true);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			_callTasksVm.ChangeEnitity(action, tasks, false);
		}

		protected void OnTaskstateButtonEnumItemClicked(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.TaskState = (CallTaskStatus)taskStatusComboBox.SelectedItem);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			_callTasksVm.ChangeEnitity(action, tasks, false);
		}

		protected void OnDatepickerDeadlineChangeDateChangedByUser(object sender, EventArgs e)
		{
			Action<CallTask> action = ((task) => task.EndActivePeriod = datepickerDeadlineChange.Date);
			var tasks = representationtreeviewTask.GetSelectedObjects().OfType<CallTaskVMNode>().ToArray();
			_callTasksVm.ChangeEnitity(action, tasks, false);
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

		public override void Destroy()
		{
			DisposePixbufs();
			
			taskStatusComboBox.Destroy();
			_callTaskFilterView?.Destroy();
			hboxStatistics.Children.OfType<Widget>().ToList().ForEach(x => x.Destroy());
			base.Destroy();
		}

		private void DisposePixbufs()
		{
			var addImage = buttonAdd.Image as Image;
			addImage.DisposeImagePixbuf();
			var editImage = buttonEdit.Image as Image;
			editImage.DisposeImagePixbuf();
			var deleteImage = buttonDelete.Image as Image;
			deleteImage.DisposeImagePixbuf();
			var refreshImage = buttonRefresh.Image as Image;
			refreshImage.DisposeImagePixbuf();
			var completedTasksImage = buttonCompleteSelected.Image as Image;
			completedTasksImage.DisposeImagePixbuf();
		}
	}
}
