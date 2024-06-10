using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using System;
using QS.Navigation;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Orders;
using VodovozInfrastructure.StringHandlers;
using System.Linq;

namespace Vodovoz.JournalViewModels
{
	public class PromotionalSetsJournalViewModel : SingleEntityJournalViewModelBase<PromotionalSet, PromotionalSetViewModel, PromotionalSetJournalNode>
	{
		private readonly ILifetimeScope _lifetimeScope;
		
		public PromotionalSetsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false)
			: base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog, navigationManager)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			
			TabName = "Промонаборы";
			UseSlider = false;

			var threadLoader = DataLoader as ThreadDataLoader<PromotionalSetJournalNode>;
			threadLoader.MergeInOrderBy(x => x.IsArchive, false);
			threadLoader.MergeInOrderBy(x => x.Id, false);

			UpdateOnChanges(typeof(PromotionalSet));
		}

		protected override Func<IUnitOfWork, IQueryOver<PromotionalSet>> ItemsSourceQueryFunction => (uow) => {
			PromotionalSetJournalNode resultAlias = null;
			DiscountReason reasonAlias = null;

			var query = uow.Session.QueryOver<PromotionalSet>();
			query.Where(
				GetSearchCriterion<PromotionalSet>(
					x => x.Id
				)
			);

			var result = query.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
				.Select(x => x.Name).WithAlias(() => resultAlias.Name)
				)
				.TransformUsing(Transformers.AliasToBean<PromotionalSetJournalNode>())
				.OrderBy(x => x.Name).Asc;
			return result;
		};

		protected override Func<PromotionalSetViewModel> CreateDialogFunction => () => new PromotionalSetViewModel(
			EntityUoWBuilder.ForCreate(),
			UnitOfWorkFactory,
			commonServices,
			NavigationManager,
			_lifetimeScope.Resolve<IStringHandler>(),
			_lifetimeScope
		);

		protected override Func<PromotionalSetJournalNode, PromotionalSetViewModel> OpenDialogFunction => node => new PromotionalSetViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			UnitOfWorkFactory,
			commonServices,
			NavigationManager,
			_lifetimeScope.Resolve<IStringHandler>(),
			_lifetimeScope
	   	);

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateCustomEditAction();
		}

		private void CreateCustomEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<PromotionalSetJournalNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}

					var selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}

					var config = EntityConfigs[selectedNode.EntityType];

					return config.PermissionResult.CanUpdate || config.PermissionResult.CanRead;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<PromotionalSetJournalNode>();

					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}

					var selectedNode = selectedNodes.First();

					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}

					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);

					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				}
			);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			NodeActionsList.Add(editAction);
		}
	}
}
