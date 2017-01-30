using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;
using Gamma.ColumnConfig;
using QSOrmProject;
using NHibernate.Transform;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate;

namespace Vodovoz.ViewModel
{
	public class FinesVM : RepresentationModelEntityBase<Fine, FinesVMNode>
	{
		#region Поля
		#endregion

		#region Конструкторы

		public FinesVM() : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new FineFilter(UoW);
		}

		public FinesVM(FineFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public FinesVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
		}

		#endregion

		#region Свойства

		public FineFilter Filter {
			get {
				return RepresentationFilter as FineFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
			FinesVMNode resultAlias = null;
			Fine fineAlias = null;
			FineItem fineItemAlias = null;
			Employee employeeAlias = null;

			var query = UoW.Session.QueryOver<Fine> (() => fineAlias);

			var subquery = NHibernate.Criterion.QueryOver.Of<FineItem>(() => fineItemAlias)
				.Where(() => fineAlias.Id == fineItemAlias.Fine.Id)
				.JoinAlias(fi => fi.Employee, () => employeeAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Select(Projections.SqlFunction (
					new SQLFunctionTemplate (NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
					NHibernateUtil.String,
						Projections.SqlFunction (new StandardSQLFunction("CONCAT_WS"),
						NHibernateUtil.String,
						Projections.Constant (" "),
						Projections.Property (() => employeeAlias.LastName),
						Projections.Property (() => employeeAlias.Name),
						Projections.Property (() => employeeAlias.Patronymic)
					),
					Projections.Constant ("\n")));

			var result = query
				.SelectList(list => list
					.Select(() => fineAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fineAlias.Date).WithAlias(() => resultAlias.Date)
					.SelectSubQuery(subquery).WithAlias(() => resultAlias.EmployeesName)
				).OrderBy(o => o.Id).Asc
				.TransformUsing(Transformers.AliasToBean<FinesVMNode>())
				.List<FinesVMNode>();

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <FinesVMNode>.Create()
			.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
			.AddColumn("Сотудники").AddTextRenderer(node => node.EmployeesName)
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get {
				return columnsConfig;
			}
		}

		#endregion

		#region implemented abstract members of RepresentationModelEntityBase

		protected override bool NeedUpdateFunc(Fine updatedSubject)
		{
			return true;
		}

		#endregion

		#region Методы
		#endregion
	}

	public class FinesVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public DateTime Date { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string EmployeesName { get; set; }
	}
}

