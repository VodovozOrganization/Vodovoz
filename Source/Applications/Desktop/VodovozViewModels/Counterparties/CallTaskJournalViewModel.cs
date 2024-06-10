using ClosedXML.Excel;
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
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Sale;
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
			_fileDialogService = fileDialogService;
			_filterViewModel = filterViewModel
				?? throw new ArgumentNullException(nameof(filterViewModel));

			if(filterConfigurationAction != null)
			{
				_filterViewModel.SetAndRefilterAtOnce(filterConfigurationAction);
			}

			JournalFilter = _filterViewModel;

			_filterViewModel.OnFiltered += OnFilterRefiltered;
		}

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

			switch(_filterViewModel.DateType)
			{
				case TaskFilterDateType.CreationTime:
					tasksQuery.Where(x => x.CreationDate >= _filterViewModel.StartDate.Date)
							  .And(x => x.CreationDate <= _filterViewModel.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
				case TaskFilterDateType.CompleteTaskDate:
					tasksQuery.Where(x => x.CompleteDate >= _filterViewModel.StartDate.Date)
							  .And(x => x.CompleteDate <= _filterViewModel.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
				default:
					tasksQuery.Where(x => x.EndActivePeriod >= _filterViewModel.StartDate.Date)
							  .And(x => x.EndActivePeriod <= _filterViewModel.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
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

			var bottleDebtByAddressQuery = uow.Session.QueryOver(() => bottlesMovementOperationAlias)
			.Where(() => bottlesMovementOperationAlias.Counterparty.Id == counterpartyAlias.Id)
			.And(() => bottlesMovementOperationAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementOperationAlias.Returned),
								Projections.Sum(() => bottlesMovementOperationAlias.Delivered)}
				));

			var bottleDebtByClientQuery = uow.Session.QueryOver(() => bottlesMovementOperationAlias)
			.Where(() => bottlesMovementOperationAlias.Counterparty.Id == counterpartyAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementOperationAlias.Returned),
								Projections.Sum(() => bottlesMovementOperationAlias.Delivered) }
				));

			return tasksQuery
				.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhoneAlias, () => !deliveryPointPhoneAlias.IsArchive)
				.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhoneAlias, () => !counterpartyPhoneAlias.IsArchive)
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
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByClientQuery).WithAlias(() => resultAlias.DebtByClient)
				)
			.TransformUsing(Transformers.AliasToBean<CallTaskJournalNode>());
		}

		private IEnumerable<CallTaskJournalNode> SortResult(IEnumerable<CallTaskJournalNode> tasks)
		{
			IEnumerable<CallTaskJournalNode> result;

			switch(_filterViewModel.SortingParam)
			{
				case SortingParamType.DebtByAddress:
					result = tasks.OrderBy(x => x.DebtByAddress);
					break;
				case SortingParamType.DebtByClient:
					result = tasks.OrderBy(x => x.DebtByClient);
					break;
				case SortingParamType.AssignedEmployee:
					result = tasks.OrderBy(x => x.AssignedEmployeeName);
					break;
				case SortingParamType.Client:
					result = tasks.OrderBy(x => x.ClientName);
					break;
				case SortingParamType.Deadline:
					result = tasks.OrderBy(x => x.Deadline);
					break;
				case SortingParamType.DeliveryPoint:
					result = tasks.OrderBy(x => x.AddressName);
					break;
				case SortingParamType.Id:
					result = tasks.OrderBy(x => x.Id);
					break;
				case SortingParamType.ImportanceDegree:
					result = tasks.OrderBy(x => x.ImportanceDegree);
					break;
				case SortingParamType.Status:
					result = tasks.OrderBy(x => x.TaskStatus);
					break;
				default:
					throw new NotImplementedException();
			}
			if(_filterViewModel.SortingDirection == SortingDirectionType.FromBiggerToSmaller)
			{
				result = result.Reverse();
			}

			return result;
		}

		private void OnFilterRefiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		internal void ExportTasks(string fileName)
		{
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
			var colName = new string[] { "№", "Номер \nзадачи", "Статус", "Клиент", "Дата созадния", "Адрес", "Долг \nпо адресу", "Долг \nпо клиенту", "Телефон адреса", "Телефон клиента", "Ответственный", "Выполнить до", "Срочность", "Комментарий" };
			var index = 0;
			var rows = from row in Items as List<CallTaskJournalNode>
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

		public Dictionary<string, int> GetStatistics(Employee employee = null)
		{
			var statisticsParam = new Dictionary<string, int>();

			if(!(_filterViewModel is CallTaskFilterViewModel taskFilter))
			{
				return statisticsParam;
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
			statisticsParam.Add("Звонков : ", callTaskQuery.RowCount());

			var difTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.DifficultClient);
			statisticsParam.Add("Сложных клиентов : ", difTaskQuery.RowCount());

			var jobTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.Task);
			statisticsParam.Add("Заданий : ", difTaskQuery.RowCount());

			statisticsParam.Add("Кол-во задач : ", Items.Count);

			statisticsParam.Add("Тара на забор : ", Items.OfType<CallTaskJournalNode>().Sum(x => x.TareReturn));

			return statisticsParam;
		}

		protected override void CreatePopupActions()
		{
			PopupActionsList.Clear();
			PopupActionsList.Add(new JournalAction("Отметить как важное",
				(_) => true,
				(_) => true,
				(selectedItems) =>
				{
					var selectedNodes = selectedItems.Cast<CallTaskJournalNode>();
					ChangeEnitity((task) => task.ImportanceDegree = ImportanceDegreeType.Important, selectedNodes.ToArray(), false);
					foreach(var selectedNode in selectedNodes)
					{
						selectedNode.ImportanceDegree = ImportanceDegreeType.Important;
					}
				}));
		}

		public void ChangeEnitity(Action<CallTask> action, CallTaskJournalNode[] taskNodes, bool NeedUpdate = true)
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

			if(NeedUpdate)
			{
				Refresh();
			}
		}
	}
}
