using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain;
using Vodovoz.Domain.Store;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public class WarehousesSettingsViewModel : NamedDomainEntitiesSettingsViewModelBase
	{
		private readonly INavigationManager _navigationManager;

		public WarehousesSettingsViewModel(
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			IGeneralSettingsParametersProvider generalSettingsParametersProvider,
			string parameterName) : base(commonServices, unitOfWorkFactory, generalSettingsParametersProvider, parameterName)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}
		
		protected override void AddEntity()
		{
			var page = _navigationManager.OpenViewModel<WarehouseJournalViewModel>(null);
			page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			page.ViewModel.OnEntitySelectedResult += OnEntityToAddSelected;
		}
		
		protected override void SaveEntities()
		{
			var ids = ObservableEntities.Select(x => x.Id).ToArray();
			GeneralSettingsParametersProvider.UpdateWarehousesIdsForParameter(ids, ParameterName);
			CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Данные сохранены");
		}

		protected override void GetEntitiesCollection()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				NamedDomainObjectNode resultAlias = null;
				uow.Session.DefaultReadOnly = true;

				var warehousesIds = GeneralSettingsParametersProvider.WarehousesForPricesAndStocksIntegration;

				var nodes = uow.Session.QueryOver<Warehouse>()
					.WhereRestrictionOn(w => w.Id).IsIn(warehousesIds)
					.SelectList(list => list
						.Select(w => w.Id).WithAlias(() => resultAlias.Id)
						.Select(w => w.Name).WithAlias(() => resultAlias.Name))
					.TransformUsing(Transformers.AliasToBean<NamedDomainObjectNode>())
					.List<INamedDomainObject>();

				ObservableEntities = new GenericObservableList<INamedDomainObject>(nodes);
			}
		}

		private void OnEntityToAddSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedNodes = e.SelectedNodes.OfType<WarehouseJournalNode>().ToArray();

			if(!selectedNodes.Any())
			{
				return;
			}
			
			foreach(var selectedNode in selectedNodes)
			{
				var node = ObservableEntities.SingleOrDefault(x => x.Id == selectedNode.Id);

				if(node != null)
				{
					return;
				}

				ObservableEntities.Add(new NamedDomainObjectNode
				{
					Id = selectedNode.Id,
					Name = selectedNode.Name
				});
			}
		}
	}
}
