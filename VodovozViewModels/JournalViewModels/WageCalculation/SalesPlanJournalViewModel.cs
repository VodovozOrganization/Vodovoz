using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.JournalViewModels.WageCalculation
{
	public class SalesPlanJournalViewModel : SingleEntityJournalViewModelBase<SalesPlan, SalesPlanViewModel, SalesPlanJournalNode>
	{
		public SalesPlanJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(unitOfWorkFactory, commonServices)
		{
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
				GetSearchCriterion<SalesPlan>(
					x => x.Id
				)
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
		   EntityConstructorParam.ForCreate(),
		   commonServices
		);

		protected override Func<SalesPlanJournalNode, SalesPlanViewModel> OpenDialogFunction => n => new SalesPlanViewModel(
		   EntityConstructorParam.ForOpen(n.Id),
		   commonServices
	   	);
	}
}
