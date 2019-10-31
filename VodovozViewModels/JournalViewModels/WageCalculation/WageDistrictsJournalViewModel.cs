using System;
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
	public class WageDistrictsJournalViewModel : SingleEntityJournalViewModelBase<WageDistrict, WageDistrictViewModel, WageDistrictJournalNode>
	{
		public WageDistrictsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices) : base(unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал групп зарплатных районов";

			var threadLoader = DataLoader as ThreadDataLoader<WageDistrictJournalNode>;
			threadLoader.MergeInOrderBy(x => x.IsArchive, false);
			threadLoader.MergeInOrderBy(x => x.Name, false);

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

		protected override Func<IUnitOfWork, IQueryOver<WageDistrict>> ItemsSourceQueryFunction => (uow) => {
			WageDistrictJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<WageDistrict>();
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
