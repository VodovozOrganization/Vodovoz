using System;
using Gamma.ColumnConfig;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;
using Vodovoz.JournalFilters;

namespace Vodovoz.Representations
{
	public class PremiumVM : RepresentationModelEntityBase<Premium, PremiumVMNode>
	{
		#region Поля
		#endregion

		#region Конструкторы

		public PremiumVM() : this(UnitOfWorkFactory.CreateWithoutRoot ())
		{
			CreateRepresentationFilter = () => new PremiumFilter(UoW);
		}

		public PremiumVM(PremiumFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public PremiumVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
		}

		#endregion

		#region Свойства

		public virtual PremiumFilter Filter {
			get {
				return RepresentationFilter as PremiumFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
			PremiumVMNode resultAlias = null;
			Premium premiumAlias = null;
			PremiumItem premiumItemAlias = null;
			Employee employeeAlias = null;
			Subdivision subdivisionAlias = null;

			var query = UoW.Session.QueryOver<Premium>(() => premiumAlias)
				.JoinAlias(f => f.Items, () => premiumItemAlias)
				.JoinAlias(() => premiumItemAlias.Employee, () => employeeAlias);

			if(Filter.RestrictionSubdivision != null) {
				query.Where(() => employeeAlias.Subdivision.Id == Filter.RestrictionSubdivision.Id);
			}

			if(Filter.RestrictionPremiumDateStart.HasValue) {
				query.Where(() => premiumAlias.Date >= Filter.RestrictionPremiumDateStart.Value);
			}

			if(Filter.RestrictionPremiumDateEnd.HasValue) {
				query.Where(() => premiumAlias.Date <= Filter.RestrictionPremiumDateEnd.Value);
			}

			var result = query
				.SelectList(list => list
					.SelectGroup(() => premiumAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => premiumAlias.Date).WithAlias(() => resultAlias.Date)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.SqlFunction(new StandardSQLFunction("CONCAT_WS"),
							NHibernateUtil.String,
							Projections.Constant(" "),
							Projections.Property(() => employeeAlias.LastName),
							Projections.Property(() => employeeAlias.Name),
							Projections.Property(() => employeeAlias.Patronymic)
						),
						Projections.Constant("\n"))).WithAlias(() => resultAlias.EmployeesName)
					.Select(() => premiumAlias.PremiumReasonString).WithAlias(() => resultAlias.PremiumReason)
					.Select(() => premiumAlias.TotalMoney).WithAlias(() => resultAlias.PremiumSumm)
				).OrderBy(o => o.Date).Desc
				.TransformUsing(Transformers.AliasToBean<PremiumVMNode>())
				.List<PremiumVMNode>();

			SetItemsSource(result);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<PremiumVMNode>.Create()
			.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Дата").AddTextRenderer(node => node.Date.ToString("d"))
			.AddColumn("Сотудники").AddTextRenderer(node => node.EmployeesName)
			.AddColumn("Сумма штрафа").AddTextRenderer(node => node.PremiumSumm.ToString())
			.AddColumn("Причина штрафа").AddTextRenderer(node => node.PremiumReason)
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get {
				return columnsConfig;
			}
		}

		#endregion

		#region implemented abstract members of RepresentationModelEntityBase

		protected override bool NeedUpdateFunc(Premium updatedSubject)
		{
			return true;
		}

		#endregion

		#region Методы
		#endregion
	}

	public class PremiumVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id { get; set; }

		public DateTime Date { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string EmployeesName { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string PremiumReason { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public decimal PremiumSumm { get; set; }
	}
}
