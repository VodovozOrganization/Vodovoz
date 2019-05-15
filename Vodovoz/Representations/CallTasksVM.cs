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
using QS.DomainModel.UoW;
using QS.Tools;
using QS.Utilities.Text;
using QSContacts;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using QSSupportLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.StoredResources;
using Vodovoz.Filters;
using Vodovoz.Services;

namespace Vodovoz.Representations
{
	public class CallTasksVM : RepresentationModelEntityBase<CallTask, CallTaskVMNode>
	{
		private readonly Pixbuf img; //TODO : переписать на множество вариантов
		private readonly Pixbuf emptyImg;

		private int taskCount = 0;

		public IQueryFilter Filter { get; set; }

		public override IColumnsConfig ColumnsConfig => FluentColumnsConfig<CallTaskVMNode>.Create()
			.AddColumn("№").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Срочность").AddPixbufRenderer(node => node.ImportanceDegree == ImportanceDegreeType.Important && !node.IsTaskComplete ? img : emptyImg)
			.AddColumn("Статус").AddEnumRenderer(node => node.TaskStatus)
			.AddColumn("Клиент").AddTextRenderer(node => node.ClientName ?? String.Empty)
			.AddColumn("Адрес").AddTextRenderer(node => node.AddressName ?? String.Empty)
			.AddColumn("Долг по адресу").AddTextRenderer(node => node.DebtByAddress.ToString()).XAlign(0.5f)
			.AddColumn("Долг по клиенту").AddTextRenderer(node => node.DebtByClient.ToString()).XAlign(0.5f)
			.AddColumn("Телефены").AddTextRenderer(node => node.Phones == "+7" ? String.Empty : node.Phones ).WrapMode(Pango.WrapMode.WordChar)
			.AddColumn("Ответственный").AddTextRenderer(node => node.AssignedEmployeeName ?? String.Empty)
			.AddColumn("Выполнить до").AddTextRenderer(node => node.Deadline.ToString("dd / MM / yyyy  HH:mm"))
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
			.Finish();

		public CallTasksVM(IImageProvider imageProvider)
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			img = imageProvider.GetCrmIndicator(UoW);
			emptyImg = img.Copy();
			emptyImg.Fill(0xffffffff);
		}

		public override void UpdateNodes()
		{
			DeliveryPoint deliveryPointAlias = null;
			BottlesMovementOperation bottlesMovementAlias = null;
			CallTask callTaskAlias = null;
			CallTaskVMNode resultAlias = null;
			Counterparty counterpartyAlias = null;
			Employee employeeAlias = null;
			Phone phonesAlias = null;
			var filterCriterion = Filter?.GetFilter();
			var tasksQuery = UoW.Session.QueryOver(() => callTaskAlias);

			if(filterCriterion != null)
				tasksQuery = tasksQuery.And(filterCriterion);

			var bottleDebtByAddressQuery = UoW.Session.QueryOver(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered)}
				));

			var bottleDebtByClientQuery = UoW.Session.QueryOver(() => bottlesMovementAlias)
			.Where(() => bottlesMovementAlias.Counterparty.Id == deliveryPointAlias.Counterparty.Id)
			.Select(
				Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?2 - ?1 )"),
					NHibernateUtil.Int32, new IProjection[] {
								Projections.Sum(() => bottlesMovementAlias.Returned),
								Projections.Sum(() => bottlesMovementAlias.Delivered) }
				));
				
			var tasks = tasksQuery
			.JoinAlias(() => callTaskAlias.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.JoinAlias(() => deliveryPointAlias.Phones , () => phonesAlias , NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.JoinAlias(() => callTaskAlias.Counterparty, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
			.JoinAlias(() => callTaskAlias.AssignedEmployee, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
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
				   .Select(() => callTaskAlias.ImportanceDegree).WithAlias(() => resultAlias.ImportanceDegree)
				   .Select(() => callTaskAlias.IsTaskComplete).WithAlias(() => resultAlias.IsTaskComplete)
				   .Select(() => callTaskAlias.TareReturn).WithAlias(() => resultAlias.TareReturn)
				   .Select(Projections.SqlFunction(
					   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( CONCAT(?2 , ?1) SEPARATOR ?3 )"),
					   NHibernateUtil.String,
					   Projections.Property(() => phonesAlias.DigitsNumber),
					   Projections.Constant("+7"),
					   Projections.Constant("\n"))
				   ).WithAlias(() => resultAlias.Phones)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByAddressQuery).WithAlias(() => resultAlias.DebtByAddress)
				   .SelectSubQuery((QueryOver<BottlesMovementOperation>)bottleDebtByClientQuery).WithAlias(() => resultAlias.DebtByClient)
				)
			.OrderBy(x => x.Counterparty.Id).Desc()
			.TransformUsing(Transformers.AliasToBean<CallTaskVMNode>())
			.List<CallTaskVMNode>();
			taskCount = tasks.Count;
			SetItemsSource(tasks);

		}

		public Dictionary<string, int> GetStatistics (Employee employee = null) //TODO : перенести в репозиторий + уростить вызов запросов 
		{
			var statisticsParam = new Dictionary<string, int>();

			if(!(Filter is CallTaskFilter taskFilter)) 
				return statisticsParam;

			DateTime start = taskFilter.StartDate.Date;
			DateTime end = taskFilter.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
			CallTask tasksAlias = null;

			var baseQuery = UoW.Session.QueryOver(() => tasksAlias)
				.Where(() => tasksAlias.CompleteDate >= start)
				.And(() => tasksAlias.CompleteDate <= end);
			if(employee != null)
				baseQuery.And(() => tasksAlias.AssignedEmployee.Id == employee.Id);

			var callTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.Call);
			statisticsParam.Add("Звонков : ", callTaskQuery.RowCount() );

			var difTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.DifficultClient);
			statisticsParam.Add("Сложных клиентов : ", difTaskQuery.RowCount());

			var jobTaskQuery = baseQuery.And(() => tasksAlias.TaskState == CallTaskStatus.Task);
			statisticsParam.Add("Заданий : ", difTaskQuery.RowCount());

			statisticsParam.Add("Кол-во задач : ", taskCount);

			statisticsParam.Add("Тара на забор : ", ItemsList.OfType<CallTaskVMNode>().Sum(x => x.TareReturn));

			return statisticsParam;
		}

		protected override bool NeedUpdateFunc(CallTask updatedSubject) => true;

		public override Menu GetPopupMenu(RepresentationSelectResult[] selected)
		{
			var callTaskNode = selected.Select(x => x.VMNode).OfType<CallTaskVMNode>().ToArray();

			Menu popupMenu = new Menu();
			MenuItem menuItemOpenReportByCounterparty = new MenuItem("Отметить как важное");
			Action<CallTask> action = (task) => task.ImportanceDegree = ImportanceDegreeType.Important;
			menuItemOpenReportByCounterparty.Activated += (e, arg) => 
			{
				ChangeEnitity(action, callTaskNode ,false);
				callTaskNode.ToList().ForEach(obj => obj.ImportanceDegree = ImportanceDegreeType.Important);
			};
			popupMenu.Add(menuItemOpenReportByCounterparty);

			return popupMenu;
		}

		public void ChangeEnitity(Action<CallTask> action , CallTaskVMNode[] tasks , bool NeedUpdate = true)
		{
			if(action == null)
				return;

			tasks.ToList().ForEach((taskNode) => 
			{
				CallTask task = UoW.GetById<CallTask>(taskNode.Id);
				action(task);
				UoW.Save(task);
				UoW.Commit();
			});
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

		public string Phones { get; set; }

		public string EmployeeLastName { get; set; }
		public string EmployeeName { get; set; }
		public string EmployeePatronymic { get; set; }

		public string AssignedEmployeeName => PersonHelper.PersonNameWithInitials(EmployeeLastName, EmployeeName, EmployeePatronymic);

		public DateTime Deadline { get; set; }

		public DateTime CreationDate { get; set; }

		public ImportanceDegreeType ImportanceDegree { get; set; }

		public bool IsTaskComplete { get; set; }

		public int TareReturn { get; set; }

		public string RowColor {
			get {
				if(IsTaskComplete)
					return "green";
				if(DateTime.Now > Deadline)
					return "red";

				return "black";
			}
		}
	}
}
