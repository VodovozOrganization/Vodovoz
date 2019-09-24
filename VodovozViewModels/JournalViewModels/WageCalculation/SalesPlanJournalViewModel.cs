using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.Config;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.JournalViewModels.WageCalculation
{
	public class SalesPlanJournalViewModel : SingleEntityJournalViewModelBase<SalesPlan, SalesPlanViewModel, SalesPlanJournalNode>
	{
		public SalesPlanJournalViewModel(IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(entityConfigurationProvider, commonServices)
		{
			TabName = "Журнал планов продаж";
			SetOrder(
				new Dictionary<Func<SalesPlanJournalNode, object>, bool> {
					{ x => x.IsArchive, false },
					{ x => x.Id, false }
				}
			);

			UpdateOnChanges(typeof(SalesPlan));
		}

		protected override Func<IQueryOver<SalesPlan>> ItemsSourceQueryFunction => () => {
			SalesPlanJournalNode resultAlias = null;

			var query = UoW.Session.QueryOver<SalesPlan>();
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
