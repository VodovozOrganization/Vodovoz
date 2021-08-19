using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Osm;
using QS.Osm.Loaders;
using QS.Project.Domain;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModel;
using QS.Project.Services;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPointsManagementView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private IUnitOfWorkGeneric<Counterparty> _deliveryPointUoW;

		public IUnitOfWorkGeneric<Counterparty> DeliveryPointUoW
		{
			get => _deliveryPointUoW;
			set
			{
				if(_deliveryPointUoW == value)
				{
					return;
				}

				_deliveryPointUoW = value;
				if(DeliveryPointUoW.Root.DeliveryPoints == null)
				{
					DeliveryPointUoW.Root.DeliveryPoints = new List<DeliveryPoint>();
				}

				treeDeliveryPoints.RepresentationModel = new ClientDeliveryPointsVM(value);
				treeDeliveryPoints.RepresentationModel.UpdateNodes();
			}
		}

		private bool CanDelete()
		{
			return ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
				"can_delete_counterparty_and_deliverypoint")
			       && treeDeliveryPoints.Selection.CountSelectedRows() > 0;
		}


		public DeliveryPointsManagementView()
		{
			this.Build();

			treeDeliveryPoints.Selection.Changed += OnSelectionChanged;
		}

		private void OnSelectionChanged(object sender, EventArgs e)
		{
			var selected = treeDeliveryPoints.Selection.CountSelectedRows() > 0;
			buttonEdit.Sensitive = selected;
			buttonDelete.Sensitive = CanDelete();
		}

		private void OnButtonAddClicked(object sender, EventArgs e)
		{
			if(MyOrmDialog.UoW.IsNew)
			{
				if(CommonDialogs.SaveBeforeCreateSlaveEntity(MyEntityDialog.EntityObject.GetType(), typeof(DeliveryPoint)))
				{
					if(!MyTdiDialog.Save())
					{
						return;
					}
				}
				else
				{
					return;
				}
			}

			var client = DeliveryPointUoW.Root;
			var dpViewModel = new DeliveryPointViewModel(client, new UserRepository(), new GtkTabsOpener(),
				new PhoneRepository(), ContactParametersProvider.Instance,
				new CitiesDataLoader(OsmWorker.GetOsmService()), new StreetsDataLoader(OsmWorker.GetOsmService()),
				new HousesDataLoader(OsmWorker.GetOsmService()),
				new NomenclatureSelectorFactory(),
				new NomenclatureFixedPriceController(new NomenclatureFixedPriceFactory(),
					new WaterFixedPricesGenerator(new NomenclatureRepository(new NomenclatureParametersProvider(_parametersProvider)))),
				EntityUoWBuilder.ForCreate(), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			MyTab.TabParent.AddSlaveTab(MyTab, dpViewModel);
			treeDeliveryPoints.RepresentationModel.UpdateNodes();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			var dpId = treeDeliveryPoints.GetSelectedObjects<ClientDeliveryPointVMNode>()[0].Id;
			var dpViewModel = new DeliveryPointViewModel(new UserRepository(), new GtkTabsOpener(), new PhoneRepository(),
				ContactParametersProvider.Instance,
				new CitiesDataLoader(OsmWorker.GetOsmService()), new StreetsDataLoader(OsmWorker.GetOsmService()),
				new HousesDataLoader(OsmWorker.GetOsmService()),
				new NomenclatureSelectorFactory(),
				new NomenclatureFixedPriceController(new NomenclatureFixedPriceFactory(),
					new WaterFixedPricesGenerator(new NomenclatureRepository(new NomenclatureParametersProvider(_parametersProvider)))),
				EntityUoWBuilder.ForOpen(dpId), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			MyTab.TabParent.AddSlaveTab(MyTab, dpViewModel);
		}

		protected void OnTreeDeliveryPointsRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click();
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			if(OrmMain.DeleteObject(typeof(DeliveryPoint), treeDeliveryPoints.GetSelectedObject<DeliveryPoint>().Id))
			{
				treeDeliveryPoints.RepresentationModel.UpdateNodes();
			}
		}
	}
}
