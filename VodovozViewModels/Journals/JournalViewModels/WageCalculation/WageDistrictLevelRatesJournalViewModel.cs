using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Journals.JournalViewModels.WageCalculation
{
	public class WageDistrictLevelRatesJournalViewModel : SingleEntityJournalViewModelBase<WageDistrictLevelRates, WageDistrictLevelRatesViewModel, WageDistrictLevelRatesJournalNode>
	{
		public WageDistrictLevelRatesJournalViewModel(
			EntitiesJournalActionsViewModel journalActionsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices) : base(journalActionsViewModel, unitOfWorkFactory, commonServices)
		{
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
			UnitOfWorkFactory,
			CommonServices,
			UoW
	   );

		protected override Func<JournalEntityNodeBase, WageDistrictLevelRatesViewModel> OpenDialogFunction => n => new WageDistrictLevelRatesViewModel(
			this,
			EntityUoWBuilder.ForOpen(n.Id),
			UnitOfWorkFactory,
			CommonServices,
			UoW
	   );
	}
}
