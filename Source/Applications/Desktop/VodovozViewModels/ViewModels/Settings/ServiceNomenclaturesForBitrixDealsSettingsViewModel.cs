using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Settings.Common;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public class ServiceNomenclaturesForBitrixDealsSettingsViewModel : NamedDomainEntitiesSettingsViewModelBase
	{
		private readonly INavigationManager _navigationManager;

		public ServiceNomenclaturesForBitrixDealsSettingsViewModel(
			INavigationManager navigationManager,
			ICommonServices commonServices,
			IUnitOfWorkFactory unitOfWorkFactory,
			IGeneralSettings generalSettingsSettings,
			string parameterName
			) : base(commonServices, unitOfWorkFactory, generalSettingsSettings, parameterName)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		protected override void AddEntity()
		{
			var page = _navigationManager.OpenViewModel<NomenclaturesJournalViewModel>(null);
			page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += OnEntityToAddSelected;
		}

		protected override void GetEntitiesCollection()
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				NamedDomainObjectNode resultAlias = null;
				uow.Session.DefaultReadOnly = true;

				var nomenclatureIds = GeneralSettingsSettings.ServiceNomenclaturesForBitrixDeals;

				var nodes = uow.Session.QueryOver<Nomenclature>()
					.WhereRestrictionOn(n => n.Id).IsIn(nomenclatureIds)
					.SelectList(list => list
						.Select(w => w.Id).WithAlias(() => resultAlias.Id)
						.Select(w => w.Name).WithAlias(() => resultAlias.Name))
					.TransformUsing(Transformers.AliasToBean<NamedDomainObjectNode>())
					.List<INamedDomainObject>();

				ObservableEntities = new GenericObservableList<INamedDomainObject>(nodes);
			}
		}

		protected override void SaveEntities()
		{
			var ids = ObservableEntities.Select(x => x.Id).ToArray();
			GeneralSettingsSettings.UpdateServiceNomenclaturesForBitrixDeals(ids, ParameterName);
			CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Данные сохранены");
		}

		private void OnEntityToAddSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNodes = e.SelectedObjects.OfType<NomenclatureJournalNode>().ToArray();

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
