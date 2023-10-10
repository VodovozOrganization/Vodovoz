using System;
using System.Collections.Generic;
using Fias.Client;
using Fias.Client.Cache;
using Gamma.ColumnConfig;
using Gtk;
using QSOrmProject;
using Vodovoz.Domain.Client;
using QS.Project.Services;
using QS.Services;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using QS.DomainModel.UoW;
using Vodovoz.Infrastructure;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPointsManagementView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private Counterparty _counterparty;
		private readonly IPermissionResult _permissionResult;
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory;
		private readonly bool _canDeleteByPresetPermission;
		private readonly IDeliveryPointRepository _deliveryPointRepository = new DeliveryPointRepository();

		public DeliveryPointsManagementView()
		{
			this.Build();

			treeDeliveryPoints.ColumnsConfig = FluentColumnsConfig<DeliveryPoint>.Create()
				.AddColumn("Адрес").AddTextRenderer(node => node.CompiledAddress).WrapMode(Pango.WrapMode.WordChar).WrapWidth(1000)
				.AddColumn("Номер").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Фикс. цены").AddToggleRenderer(node => node.HasFixedPrices).Editing(false)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsActive ? GdkColors.PrimaryText : GdkColors.DangerText)
				.Finish();
			_canDeleteByPresetPermission =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete_counterparty_and_deliverypoint");
			_permissionResult = ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DeliveryPoint));
			
			buttonAdd.Sensitive = _permissionResult.CanCreate;

			if(_permissionResult.CanRead)
			{
				treeDeliveryPoints.RowActivated += (o, args) => buttonEdit.Click();
			}
			treeDeliveryPoints.Selection.Changed += OnSelectionChanged;
			
			IParametersProvider parametersProvider = new ParametersProvider();
			IFiasApiParametersProvider fiasApiParametersProvider = new FiasApiParametersProvider(parametersProvider);
			var geoCoderCache = new GeocoderCache(UnitOfWorkFactory.GetDefaultFactory);
			IFiasApiClient fiasApiClient = new FiasApiClient(fiasApiParametersProvider.FiasApiBaseUrl, fiasApiParametersProvider.FiasApiToken, geoCoderCache);
			_deliveryPointViewModelFactory = new DeliveryPointViewModelFactory(fiasApiClient);
		}
		
		public Counterparty Counterparty
		{
			set
			{
				if(_counterparty == value)
				{
					return;
				}
				_counterparty = value;
				if(_counterparty.DeliveryPoints == null)
				{
					_counterparty.DeliveryPoints = new List<DeliveryPoint>();
				}
				UpdateNodes();
			}
		}
		
		private void OnSelectionChanged(object sender, EventArgs e)
		{
			var selected = treeDeliveryPoints.Selection.CountSelectedRows() > 0;
			buttonEdit.Sensitive = selected && _permissionResult.CanRead;
			buttonDelete.Sensitive = selected && _permissionResult.CanDelete && _canDeleteByPresetPermission;
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

			var dpViewModel = _deliveryPointViewModelFactory.GetForCreationDeliveryPointViewModel(_counterparty);
			MyTab.TabParent.AddSlaveTab(MyTab, dpViewModel);
			dpViewModel.EntitySaved += (o, args) => UpdateNodes();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			var dpId = treeDeliveryPoints.GetSelectedObject<DeliveryPoint>().Id;
			var dpViewModel = _deliveryPointViewModelFactory.GetForOpenDeliveryPointViewModel(dpId);
			MyTab.TabParent.AddSlaveTab(MyTab, dpViewModel);
			dpViewModel.EntitySaved += (o, args) => UpdateNodes();
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var deliveryPoint = treeDeliveryPoints.GetSelectedObject<DeliveryPoint>();
			if(OrmMain.DeleteObject(typeof(DeliveryPoint), deliveryPoint.Id))
			{
				UpdateNodes();
			}
		}

		private void UpdateNodes()
		{
			var result = _deliveryPointRepository.GetDeliveryPointsByCounterpartyId(UoW, _counterparty.Id);
			treeDeliveryPoints.SetItemsSource(result);
		}
	}
}
