using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;
using Vodovoz.JournalFilters;

namespace Vodovoz.ViewModel
{
	public class EmployeesVM : RepresentationModelEntityBase<Employee, EmployeesVMNode>
	{
		public EmployeeRepresentationFilterViewModel Filter {
			get => RepresentationFilter as EmployeeRepresentationFilterViewModel;
			set => RepresentationFilter = value as IRepresentationFilter;
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			EmployeesVMNode resultAlias = null;
			Employee employeeAlias = null;

			var query = UoW.Session.QueryOver<Employee>(() => employeeAlias);

			if(Filter?.Status != null)
				query.Where(e => e.Status == Filter.Status);

			if(Filter?.Category != null)
				query.Where(e => e.Category == Filter.Category);

			if(Filter?.RestrictWageParameterItemType != null) {
				WageParameterItem wageParameterItemAlias = null;
				var subquery = QueryOver.Of<EmployeeWageParameter>()
					.Left.JoinAlias(x => x.WageParameterItem, () => wageParameterItemAlias)
					.Where(() => wageParameterItemAlias.WageParameterItemType == Filter.RestrictWageParameterItemType.Value)
					.Where(p => p.EndDate == null || p.EndDate >= DateTime.Today)
					.Select(p => p.Employee.Id)
				;
				query.WithSubquery.WhereProperty(e => e.Id).In(subquery);
			}

			if(Filter?.DriverTerminalRelation != null)
			{
				var relation = Filter?.DriverTerminalRelation;
				DriverAttachedTerminalDocumentBase baseAlias = null;
				DriverAttachedTerminalGiveoutDocument giveoutAlias = null;
				var baseQuery = QueryOver.Of(() => baseAlias)
					.Where(doc => doc.Driver.Id == employeeAlias.Id)
					.Select(doc => doc.Id).OrderBy(doc => doc.CreationDate).Desc.Take(1);
				var giveoutQuery = QueryOver.Of(() => giveoutAlias).WithSubquery.WhereProperty(giveout => giveout.Id).Eq(baseQuery)
					.Select(doc => doc.Driver.Id);
				if(relation == DriverTerminalRelation.WithTerminal)
				{
					query.WithSubquery.WhereProperty(e => e.Id).In(giveoutQuery);
				}
				else
				{
					query.WithSubquery.WhereProperty(e => e.Id).NotIn(giveoutQuery);
				}
			}

			IList<EmployeesVMNode> result = null;

			if(Filter?.SortByPriority ?? false)
			{
				var endDate = DateTime.Today;
				var start3 = endDate.AddMonths(-3).AddDays(-2);
				var start2 = endDate.AddMonths(-2).AddDays(-1);
				var start1 = endDate.AddMonths(-1);
				endDate = endDate.AddDays(1).AddSeconds(-1);
				var timestampDiff = new SQLFunctionTemplate(
					NHibernateUtil.Int32, "CASE WHEN TIMESTAMPDIFF(MONTH, ?1, ?2) > 2 THEN 3 ELSE TIMESTAMPDIFF(MONTH, ?1, ?2) END");
				var avgSalary = new SQLFunctionTemplate(NHibernateUtil.Decimal, "(IFNULL(?1,0)+IFNULL(?2,0)+IFNULL(?3,0))/3");
				WagesMovementOperations wmo3 = null;
				WagesMovementOperations wmo2 = null;
				WagesMovementOperations wmo1 = null;
				const WagesType opType = WagesType.AccrualWage;

				result = query
						.SelectList(list => list
							.Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
							.Select(() => employeeAlias.Status).WithAlias(() => resultAlias.Status)
							.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmpFirstName)
							.Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmpLastName)
							.Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmpMiddleName)
							.Select(() => employeeAlias.Category).WithAlias(() => resultAlias.EmpCatEnum)
						//расчет стажа работы в месяцах
							.Select(Projections.SqlFunction(timestampDiff, NHibernateUtil.Int32,
								Projections.Property(() => employeeAlias.CreationDate),
								Projections.Constant(endDate))
							).WithAlias(() => resultAlias.TotalMonths)
						//расчет средней зп за последние три месяца
							.Select(Projections.SqlFunction(avgSalary, NHibernateUtil.Decimal,
									Projections.SubQuery(QueryOver.Of(() => wmo3)
										.Where(() => wmo3.Employee.Id == employeeAlias.Id)
										.And(Restrictions.Ge(Projections.Property(() => wmo3.OperationTime), start3))
										.And(Restrictions.Lt(Projections.Property(() => wmo3.OperationTime), start2))
										.And(() => wmo3.OperationType == opType)
										.Select(Projections.Sum(() => wmo3.Money))),

									Projections.SubQuery(QueryOver.Of(() => wmo2)
										.Where(() => wmo2.Employee.Id == employeeAlias.Id)
										.And(Restrictions.Ge(Projections.Property(() => wmo2.OperationTime), start2))
										.And(Restrictions.Lt(Projections.Property(() => wmo2.OperationTime), start1))
										.And(() => wmo2.OperationType == opType)
										.Select(Projections.Sum(() => wmo2.Money))),

									Projections.SubQuery(QueryOver.Of(() => wmo1)
										.Where(() => wmo1.Employee.Id == employeeAlias.Id)
										.And(Restrictions.Ge(Projections.Property(() => wmo1.OperationTime), start1))
										.And(Restrictions.Le(Projections.Property(() => wmo1.OperationTime), endDate))
										.And(() => wmo1.OperationType == opType)
										.Select(Projections.Sum(() => wmo1.Money))))
							).WithAlias(() => resultAlias.AvgSalary)

							.SelectGroup(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
						)
						.OrderByAlias(() => resultAlias.TotalMonths).Desc
						.ThenByAlias(() => resultAlias.AvgSalary).Desc
						.TransformUsing(Transformers.AliasToBean<EmployeesVMNode>())
						.List<EmployeesVMNode>();
				SetItemsSource(result);
				return;
			}

			result = query
				.SelectList(list => list
				   .Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => employeeAlias.Status).WithAlias(() => resultAlias.Status)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmpFirstName)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmpLastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmpMiddleName)
				   .Select(() => employeeAlias.Category).WithAlias(() => resultAlias.EmpCatEnum)
				)
				.OrderBy(x => x.LastName).Asc
				.OrderBy(x => x.Name).Asc
				.OrderBy(x => x.Patronymic).Asc
				.TransformUsing(NHibernate.Transform.Transformers.AliasToBean<EmployeesVMNode>())
				.List<EmployeesVMNode>();

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<EmployeesVMNode>.Create()
			.AddColumn("Код").SetDataProperty(node => node.Id.ToString())
			.AddColumn("Ф.И.О.").SetDataProperty(node => node.FullName)
			.AddColumn("Категория").SetDataProperty(node => node.EmpCatEnum.GetEnumTitle())
				.MinWidth(200)
			.AddColumn("Статус").AddEnumRenderer(node => node.Status)
			.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
			.Finish();

		public override IColumnsConfig ColumnsConfig => columnsConfig;

		public override bool PopupMenuExist => false;

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(Employee updatedSubject) => true;

		#endregion

		public EmployeesVM(IUnitOfWork uow, EmployeeRepresentationFilterViewModel filterViewModel)
		{
			Filter = filterViewModel;
			UoW = uow;
		}

		public EmployeesVM(EmployeeRepresentationFilterViewModel filterViewModel)
		{
			Filter = filterViewModel;
		}

		public EmployeesVM()
		{
			CreateRepresentationFilter = () => new EmployeeRepresentationFilterViewModel { Status = EmployeeStatus.IsWorking };
		}

		public EmployeesVM(IUnitOfWork uow)
		{
			CreateRepresentationFilter = () => new EmployeeRepresentationFilterViewModel { Status = EmployeeStatus.IsWorking };
			this.UoW = uow;
		}
	}

	public class EmployeesVMNode : QS.RepresentationModel.GtkUI.INodeWithEntryFastSelect
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public string EmpLastName { get; set; }
		public string EmpFirstName { get; set; }
		public string EmpMiddleName { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string FullName => String.Format("{0} {1} {2}", EmpLastName, EmpFirstName, EmpMiddleName);

		public EmployeeCategory EmpCatEnum { get; set; }

		public EmployeeStatus Status { get; set; }

		public string RowColor => Status == EmployeeStatus.IsFired ? "grey" : "black";

		public string EntityTitle => FullName;

		/// <summary>
		/// Стаж работы в месяцах
		/// </summary>
		public int TotalMonths { get; set; }
		/// <summary>
		/// Средняя з/п за три месяца
		/// </summary>
		public decimal AvgSalary { get; set; }
	}
}
