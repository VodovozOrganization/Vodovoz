using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalNodes;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels;

namespace Vodovoz.JournalViewModels
{
	public class ResidueJournalViewModel : FilterableSingleEntityJournalViewModelBase<Residue, ResidueViewModel, ResidueJournalNode, ResidueFilterViewModel>
	{
		readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;

		public ResidueJournalViewModel(
			ResidueFilterViewModel filterViewModel,
			IEmployeeService employeeService,
			IRepresentationEntityPicker representationEntityPicker,
			IMoneyRepository moneyRepository,
			IDepositRepository depositRepository,
			IBottlesRepository bottlesRepository,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			ICounterpartyJournalFactory counterpartyJournalFactory
		) 
		: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			TabName = "Журнал остатков";
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.representationEntityPicker = representationEntityPicker ?? throw new ArgumentNullException(nameof(representationEntityPicker));
			this.moneyRepository = moneyRepository ?? throw new ArgumentNullException(nameof(moneyRepository));
			this.depositRepository = depositRepository ?? throw new ArgumentNullException(nameof(depositRepository));
			this.bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			SetOrder(x => x.Date, true);
			UpdateOnChanges(
				typeof(Residue)
			);
		}

		private readonly IEmployeeService employeeService;
		private readonly IRepresentationEntityPicker representationEntityPicker;
		private readonly IMoneyRepository moneyRepository;
		private readonly IDepositRepository depositRepository;
		private readonly IBottlesRepository bottlesRepository;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly ICommonServices commonServices;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;

		protected override Func<IUnitOfWork, IQueryOver<Residue>> ItemsSourceQueryFunction => (uow) => {
			Counterparty counterpartyAlias = null;
			Employee authorAlias = null;
			Employee lastEditorAlias = null;
			ResidueJournalNode resultAlias = null;
			Residue residueAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var residueQuery = uow.Session.QueryOver<Residue>(() => residueAlias)
				.JoinQueryOver(() => residueAlias.Customer, () => counterpartyAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => residueAlias.DeliveryPoint, () => deliveryPointAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => residueAlias.LastEditAuthor, () => lastEditorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinQueryOver(() => residueAlias.Author, () => authorAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			if(FilterViewModel != null) {
				var dateCriterion = Projections.SqlFunction(
					   new SQLFunctionTemplate(
						   NHibernateUtil.Date,
						   "Date(?1)"
						  ),
					   NHibernateUtil.Date,
					   Projections.Property(() => residueAlias.Date)
					);

				if(FilterViewModel.StartDate.HasValue) {
					residueQuery.Where(Restrictions.Ge(dateCriterion, FilterViewModel.StartDate.Value));
				}

				if(FilterViewModel.EndDate.HasValue) {
					residueQuery.Where(Restrictions.Le(dateCriterion, FilterViewModel.EndDate.Value));
				}
			}

			residueQuery.Where(GetSearchCriterion(
				() => residueAlias.Id,
				() => counterpartyAlias.Name,
				() => deliveryPointAlias.CompiledAddress
			));

			var resultQuery = residueQuery
				.SelectList(list => list
				   .Select(() => residueAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => residueAlias.Date).WithAlias(() => resultAlias.Date)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Counterparty)
				   .Select(() => deliveryPointAlias.ShortAddress).WithAlias(() => resultAlias.DeliveryPoint)
				   .Select(() => authorAlias.LastName).WithAlias(() => resultAlias.AuthorSurname)
				   .Select(() => authorAlias.Name).WithAlias(() => resultAlias.AuthorName)
				   .Select(() => authorAlias.Patronymic).WithAlias(() => resultAlias.AuthorPatronymic)
				   .Select(() => lastEditorAlias.LastName).WithAlias(() => resultAlias.LastEditorSurname)
				   .Select(() => lastEditorAlias.Name).WithAlias(() => resultAlias.LastEditorName)
				   .Select(() => lastEditorAlias.Patronymic).WithAlias(() => resultAlias.LastEditorPatronymic)
				   .Select(() => residueAlias.LastEditTime).WithAlias(() => resultAlias.LastEditedTime)
				)
				.OrderBy(() => residueAlias.Date).Desc
				.TransformUsing(Transformers.AliasToBean<ResidueJournalNode>());
			return resultQuery;
		};

		protected override Func<ResidueViewModel> CreateDialogFunction => () => new ResidueViewModel(
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			employeeService, 
			representationEntityPicker, 
			bottlesRepository, 
			depositRepository, 
			moneyRepository, 
			commonServices,
			_employeeJournalFactory,
			_subdivisionParametersProvider,
			_counterpartyJournalFactory
		);

		protected override Func<ResidueJournalNode, ResidueViewModel> OpenDialogFunction => (node) => new ResidueViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			unitOfWorkFactory,
			employeeService, 
			representationEntityPicker, 
			bottlesRepository, 
			depositRepository, 
			moneyRepository, 
			commonServices,
			_employeeJournalFactory,
			_subdivisionParametersProvider,
			_counterpartyJournalFactory
		);
	}
}
