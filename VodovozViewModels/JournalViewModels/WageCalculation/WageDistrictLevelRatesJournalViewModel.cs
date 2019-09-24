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
	public class WageDistrictLevelRatesJournalViewModel : SingleEntityJournalViewModelBase<WageDistrictLevelRates, WageDistrictLevelRatesViewModel, WageDistrictLevelRatesJournalNode>
	{
		public WageDistrictLevelRatesJournalViewModel(IEntityConfigurationProvider entityConfigurationProvider, ICommonServices commonServices) : base(entityConfigurationProvider, commonServices)
		{
			TabName = "Журнал ставок по уровням";
			SetOrder(
				new Dictionary<Func<WageDistrictLevelRatesJournalNode, object>, bool> {
					{ x => x.IsArchive, false },
					{ x => x.Name, false }
				}
			);

			UpdateOnChanges(typeof(WageDistrictLevelRates));
		}

		protected override Func<IQueryOver<WageDistrictLevelRates>> ItemsSourceQueryFunction => () => {
			WageDistrictLevelRatesJournalNode resultAlias = null;

			var query = UoW.Session.QueryOver<WageDistrictLevelRates>();
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
								)
								.TransformUsing(Transformers.AliasToBean<WageDistrictLevelRatesJournalNode>())
								.OrderBy(x => x.Name).Asc
								.ThenBy(x => x.IsArchive).Asc
								;

			return result;
		};

		protected override Func<WageDistrictLevelRatesViewModel> CreateDialogFunction => () => new WageDistrictLevelRatesViewModel(
		   EntityConstructorParam.ForCreate(),
		   commonServices,
		   UoW
	   );

		protected override Func<WageDistrictLevelRatesJournalNode, WageDistrictLevelRatesViewModel> OpenDialogFunction => n => new WageDistrictLevelRatesViewModel(
		   EntityConstructorParam.ForOpen(n.Id),
		   commonServices,
		   UoW
	   );
	}
}
