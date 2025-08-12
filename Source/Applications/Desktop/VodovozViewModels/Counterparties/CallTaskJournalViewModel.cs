using ClosedXML.Excel;
using FluentNHibernate.Data;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Report.ViewModels;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.ReportsParameters;
using static Vodovoz.ViewModels.Counterparties.CallTaskFilterViewModel;
using static Vodovoz.ViewModels.Counterparties.CallTaskJournalViewModel;

namespace Vodovoz.ViewModels.Counterparties
{
	public partial class CallTaskJournalViewModel : EntityJournalViewModelBase<CallTask, CallTaskViewModel, CallTaskJournalNode>
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly CallTaskFilterViewModel _filterViewModel;

		public CallTaskJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			IFileDialogService fileDialogService,
			CallTaskFilterViewModel filterViewModel,
			Action<CallTaskFilterViewModel> filterConfigurationAction = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_fileDialogService = fileDialogService
				?? throw new ArgumentNullException(nameof(fileDialogService));
			_filterViewModel = filterViewModel
				?? throw new ArgumentNullException(nameof(filterViewModel));

			if(filterConfigurationAction != null)
			{
				_filterViewModel.ConfigureWithoutFiltering(filterConfigurationAction);
			}

			JournalFilter = _filterViewModel;

			_filterViewModel.OnFiltered += OnFilterRefiltered;

			DataLoader.ItemsListUpdated += OnDataLoaderItemsListUpdated;
			CreatePopupActions();

			SelectionMode = JournalSelectionMode.Multiple;
		}

		public override string FooterInfo { get; set; }

		protected override Func<IUnitOfWork, int> ItemsCountFunction => unitOfWork => ItemsQuery(unitOfWork).List<CallTaskJournalNode>().Count;

		protected override IQueryOver<CallTask> ItemsQuery(IUnitOfWork uow)
		{
			CallTask callTaskAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			Counterparty counterpartyAlias = null;
			Phone counterpartyPhoneAlias = null;
			Phone deliveryPointPhoneAlias = null;
			Employee employeeAlias = null;
			BottlesMovementOperation bottlesMovementOperationAlias = null;

			CallTaskJournalNode resultAlias = null;

			var tasksQuery = uow.Session.QueryOver(() => callTaskAlias)
				.Left.JoinAlias(() => callTaskAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias);

			var latestEndDateTime = _filterViewModel.EndDate.Date
				.AddHours(23)
				.AddMinutes(59)
				.AddSeconds(59);

			switch(_filterViewModel.DateType)
			{
				case TaskFilterDateType.CreationTime:
					tasksQuery
						.Where(x => x.CreationDate >= _filterViewModel.StartDate.Date)
						.And(x => x.CreationDate <= latestEndDateTime);
					break;
				case TaskFilterDateType.CompleteTaskDate:
					tasksQuery
						.Where(x => x.CompleteDate >= _filterViewModel.StartDate.Date)
						.And(x => x.CompleteDate <= latestEndDateTime);
					break;
				default:
					tasksQuery
						.Where(x => x.EndActivePeriod >= _filterViewModel.StartDate.Date)
						.And(x => x.EndActivePeriod <= latestEndDateTime);
					break;
			}

			if(_filterViewModel.Employee != null)
			{
				tasksQuery.Where(x => x.AssignedEmployee == _filterViewModel.Employee);
			}
			else if(_filterViewModel.ShowOnlyWithoutEmployee)
			{
				tasksQuery.Where(x => x.AssignedEmployee == null);
			}

			if(_filterViewModel.HideCompleted)
			{
				tasksQuery.Where(x => !x.IsTaskComplete);
			}

			if(_filterViewModel.DeliveryPointCategory != null)
			{
				tasksQuery.Where(() => deliveryPointAlias.Category == _filterViewModel.DeliveryPointCategory);
			}

			if(_filterViewModel.GeographicGroup != null)
			{
				tasksQuery.Where(() => districtAlias.GeographicGroup.Id == _filterViewModel.GeographicGroup.Id);
			}

			var bottleDebtByAddressQuery = uow.Session
				.QueryOver(() => bottlesMovementOperationAlias)
				.Where(() => bottlesMovementOperationAlias.Counterparty.Id == counterpartyAlias.Id)
				.And(() => bottlesMovementOperationAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
				.Select(
					Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
						NHibernateUtil.Int32, new IProjection[]
						{
							Projections.Sum(() => bottlesMovementOperationAlias.Returned),
							Projections.Sum(() => bottlesMovementOperationAlias.Delivered)
						}));

			var bottleDebtByClientQuery = uow.Session
				.QueryOver(() => bottlesMovementOperationAlias)
				.Where(() => bottlesMovementOperationAlias.Counterparty.Id == counterpartyAlias.Id)
				.Select(
					Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
						NHibernateUtil.Int32, new IProjection[]
						{
							Projections.Sum(() => bottlesMovementOperationAlias.Returned),
							Projections.Sum(() => bottlesMovementOperationAlias.Delivered)
						}));

			tasksQuery
				.Left.JoinAlias(
					() => deliveryPointAlias.Phones,
					() => deliveryPointPhoneAlias,
					() => !deliveryPointPhoneAlias.IsArchive)
				.Left.JoinAlias(
					() => counterpartyAlias.Phones,
					() => counterpartyPhoneAlias,
					() => !counterpartyPhoneAlias.IsArchive)
				.Left.JoinAlias(() => callTaskAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => callTaskAlias.AssignedEmployee, () => employeeAlias)
				.SelectList(list => list
					.SelectGroup(() => callTaskAlias.Id)
					.Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.AddressName)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.ClientName)
					.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmployeeName)
					.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmployeeLastName)
					.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmployeePatronymic)
					.Select(() => callTaskAlias.EndActivePeriod).WithAlias(() => resultAlias.Deadline)
					.Select(() => callTaskAlias.CreationDate).WithAlias(() => resultAlias.CreationDate)
					.Select(() => callTaskAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => callTaskAlias.TaskState).WithAlias(() => resultAlias.TaskStatus)
					.Select(() => callTaskAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => callTaskAlias.ImportanceDegree).WithAlias(() => resultAlias.ImportanceDegree)
					.Select(() => callTaskAlias.IsTaskComplete).WithAlias(() => resultAlias.IsTaskComplete)
					.Select(() => callTaskAlias.TareReturn).WithAlias(() => resultAlias.TareReturn)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( CONCAT(?2 , ?1) SEPARATOR ?3 )"),
						NHibernateUtil.String,
						Projections.Property(() => deliveryPointPhoneAlias.DigitsNumber),
						Projections.Constant("+7"),
						Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.DeliveryPointPhones)
				   .Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( CONCAT(?2 , ?1) SEPARATOR ?3 )"),
						NHibernateUtil.String,
						Projections.Property(() => counterpartyPhoneAlias.DigitsNumber),
						Projections.Constant("+7"),
						Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.CounterpartyPhones)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByAddressQuery).WithAlias(() => resultAlias.DebtByAddress)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByClientQuery).WithAlias(() => resultAlias.DebtByClient));

			tasksQuery.Where(GetSearchCriterion(
				() => callTaskAlias.Id,
				() => counterpartyAlias.Name,
				() => callTaskAlias.TaskState,
				() => deliveryPointAlias.ShortAddress));

			IProjection GetSortResultExpression()
			{
				switch(_filterViewModel.SortingParam)
				{
					case SortingParamType.DebtByAddress:
						return Projections.SubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByAddressQuery);
					case SortingParamType.DebtByClient:
						return Projections.SubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByClientQuery);
					case SortingParamType.AssignedEmployee:
						return Projections.Property(() => employeeAlias.Name);
					case SortingParamType.Client:
						return Projections.Property(() => counterpartyAlias.Name);
					case SortingParamType.Deadline:
						return Projections.Property(() => callTaskAlias.EndActivePeriod);
					case SortingParamType.DeliveryPoint:
						return Projections.Property(() => deliveryPointAlias.ShortAddress);
					case SortingParamType.ImportanceDegree:
						return Projections.Property(() => callTaskAlias.ImportanceDegree);
					case SortingParamType.Status:
						return Projections.Property(() => callTaskAlias.TaskState);
					case SortingParamType.Id:
					default:
						return Projections.Property(() => callTaskAlias.Id);
				}
			}

			var spec = GetSortResultExpression();

			if(_filterViewModel.SortingDirection == SortingDirectionType.FromBiggerToSmaller)
			{
				return tasksQuery
					.TransformUsing(Transformers.AliasToBean<CallTaskJournalNode>())
					.OrderBy(spec).Desc();
			}

			return tasksQuery
				.TransformUsing(Transformers.AliasToBean<CallTaskJournalNode>())
				.OrderBy(spec).Asc();
		}

		private void OnFilterRefiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		private void OnDataLoaderItemsListUpdated(object sender, EventArgs e)
		{
			FooterInfo = GetSummary();
		}

		private void ExportTasks()
		{
			var fileName = $"{TabName} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

			using(var wb = new XLWorkbook())
			{
				var sheetName = $"{DateTime.Now:dd.MM.yyyy}";
				var ws = wb.Worksheets.Add(sheetName);

				InsertValues(ws);

				if(TryGetSavePath(fileName, out string path))
				{
					wb.SaveAs(path);
				}
			}
		}

		private void InsertValues(IXLWorksheet ws)
		{
			var colName = new string[]
			{
				"№",
				"Номер \nзадачи",
				"Статус",
				"Клиент",
				"Дата созадния",
				"Адрес",
				"Долг \nпо адресу",
				"Долг \nпо клиенту",
				"Телефон адреса",
				"Телефон клиента",
				"Ответственный",
				"Выполнить до",
				"Срочность",
				"Комментарий"
			};

			var items = ItemsQuery(UoW).List<CallTaskJournalNode>();

			var index = 0;
			var rows = from row in items
					   select new
					   {
						   ind = index++,
						   row.Id,
						   TaskStatus = row.TaskStatus.GetEnumTitle(),
						   row.ClientName,
						   CreationDate = $"{row.CreationDate:dd.MM.yyyy HH:mm}",
						   row.AddressName,
						   row.DebtByAddress,
						   row.DebtByClient,
						   DeliveryPointPhones = row.DeliveryPointPhones?.Replace("\n", ",\n"),
						   CounterpartyPhones = row.CounterpartyPhones?.Replace("\n", ",\n"),
						   row.AssignedEmployeeName,
						   Deadline = $"{row.Deadline:dd.MM.yyyy HH:mm}",
						   ImportanceDegree = row.ImportanceDegree.GetEnumTitle(),
						   Comment = row.Comment?.Trim()
					   };

			for(int i = 0; i < colName.Length; i++)
			{
				ws.Cell(1, i + 1).Value = colName[i];
			}

			ws.Cell(2, 1).InsertData(rows);
			ws.Columns(1, colName.Length - 1).AdjustToContents();
			ws.Column(colName.Length).Width = 60;
			ws.Column(colName.Length).Style.Alignment.WrapText = true;

			var indexDPPhones = Array.IndexOf(colName, "Телефон адреса") + 1;
			var indexCPhones = Array.IndexOf(colName, "Телефон клиента") + 1;
			ws.Column(indexDPPhones).Style.Alignment.WrapText = true;
			ws.Column(indexDPPhones).SetDataType(XLDataType.Text);
			ws.Column(indexCPhones).Style.Alignment.WrapText = true;
			ws.Column(indexCPhones).SetDataType(XLDataType.Text);

			ws.Columns().AddVerticalPageBreaks();
		}

		private bool TryGetSavePath(string fileName, out string path)
		{
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = fileName
			};

			dialogSettings.FileFilters.Add(new DialogFileFilter("Excel", ".xlsx"));

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			path = result.Path;

			return result.Successful;
		}

		public string GetSummary()
		{
			var employee = _filterViewModel.Employee;

			var statisticsParam = new Dictionary<string, int>();

			if(!(_filterViewModel is CallTaskFilterViewModel taskFilter))
			{
				return string.Empty;
			}

			DateTime start = taskFilter.StartDate.Date;
			DateTime end = taskFilter.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
			CallTask tasksAlias = null;

			var baseQuery = UoW.Session.QueryOver(() => tasksAlias)
				.Where(() => tasksAlias.CompleteDate >= start)
				.And(() => tasksAlias.CompleteDate <= end);

			if(employee != null)
			{
				baseQuery.And(() => tasksAlias.AssignedEmployee.Id == employee.Id);
			}

			var callTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.Call);
			statisticsParam.Add("Звонков", callTaskQuery.RowCount());

			var difTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.DifficultClient);
			statisticsParam.Add("Сложных клиентов", difTaskQuery.RowCount());

			var jobTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.Task);
			statisticsParam.Add("Заданий", difTaskQuery.RowCount());

			var allNodes = ItemsQuery(UoW).List<CallTaskJournalNode>();

			statisticsParam.Add("Кол-во задач", allNodes.Count);

			statisticsParam.Add("Тара на забор", allNodes.Sum(x => x.TareReturn));

			return string.Join(" | ", statisticsParam.Select(pair => $"{pair.Key} : {pair.Value}"));
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();

			bool canCreate = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(CallTask)).CanCreate;
			bool canEdit = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(CallTask)).CanUpdate;
			bool canDelete = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(CallTask)).CanDelete;

			var addAction = new JournalAction("Добавить",
					(selected) => canCreate,
					(selected) => VisibleCreateAction,
					(selected) => CreateEntityDialog(),
					"Insert"
					);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
					(selected) => canEdit && selected.Any(),
					(selected) => VisibleEditAction,
					(selected) => selected.Cast<CallTaskJournalNode>().ToList().ForEach(EditEntityDialog)
					);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			var deleteAction = new JournalAction("Удалить",
					(selected) => canDelete && selected.Any(),
					(selected) => VisibleDeleteAction,
					(selected) => DeleteEntities(selected.Cast<CallTaskJournalNode>().ToArray()),
					"Delete"
					);
			NodeActionsList.Add(deleteAction);

			NodeActionsList.Add(new JournalAction(
				"Экспорт",
				nodes => true,
				nodes => true,
				nodes => ExportTasks()));

			NodeActionsList.Add(new JournalAction(
				"Массовое редактирование",
				nodes => CurrentPermissionService.ValidateEntityPermission(typeof(CallTask)).CanUpdate
					&& nodes.Count() > 1,
				nodes => true,
				nodes =>
				{
					NavigationManager.OpenViewModel<CallTaskMassEditViewModel>(
						this,
						OpenPageOptions.AsSlave,
						viewModel => viewModel
							.AddTasks(nodes
								.Cast<CallTaskJournalNode>()
								.Select(ct => ct.Id)));
				}));
		}

		protected override void CreatePopupActions()
		{
			PopupActionsList.Clear();
			PopupActionsList.Add(new JournalAction(
				"Отметить как важное",
				nodes => nodes.Count() == 1,
				_ => true,
				selectedItems =>
				{
					var selectedNodes = selectedItems.Cast<CallTaskJournalNode>();
					ChangeEnitity((task) => task.ImportanceDegree = ImportanceDegreeType.Important, selectedNodes.ToArray());
					foreach(var selectedNode in selectedNodes)
					{
						selectedNode.ImportanceDegree = ImportanceDegreeType.Important;
					}
				}));
			PopupActionsList.Add(new JournalAction(
				"Открыть акт сверки взаиморасчетов",
				nodes => nodes.Count() == 1,
				_ => true,
				selectedItems =>
				{
					var selectedNodes = selectedItems.Cast<CallTaskJournalNode>();

					var callTaskId = selectedNodes.SingleOrDefault().Id;

					NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(
							this,
							typeof(RevisionReportViewModel),
							OpenPageOptions.AsSlave,
							vm =>
							{
								if(vm.ReportParametersViewModel is RevisionReportViewModel reportVm)
								{
									if(reportVm.Counterparty == null)
									{
										var counterparty = reportVm
											.UnitOfWork
											.GetById<CallTask>(callTaskId)
											.Counterparty;
										reportVm.Counterparty = counterparty;
									}
								}
							});
				}));
		}

		public void ChangeEnitity(Action<CallTask> action, CallTaskJournalNode[] taskNodes)
		{
			if(action == null)
			{
				return;
			}

			var ids = taskNodes.Select(x => x.Id).ToArray();
			var tasks = UoW.GetById<CallTask>(ids);

			tasks.ToList().ForEach(task =>
			{
				action(task);
				UoW.Save(task);
			});

			UoW.Commit();
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterRefiltered;
			DataLoader.ItemsListUpdated -= OnDataLoaderItemsListUpdated;
			base.Dispose();
		}
	}
}
