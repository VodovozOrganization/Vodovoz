using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.Utilities;
using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;

namespace Vodovoz.Journals.JournalViewModels.Employees
{
	public class FinesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Fine, FineViewModel, FineJournalNode, FineFilterViewModel>
	{
		private readonly IEmployeeService employeeService;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICommonServices commonServices;

		public FinesJournalViewModel(
			FineFilterViewModel filterViewModel,
			IEmployeeService employeeService,
			IEmployeeJournalFactory employeeJournalFactory,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			TabName = "Журнал штрафов";
			UpdateOnChanges(typeof(Fine), typeof(FineItem));
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateEditAction();
			CreateDefaultDeleteAction();
		}

		private string GetTotalSumInfo()
		{
			var total = Items.Cast<FineJournalNode>().Sum(node => node.FineSumm);
			return CurrencyWorks.GetShortCurrencyString(total);
		}

		public override string FooterInfo
		{
			get => $"Сумма отфильтрованных штрафов:{GetTotalSumInfo()}. {base.FooterInfo}";
			set { }
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<FineJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					FineJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<FineJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					FineJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog) {
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None) {
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		protected override Func<IUnitOfWork, IQueryOver<Fine>> ItemsSourceQueryFunction => uow => {
			FineJournalNode resultAlias = null;
			Fine fineAlias = null;
			FineItem fineItemAlias = null;
			Employee employeeAlias = null;
			RouteList routeListAlias = null;

			var query = uow.Session.QueryOver<Fine>(() => fineAlias)
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

			if (FilterViewModel.ExcludedIds != null && FilterViewModel.ExcludedIds.Any())
				query.WhereRestrictionOn(() => fineAlias.Id).Not.IsIn(FilterViewModel.ExcludedIds);
			
			if (FilterViewModel.FindFinesWithIds != null && FilterViewModel.FindFinesWithIds.Any())
				query.WhereRestrictionOn(() => fineAlias.Id).IsIn(FilterViewModel.FindFinesWithIds);

			var employeeProjection = CustomProjections.Concat_WS(
				" ",
				() => employeeAlias.LastName,
				() => employeeAlias.Name,
				() => employeeAlias.Patronymic
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
							Projections.Property(() => employeeAlias.LastName),
							Projections.Property(() => employeeAlias.Name),
							Projections.Property(() => employeeAlias.Patronymic)
						),
						Projections.Constant("\n"))).WithAlias(() => resultAlias.EmployeesName)
					.Select(() => fineAlias.FineReasonString).WithAlias(() => resultAlias.FineReason)
					.Select(() => fineAlias.TotalMoney).WithAlias(() => resultAlias.FineSumm)
				).OrderBy(o => o.Date).Desc.OrderBy(o => o.Id).Desc
				.TransformUsing(Transformers.AliasToBean<FineJournalNode>());
		};

		protected override Func<FineViewModel> CreateDialogFunction => () => new FineViewModel(
			EntityUoWBuilder.ForCreate(),
			QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
			employeeService,
			_employeeJournalFactory,
			commonServices,
			NavigationManager
		);

		protected override Func<FineJournalNode, FineViewModel> OpenDialogFunction => (node) => new FineViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
			employeeService,
			_employeeJournalFactory,
			commonServices,
			NavigationManager
		);
	}
}
