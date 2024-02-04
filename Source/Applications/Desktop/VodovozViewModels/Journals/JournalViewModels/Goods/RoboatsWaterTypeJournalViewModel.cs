using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Roboats;
using Vodovoz.ViewModels.Dialogs.Roboats;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class RoboatsWaterTypeJournalViewModel : EntityJournalViewModelBase<RoboatsWaterType, RoboatsWaterTypeViewModel, RoboatsWaterTypeJournalNode>
	{

		public RoboatsWaterTypeJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			TabName = "Вода для Roboats";

			UpdateOnChanges(typeof(RoboatsWaterType));
		}

		protected override IQueryOver<RoboatsWaterType> ItemsQuery(IUnitOfWork uow)
		{
			RoboatsWaterType roboatsWaterTypeAlias = null;
			Nomenclature nomenclatureAlias = null;
			RoboatsWaterTypeJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => roboatsWaterTypeAlias)
				.Left.JoinAlias(() => roboatsWaterTypeAlias.Nomenclature, () => nomenclatureAlias);

			itemsQuery.Where(
				GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => roboatsWaterTypeAlias.Id
				)
			);

			itemsQuery
				.SelectList(list => list
					.Select(() => roboatsWaterTypeAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Nomenclature)
					.Select(() => roboatsWaterTypeAlias.RoboatsAudiofile).WithAlias(() => resultAlias.RoboatsAudioFileName)
				)
				.TransformUsing(Transformers.AliasToBean<RoboatsWaterTypeJournalNode>());

			return itemsQuery;
		}
	}
}
