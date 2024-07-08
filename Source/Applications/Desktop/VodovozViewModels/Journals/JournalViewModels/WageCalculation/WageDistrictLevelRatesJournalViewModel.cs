using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Journals.JournalViewModels.WageCalculation
{
	public class WageDistrictLevelRatesJournalViewModel : SingleEntityJournalViewModelBase<WageDistrictLevelRates, WageDistrictLevelRatesViewModel, WageDistrictLevelRatesJournalNode>
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IWageCalculationRepository _wageCalculationRepository;

		public WageDistrictLevelRatesJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, IWageCalculationRepository wageCalculationRepository) : base(unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			TabName = "Журнал ставок по уровням";

			var threadLoader = DataLoader as ThreadDataLoader<WageDistrictLevelRatesJournalNode>;
			threadLoader.MergeInOrderBy(x => x.IsArchive, false);
			threadLoader.MergeInOrderBy(x => x.Name, false);

			UpdateOnChanges(typeof(WageDistrictLevelRates));
		}

		protected override Func<IUnitOfWork, IQueryOver<WageDistrictLevelRates>> ItemsSourceQueryFunction => (uow) => {
			WageDistrictLevelRatesJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<WageDistrictLevelRates>();
			query.Where(
				GetSearchCriterion<WageDistrictLevelRates>(
					x => x.Id
				)
			);

			var result = query.SelectList(list => list
									.Select(x => x.Id).WithAlias(() => resultAlias.Id)
									.Select(x => x.Name).WithAlias(() => resultAlias.Name)
									.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
									.Select(x => x.IsDefaultLevel).WithAlias(() => resultAlias.IsDefaultLevel)
									.Select(x => x.IsDefaultLevelForOurCars).WithAlias(() => resultAlias.IsDefaultLevelOurCars)
									.Select(x => x.IsDefaultLevelForRaskatCars).WithAlias(() => resultAlias.IsDefaultLevelRaskatCars)
								)
								.TransformUsing(Transformers.AliasToBean<WageDistrictLevelRatesJournalNode>())
								.OrderBy(x => x.Name).Asc
								.ThenBy(x => x.IsArchive).Asc
								;

			return result;
		};

		protected override Func<WageDistrictLevelRatesViewModel> CreateDialogFunction => () => new WageDistrictLevelRatesViewModel(
			this,
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices,
			UoW,
			_wageCalculationRepository
	   );

		protected override Func<WageDistrictLevelRatesJournalNode, WageDistrictLevelRatesViewModel> OpenDialogFunction => n => new WageDistrictLevelRatesViewModel(
			this,
			EntityUoWBuilder.ForOpen(n.Id),
			unitOfWorkFactory,
			commonServices,
			UoW,
			_wageCalculationRepository
	   );
	}
}
