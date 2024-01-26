using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;
using static Vodovoz.ViewModels.Journals.JournalViewModels.Logistic.RouteColumnJournalViewModel;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class RouteColumnJournalViewModel : SingleEntityJournalViewModelBase<RouteColumn, RouteColumnViewModel, RouteColumnJournalNode>
	{
		public RouteColumnJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false)
			: base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog, navigationManager)
		{
			TabName = "Колонки в маршрутном листе";

			UpdateOnChanges(typeof(RouteColumn));
		}

		protected override Func<IUnitOfWork, IQueryOver<RouteColumn>> ItemsSourceQueryFunction => (uow) =>
		{
			RouteColumnJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<RouteColumn>()
				.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				.Select(x => x.ShortName).WithAlias(() => resultAlias.ShortName)
				.Select(x => x.IsHighlighted).WithAlias(() => resultAlias.IsHighlighted))
				.TransformUsing(Transformers.AliasToBean<RouteColumnJournalNode>()).OrderBy(x => x.Name).Asc;

			query.Where(
			GetSearchCriterion<RouteColumn>(
				x => x.Id,
				x => x.Name,
				x => x.ShortName
				)
			);

			return query;
		};

		protected override Func<RouteColumnViewModel> CreateDialogFunction => () =>
			NavigationManager.OpenViewModel<RouteColumnViewModel>(null).ViewModel;

		protected override Func<RouteColumnJournalNode, RouteColumnViewModel> OpenDialogFunction => selectedNode =>
			NavigationManager.OpenViewModel<RouteColumnViewModel, IEntityUoWBuilder>(null, EntityUoWBuilder.ForOpen(selectedNode.Id)).ViewModel;

		public class RouteColumnJournalNode : JournalEntityNodeBase<RouteColumn>
		{
			public string Name { get; set; }
			public string ShortName { get; set; }
			public bool IsHighlighted { get; set; }

			public override string Title => Name;
		}
	}
}
