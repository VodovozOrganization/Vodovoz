using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Services;
using QS.Utilities;
using System;
using System.Linq;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Employees;

namespace Vodovoz.Journals.JournalViewModels.Employees
{
	public class FinesJournalViewModel : EntityJournalViewModelBase<Fine, FineViewModel, FineJournalNode>
	{
		private readonly FineFilterViewModel _filterViewModel;
		private readonly ILifetimeScope _lifetimeScope;

		public FinesJournalViewModel(
			FineFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			Action<FineFilterViewModel> filterConfig = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			if(filterViewModel is null)
			{
				throw new ArgumentNullException(nameof(filterViewModel));
			}

			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			JournalFilter = filterViewModel;
			_filterViewModel = filterViewModel;
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			filterViewModel.JournalViewModel = this;

			filterViewModel.OnFiltered += OnFiltered;

			if(filterConfig != null)
			{
				filterViewModel.ConfigureWithoutFiltering(filterConfig);
			}

			TabName = $"Журнал {typeof(Fine).GetClassUserFriendlyName().GenitivePlural}";
			UpdateOnChanges(typeof(Fine), typeof(FineItem));

			UseSlider = true;
		}

		private void OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public ILifetimeScope Scope => _lifetimeScope;

		private string GetTotalSumInfo()
		{
			var total = Items.Cast<FineJournalNode>().Sum(node => node.FineSum);
			return CurrencyWorks.GetShortCurrencyString(total);
		}

		public override string FooterInfo
		{
			get => $"Сумма отфильтрованных штрафов:{GetTotalSumInfo()}. {base.FooterInfo}";
			set { }
		}

		protected override IQueryOver<Fine> ItemsQuery(IUnitOfWork unitOfWork)
		{
			FineJournalNode resultAlias = null;
			Fine fineAlias = null;
			FineItem fineItemAlias = null;
			Employee finedEmployeeAlias = null;
			Subdivision finedEmployeeSubdivision = null;
			Employee fineAuthorAlias = null;
			RouteList routeListAlias = null;

			var query = unitOfWork.Session.QueryOver(() => fineAlias)
				.JoinAlias(() => fineAlias.Author, () => fineAuthorAlias)
				.JoinAlias(f => f.Items, () => fineItemAlias)
				.JoinAlias(() => fineItemAlias.Employee, () => finedEmployeeAlias)
				.JoinAlias(() => finedEmployeeAlias.Subdivision, () => finedEmployeeSubdivision)
				.JoinAlias(f => f.RouteList, () => routeListAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if(_filterViewModel.Subdivision != null)
			{
				query.Where(() => finedEmployeeAlias.Subdivision.Id == _filterViewModel.Subdivision.Id);
			}

			if(_filterViewModel.Author != null)
			{
				query.Where(() => fineAuthorAlias.Id == _filterViewModel.Author.Id);
			}

			if(_filterViewModel.FineDateStart.HasValue)
			{
				query.Where(() => fineAlias.Date >= _filterViewModel.FineDateStart.Value);
			}

			if(_filterViewModel.FineDateEnd.HasValue)
			{
				query.Where(() => fineAlias.Date <= _filterViewModel.FineDateEnd.Value);
			}

			if(_filterViewModel.RouteListDateStart.HasValue)
			{
				query.Where(() => routeListAlias.Date >= _filterViewModel.RouteListDateStart.Value);
			}

			if(_filterViewModel.RouteListDateEnd.HasValue)
			{
				query.Where(() => routeListAlias.Date <= _filterViewModel.RouteListDateEnd.Value);
			}

			if(_filterViewModel.ExcludedIds != null && _filterViewModel.ExcludedIds.Any())
			{
				query.WhereRestrictionOn(() => fineAlias.Id).Not.IsIn(_filterViewModel.ExcludedIds);
			}

			if(_filterViewModel.FindFinesWithIds != null && _filterViewModel.FindFinesWithIds.Any())
			{
				query.WhereRestrictionOn(() => fineAlias.Id).IsIn(_filterViewModel.FindFinesWithIds);
			}

			var employeeProjection = CustomProjections.Concat_WS(
				" ",
				() => finedEmployeeAlias.LastName,
				() => finedEmployeeAlias.Name,
				() => finedEmployeeAlias.Patronymic
			);

			query.Where(GetSearchCriterion(
				() => fineAlias.Id,
				() => fineAlias.TotalMoney,
				() => fineAlias.FineReasonString,
				() => employeeProjection
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
							Projections.Property(() => finedEmployeeAlias.LastName),
							Projections.Property(() => finedEmployeeAlias.Name),
							Projections.Property(() => finedEmployeeAlias.Patronymic)
						),
						Projections.Constant("\n"))).WithAlias(() => resultAlias.FinedEmployeesNames)
					.Select(() => fineAlias.FineReasonString).WithAlias(() => resultAlias.FineReason)
					.Select(() => fineAlias.TotalMoney).WithAlias(() => resultAlias.FineSum)
					.Select(Projections.SqlFunction(new StandardSQLFunction("CONCAT_WS"),
							NHibernateUtil.String,
							Projections.Constant(" "),
							Projections.Property(() => fineAuthorAlias.LastName),
							Projections.Property(() => fineAuthorAlias.Name),
							Projections.Property(() => fineAuthorAlias.Patronymic)
						)).WithAlias(() => resultAlias.AuthorName)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
						NHibernateUtil.String,
						Projections.Property(() => finedEmployeeSubdivision.Name),
						Projections.Constant("\n"))).WithAlias(() => resultAlias.FinedEmployeesSubdivisions)
				).OrderBy(o => o.Date).Desc.OrderBy(o => o.Id).Desc
				.TransformUsing(Transformers.AliasToBean<FineJournalNode>());
		}
	}
}
