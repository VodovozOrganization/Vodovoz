using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;
using static Vodovoz.ViewModels.Journals.JournalViewModels.Logistic.RouteColumnJournalViewModel;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public partial class RouteColumnJournalViewModel : EntityJournalViewModelBase<RouteColumn, RouteColumnViewModel, RouteColumnJournalNode>
	{
		public RouteColumnJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			TabName = "Колонки в маршрутном листе";

			UpdateOnChanges(typeof(RouteColumn));

			UseSlider = true;
		}

		protected override IQueryOver<RouteColumn> ItemsQuery(IUnitOfWork unitOfWork)
		{
			RouteColumnJournalNode resultAlias = null;

			var query = unitOfWork.Session.QueryOver<RouteColumn>();

			query.Where(
				GetSearchCriterion<RouteColumn>(
					x => x.Id,
					x => x.Name,
					x => x.ShortName
					)
				);

			return query.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				.Select(x => x.ShortName).WithAlias(() => resultAlias.ShortName)
				.Select(x => x.IsHighlighted).WithAlias(() => resultAlias.IsHighlighted))
				.TransformUsing(Transformers.AliasToBean<RouteColumnJournalNode>()).OrderBy(x => x.Name).Asc;
		}
	}
}
