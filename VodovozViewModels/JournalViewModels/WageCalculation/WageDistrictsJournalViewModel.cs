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
	public class WageDistrictsJournalViewModel : SingleEntityJournalViewModelBase<WageDistrict, WageDistrictViewModel, WageDistrictJournalNode>
	{
		public WageDistrictsJournalViewModel(ICommonServices commonServices) : base(commonServices)
		{
			TabName = "Журнал групп зарплатных районов";
			SetOrder(
				new Dictionary<Func<WageDistrictJournalNode, object>, bool> {
					{ x => x.IsArchive, false },
					{ x => x.Name, false }
				}
			);

			UpdateOnChanges(typeof(WageDistrict));
		}

		protected override Func<WageDistrictViewModel> CreateDialogFunction => () => new WageDistrictViewModel(
			EntityConstructorParam.ForCreate(),
			commonServices
		);

		protected override Func<WageDistrictJournalNode, WageDistrictViewModel> OpenDialogFunction => n => new WageDistrictViewModel(
			EntityConstructorParam.ForOpen(n.Id),
			commonServices
		);

		protected override Func<IQueryOver<WageDistrict>> ItemsSourceQueryFunction => () => {
			WageDistrictJournalNode resultAlias = null;

			var query = UoW.Session.QueryOver<WageDistrict>();
			query.Where(
				GetSearchCriterion<WageDistrict>(
					x => x.Id
				)
			);

			var result = query.SelectList(list => list
									.Select(x => x.Id).WithAlias(() => resultAlias.Id)
									.Select(x => x.Name).WithAlias(() => resultAlias.Name)
									.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
								)
								.TransformUsing(Transformers.AliasToBean<WageDistrictJournalNode>())
								.OrderBy(x => x.Name).Asc
								.ThenBy(x => x.IsArchive).Asc
								;

			return result;
		};
	}
}
