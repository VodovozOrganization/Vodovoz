using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.Config;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Employees;
using Vodovoz.FilterViewModels.Employees;
using QS.Project.Domain;
using Vodovoz.TempAdapters;
using Vodovoz.Infrastructure.Services;
using QS.Project.Journal.EntitySelector;

namespace Vodovoz.JournalViewModels.Employees
{
	public class FinesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Fine, FineViewModel, FineJournalNode, FineFilterViewModel>
	{
		private readonly IUndeliveriesViewOpener undeliveryViewOpener;
		private readonly IEmployeeService employeeService;
		private readonly IEntitySelectorFactory employeeSelectorFactory;
		private readonly IEntityConfigurationProvider entityConfigurationProvider;
		private readonly ICommonServices commonServices;

		public FinesJournalViewModel(
			FineFilterViewModel filterViewModel,
			IUndeliveriesViewOpener undeliveryViewOpener,
			IEmployeeService employeeService,
			IEntitySelectorFactory employeeSelectorFactory,
			IEntityConfigurationProvider entityConfigurationProvider, 
			ICommonServices commonServices
		) : base(filterViewModel, entityConfigurationProvider, commonServices)
		{
			this.undeliveryViewOpener = undeliveryViewOpener ?? throw new ArgumentNullException(nameof(undeliveryViewOpener));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			this.entityConfigurationProvider = entityConfigurationProvider ?? throw new ArgumentNullException(nameof(entityConfigurationProvider));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
		}

		protected override Func<IQueryOver<Fine>> ItemsSourceQueryFunction => () => {
			FineJournalNode resultAlias = null;
			Fine fineAlias = null;
			FineItem fineItemAlias = null;
			Employee employeeAlias = null;
			Subdivision subdivisionAlias = null;
			RouteList routeListAlias = null;

			var query = UoW.Session.QueryOver<Fine>(() => fineAlias)
				.JoinAlias(f => f.Items, () => fineItemAlias)
				.JoinAlias(() => fineItemAlias.Employee, () => employeeAlias)
				.JoinAlias(f => f.RouteList, () => routeListAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if(FilterViewModel.Subdivision != null) {
				query.Where(() => employeeAlias.Subdivision.Id == FilterViewModel.Subdivision.Id);
			}

			if(FilterViewModel.FineDateStart.HasValue) {
				query.Where(() => fineAlias.Date >= FilterViewModel.FineDateStart.Value);
			}

			if(FilterViewModel.FineDateEnd.HasValue) {
				query.Where(() => fineAlias.Date <= FilterViewModel.FineDateEnd.Value);
			}

			if(FilterViewModel.RouteListDateStart.HasValue) {
				query.Where(() => routeListAlias.Date >= FilterViewModel.RouteListDateStart.Value);
			}

			if(FilterViewModel.RouteListDateEnd.HasValue) {
				query.Where(() => routeListAlias.Date <= FilterViewModel.RouteListDateEnd.Value);
			}

			query.Where(GetSearchCriterion(
				() => fineAlias.Id,
				() => fineAlias.TotalMoney,
				() => fineAlias.FineReasonString,
				() => employeeAlias.Name,
				() => employeeAlias.LastName,
				() => employeeAlias.Patronymic
			));

			return query
				.SelectList(list => list
					.SelectGroup(() => fineAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fineAlias.Date).WithAlias(() => resultAlias.Date)
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
					.Select(() => fineAlias.FineReasonString).WithAlias(() => resultAlias.FineReason)
					.Select(() => fineAlias.TotalMoney).WithAlias(() => resultAlias.FineSumm)
				).OrderBy(o => o.Date).Desc
				.TransformUsing(Transformers.AliasToBean<FineJournalNode>());
		};

		protected override Func<FineViewModel> CreateDialogFunction => () => new FineViewModel(
			EntityConstructorParam.ForCreate(),
			undeliveryViewOpener,
			employeeService,
			employeeSelectorFactory,
			entityConfigurationProvider,
			commonServices
		);

		protected override Func<FineJournalNode, FineViewModel> OpenDialogFunction => (node) => new FineViewModel(
			EntityConstructorParam.ForOpen(node.Id),
			undeliveryViewOpener,
			employeeService,
			employeeSelectorFactory,
			entityConfigurationProvider,
			commonServices
		);
	}
}
