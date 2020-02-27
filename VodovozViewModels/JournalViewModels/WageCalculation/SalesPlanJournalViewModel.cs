using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;
using QS.Services;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.JournalViewModels.WageCalculation
{
	public class SalesPlanJournalViewModel : SingleEntityJournalViewModelBase<SalesPlan, SalesPlanViewModel, SalesPlanJournalNode, CriterionSearchModel>
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		public SalesPlanJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			SearchViewModelBase<CriterionSearchModel> searchViewModel)
		: base(unitOfWorkFactory, commonServices, searchViewModel)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = "Журнал планов продаж";

			var threadLoader = DataLoader as ThreadDataLoader<SalesPlanJournalNode>;
			threadLoader.MergeInOrderBy(x => x.IsArchive, false);
			threadLoader.MergeInOrderBy(x => x.Id, false);

			UpdateOnChanges(typeof(SalesPlan));
		}

		protected override Func<IUnitOfWork, IQueryOver<SalesPlan>> ItemsSourceQueryFunction => (uow) => {
			SalesPlanJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<SalesPlan>();
			query.Where(
				CriterionSearchModel.ConfigureSearch()
				.AddSearchBy<SalesPlan>(x => x.Id)
				.GetSearchCriterion()
			);

			var result = query.SelectList(list => list
									.Select(x => x.Id).WithAlias(() => resultAlias.Id)
									.Select(x => x.FullBottleToSell).WithAlias(() => resultAlias.FullBottleToSell)
									.Select(x => x.EmptyBottlesToTake).WithAlias(() => resultAlias.EmptyBottlesToTake)
									.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
								)
								.TransformUsing(Transformers.AliasToBean<SalesPlanJournalNode>())
								.OrderBy(x => x.Name).Asc
								.ThenBy(x => x.IsArchive).Asc
								;

			return result;
		};

		protected override Func<SalesPlanViewModel> CreateDialogFunction => () => new SalesPlanViewModel(
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices
		);

		protected override Func<SalesPlanJournalNode, SalesPlanViewModel> OpenDialogFunction => node => new SalesPlanViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			unitOfWorkFactory,
			commonServices
	   	);
	}
}
