using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using Vodovoz.Domain.Contacts;
using QS.RepresentationModel.GtkUI;
using QS.Utilities.Text;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Sale;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Services;
using ClosedXML.Excel;
using QS.Project.Services.FileDialog;
using Gamma.Utilities;
using WrapMode = Pango.WrapMode;
using Vodovoz.Infrastructure;

namespace Vodovoz.Representations
{
	public class CallTasksVM : QSOrmProject.RepresentationModel.RepresentationModelEntityBase<CallTask, CallTaskVMNode>
	{
		private static readonly Pixbuf _emptyImg = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.empty16.png");
		private static readonly Pixbuf _fire = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.fire16.png");
		private readonly IFileDialogService _fileDialogService;

		private CallTaskFilterViewModel filter;
		public CallTaskFilterViewModel Filter
		{
			get => filter;
			set
			{
				if(filter != value)
				{
					filter = value;
					filter.OnFiltered += (sender, e) => UpdateNodes();
					PropertyChanged?.Invoke(this, EventArgs.Empty);
				}
			}
		}

		public bool NeedUpdate { get; set; }

		protected override bool NeedUpdateFunc(CallTask updatedSubject) => NeedUpdate;

		public event EventHandler PropertyChanged;

		public override IColumnsConfig ColumnsConfig => FluentColumnsConfig<CallTaskVMNode>.Create()
			.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Срочность").AddPixbufRenderer(node => node.ImportanceDegree == ImportanceDegreeType.Important && !node.IsTaskComplete ? _fire : _emptyImg)
			.AddColumn("Статус").AddEnumRenderer(node => node.TaskStatus)
			.AddColumn("Клиент").AddTextRenderer(node => node.ClientName ?? String.Empty).WrapWidth(500).WrapMode(WrapMode.WordChar)
			.AddColumn("Адрес").AddTextRenderer(node => node.AddressName ?? "Самовывоз").WrapWidth(500).WrapMode(WrapMode.WordChar)
			.AddColumn("Долг по адресу").AddTextRenderer(node => node.DebtByAddress.ToString()).XAlign(0.5f)
			.AddColumn("Долг по клиенту").AddTextRenderer(node => node.DebtByClient.ToString()).XAlign(0.5f)
			.AddColumn("Телефоны").AddTextRenderer(node => node.DeliveryPointPhones == "+7" ? String.Empty : node.DeliveryPointPhones)
				.WrapMode(Pango.WrapMode.WordChar)
			.AddColumn("Ответственный").AddTextRenderer(node => node.AssignedEmployeeName ?? String.Empty)
			.AddColumn("Выполнить до").AddTextRenderer(node => node.Deadline.ToString("dd / MM / yyyy  HH:mm"))
			.RowCells().AddSetter<CellRendererText>((c, n) =>
			{
				var color = GdkColors.PrimaryText;

				if(n.IsTaskComplete)
				{
					color = GdkColors.SuccessText;
				}

				if(DateTime.Now > n.Deadline)
				{
					color = GdkColors.DangerText;
				}

				c.ForegroundGdk = color;
			})
			.Finish();

		public CallTasksVM(IImageProvider imageProvider, IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService;
		}

		public override void UpdateNodes()
		{
			DeliveryPoint deliveryPointAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
			CallTask callTaskAlias = null;
			CallTaskVMNode resultAlias = null;
			Counterparty counterpartyAlias = null;
			Employee employeeAlias = null;
			Phone deliveryPointPhonesAlias = null;
			Phone counterpartyPhonesAlias = null;
			Domain.Orders.Order orderAlias = null;
			District districtAlias = null;

			var tasksQuery = UoW.Session.QueryOver(() => callTaskAlias)
						.Left.JoinAlias(() => callTaskAlias.DeliveryPoint, () => deliveryPointAlias)
						.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias);

			switch(Filter.DateType)
			{
				case TaskFilterDateType.CreationTime:
					tasksQuery.Where(x => x.CreationDate >= Filter.StartDate.Date)
							  .And(x => x.CreationDate <= Filter.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
				case TaskFilterDateType.CompleteTaskDate:
					tasksQuery.Where(x => x.CompleteDate >= Filter.StartDate.Date)
							  .And(x => x.CompleteDate <= Filter.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
				default:
					tasksQuery.Where(x => x.EndActivePeriod >= Filter.StartDate.Date)
							  .And(x => x.EndActivePeriod <= Filter.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
					break;
			}

			if(Filter.Employee != null)
				tasksQuery.Where(x => x.AssignedEmployee == Filter.Employee);
			else if(Filter.ShowOnlyWithoutEmployee)
				tasksQuery.Where(x => x.AssignedEmployee == null);

			if(Filter.HideCompleted)
				tasksQuery.Where(x => !x.IsTaskComplete);

			if(Filter.DeliveryPointCategory != null)
				tasksQuery.Where(() => deliveryPointAlias.Category == Filter.DeliveryPointCategory);

			if(Filter.GeographicGroup != null)
			{
				tasksQuery.Where(() => districtAlias.GeographicGroup.Id == Filter.GeographicGroup.Id);
			}

			var bottleDebtByAddressQuery = UoW.Session.QueryOver(() => bottlesMovementAlias)
			.JoinAlias(() => bottlesMovementAlias.Order, () => orderAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
			.And(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id || orderAlias.SelfDelivery)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			var bottleDebtByClientQuery = UoW.Session.QueryOver(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.Counterparty.Id == counterpartyAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered) }
				));

			var tasks = tasksQuery
			.Left.JoinAlias(() => deliveryPointAlias.Phones, () => deliveryPointPhonesAlias, () => !deliveryPointPhonesAlias.IsArchive)
			.Left.JoinAlias(() => counterpartyAlias.Phones, () => counterpartyPhonesAlias, () => !counterpartyPhonesAlias.IsArchive)
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
					   Projections.Property(() => deliveryPointPhonesAlias.DigitsNumber),
					   Projections.Constant("+7"),
					   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.DeliveryPointPhones)
				   .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( CONCAT(?2 , ?1) SEPARATOR ?3 )"),
					   NHibernateUtil.String,
					   Projections.Property(() => counterpartyPhonesAlias.DigitsNumber),
					   Projections.Constant("+7"),
					   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.CounterpartyPhones)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByAddressQuery).WithAlias(() => resultAlias.DebtByAddress)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByClientQuery).WithAlias(() => resultAlias.DebtByClient)
				)
			.TransformUsing(Transformers.AliasToBean<CallTaskVMNode>())
			.List<CallTaskVMNode>();
			tasks = SortResult(tasks).ToList();
			SetItemsSource(tasks);
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
			var rows = from row in ItemsList as List<CallTaskVMNode>
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

		private IEnumerable<CallTaskVMNode> SortResult(IEnumerable<CallTaskVMNode> tasks)
		{
			IEnumerable<CallTaskVMNode> result;
			switch(Filter.SortingParam)
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
			if(Filter.SortingDirection == SortingDirectionType.FromBiggerToSmaller)
				result = result.Reverse();
			return result;
		}

		public Dictionary<string, int> GetStatistics(Employee employee = null)
		{
			var statisticsParam = new Dictionary<string, int>();

			if(!(Filter is CallTaskFilterViewModel taskFilter))
				return statisticsParam;

			DateTime start = taskFilter.StartDate.Date;
			DateTime end = taskFilter.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
			CallTask tasksAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var baseQuery = UoW.Session.QueryOver(() => tasksAlias)
				.Where(() => tasksAlias.CompleteDate >= start)
				.And(() => tasksAlias.CompleteDate <= end);

			if(employee != null)
				baseQuery.And(() => tasksAlias.AssignedEmployee.Id == employee.Id);

			var callTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.Call);
			statisticsParam.Add("Звонков : ", callTaskQuery.RowCount());

			var difTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.DifficultClient);
			statisticsParam.Add("Сложных клиентов : ", difTaskQuery.RowCount());

			var jobTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.Task);
			statisticsParam.Add("Заданий : ", difTaskQuery.RowCount());

			statisticsParam.Add("Кол-во задач : ", ItemsList.Count);

			statisticsParam.Add("Тара на забор : ", ItemsList.OfType<CallTaskVMNode>().Sum(x => x.TareReturn));

			return statisticsParam;
		}

		public override IEnumerable<IJournalPopupItem> PopupItems
		{
			get
			{
				var result = new List<IJournalPopupItem>();

				result.Add(JournalPopupItemFactory.CreateNewAlwaysSensitiveAndVisible("Отметить как важное",
					(selectedItems) =>
					{
						var selectedNodes = selectedItems.Cast<CallTaskVMNode>();
						ChangeEnitity((task) => task.ImportanceDegree = ImportanceDegreeType.Important, selectedNodes.ToArray(), false);
						foreach(var selectedNode in selectedNodes)
						{
							selectedNode.ImportanceDegree = ImportanceDegreeType.Important;
						}
					}
				));

				return result;
			}
		}

		public void ChangeEnitity(Action<CallTask> action, CallTaskVMNode[] taskNodes, bool NeedUpdate = true)
		{
			if(action == null)
				return;

			var ids = taskNodes.Select(x => x.Id).ToArray();
			var tasks = UoW.GetById<CallTask>(ids);

			tasks.ToList().ForEach(task =>
			{
				action(task);
				UoW.Save(task);
			});

			UoW.Commit();

			if(NeedUpdate)
				UpdateNodes();
		}
	}

	public class CallTaskVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public CallTaskStatus TaskStatus { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string ClientName { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string AddressName { get; set; }

		public int DebtByAddress { get; set; }

		public int DebtByClient { get; set; }

		public string DeliveryPointPhones { get; set; }

		public string CounterpartyPhones { get; set; }

		public string Phones { get { return String.IsNullOrWhiteSpace(DeliveryPointPhones) ? CounterpartyPhones : DeliveryPointPhones; } }

		public string EmployeeLastName { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }

		public string AssignedEmployeeName => PersonHelper.PersonNameWithInitials(EmployeeLastName, EmployeeName, EmployeePatronymic);

		public DateTime Deadline { get; set; }

		public DateTime CreationDate { get; set; }

		public ImportanceDegreeType ImportanceDegree { get; set; }

		public bool IsTaskComplete { get; set; }

		public int TareReturn { get; set; }

		public string Comment { get; set; }
	}
}
