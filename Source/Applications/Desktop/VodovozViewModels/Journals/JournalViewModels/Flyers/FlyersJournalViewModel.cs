using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Flyers;
using Vodovoz.ViewModels.ViewModels.Flyers;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Flyers
{
	public class FlyersJournalViewModel : EntityJournalViewModelBase<Flyer, FlyerViewModel, FlyersJournalNode>
	{
		public FlyersJournalViewModel(
			IUnitOfWorkFactory uowFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(uowFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			
			TabName = "Журнал рекламных листовок";
			
			UpdateOnChanges(
				typeof(Flyer),
				typeof(FlyerActionTime));
		}

		protected override IQueryOver<Flyer> ItemsQuery(IUnitOfWork uow)
		{
			Flyer flyerAlias = null;
			Nomenclature leafletNomenclatureAlias = null;
			FlyerActionTime flyerActionTimeAlias = null;
			FlyersJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => flyerAlias)
				.Left.JoinAlias(l => l.FlyerNomenclature, () => leafletNomenclatureAlias)
				.Left.JoinAlias(l => l.FlyerActionTimes, () => flyerActionTimeAlias);

			query.Where(GetSearchCriterion(
				() => leafletNomenclatureAlias.Name
			));

			var result = query.SelectList(list => list
					.Select(l => l.Id).WithAlias(() => resultAlias.Id)
					.Select(() => leafletNomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => flyerActionTimeAlias.StartDate).WithAlias(() => resultAlias.StartDate)
					.Select(() => flyerActionTimeAlias.EndDate).WithAlias(() => resultAlias.EndDate))
				.OrderBy(() => leafletNomenclatureAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<FlyersJournalNode>());

			return result;
		}
	}
}
