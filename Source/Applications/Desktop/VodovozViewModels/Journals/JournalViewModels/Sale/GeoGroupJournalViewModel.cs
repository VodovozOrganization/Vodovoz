using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Sale;
using Vodovoz.Models;
using Vodovoz.ViewModels.Dialogs.Sales;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Sale
{
	public class GeoGroupJournalViewModel : SingleEntityJournalViewModelBase<GeoGroup, GeoGroupViewModel, GeoGroupJournalNode>
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly INavigationManager _navigationManager;
		private readonly IWarehouseJournalFactory _warehouseJournalFactory;
		private readonly GeoGroupVersionsModel _geoGroupVersionsModel;

		public GeoGroupJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager,
			IWarehouseJournalFactory warehouseJournalFactory,
			GeoGroupVersionsModel geoGroupVersionsModel,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false
		) : base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_warehouseJournalFactory = warehouseJournalFactory ?? throw new ArgumentNullException(nameof(warehouseJournalFactory));
			_geoGroupVersionsModel = geoGroupVersionsModel ?? throw new ArgumentNullException(nameof(geoGroupVersionsModel));

			Title = "Части города";
		}

		public void DisableChangeEntityActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<GeoGroup>> ItemsSourceQueryFunction => uow => {
			GeoGroup geoGroupAlias = null;
			GeoGroupJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => geoGroupAlias);

			query.Where(GetSearchCriterion(
				() => geoGroupAlias.Name,
				() => geoGroupAlias.Id
			));

			return query
				.SelectList(list => list
				   .Select(x => x.Id).WithAlias(() => resultAlias.Id)
				   .Select(x => x.Name).WithAlias(() => resultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<GeoGroupJournalNode>());
		};

		protected override Func<GeoGroupViewModel> CreateDialogFunction => () =>
			new GeoGroupViewModel(
				EntityUoWBuilder.ForCreate(),
				_navigationManager,
				UnitOfWorkFactory,
				_geoGroupVersionsModel,
				_warehouseJournalFactory,
				commonServices,
				_lifetimeScope);

		protected override Func<GeoGroupJournalNode, GeoGroupViewModel> OpenDialogFunction => node =>
			new GeoGroupViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				_navigationManager,
				UnitOfWorkFactory,
				_geoGroupVersionsModel,
				_warehouseJournalFactory,
				commonServices,
				_lifetimeScope);
	}
}
