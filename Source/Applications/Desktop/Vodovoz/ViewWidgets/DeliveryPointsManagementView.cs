using Autofac;
using Gamma.ColumnConfig;
using Gtk;
using QS.Project.Services;
using QS.Services;
using QS.Utilities;
using QSOrmProject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Factories;
using Vodovoz.Infrastructure;

namespace Vodovoz
{
	[ToolboxItem(true)]
	public partial class DeliveryPointsManagementView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private Counterparty _counterparty;
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private readonly IPermissionResult _permissionResult;
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory;
		private readonly bool _canDeleteByPresetPermission;
		private IDeliveryPointRepository _deliveryPointRepository;

		public DeliveryPointsManagementView()
		{
			_deliveryPointRepository = _lifetimeScope.Resolve<IDeliveryPointRepository>();
			Build();

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

			_deliveryPointViewModelFactory = new DeliveryPointViewModelFactory(_lifetimeScope);
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

			dpViewModel.EntitySaved += (o, args) => UpdateNodesAndSelectEditedRow();
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

		private void UpdateNodesAndSelectEditedRow()
		{
			Gtk.Application.Invoke((s, arg) =>
			{
				var selectedDeliveryPoint = treeDeliveryPoints.GetSelectedObject<DeliveryPoint>();
				var scrollPosition = GtkScrolledWindow?.Vadjustment?.Value ?? 0;

				UpdateNodes();

				GtkHelper.WaitRedraw();

				MoveScrollToPosition(scrollPosition);

				if(selectedDeliveryPoint != null)
				{
					SelectDeliveryPointRow(selectedDeliveryPoint);
				}
			});
		}

		private void SelectDeliveryPointRow(DeliveryPoint deliveryPoint)
		{
			if(treeDeliveryPoints.ItemsDataSource is IList<DeliveryPoint> deliveryPoints)
			{
				if(treeDeliveryPoints.Model == null)
				{
					return;
				}

				treeDeliveryPoints.SelectedRow =
						deliveryPoints.Any(dp => dp.Id == deliveryPoint.Id)
						? deliveryPoint
						: null;
			}
		}

		private void MoveScrollToPosition(double scrollPosition)
		{
			if(GtkScrolledWindow?.Vadjustment != null)
			{
				GtkScrolledWindow.Vadjustment.Value =
					scrollPosition > GtkScrolledWindow.Vadjustment.Upper
					? GtkScrolledWindow.Vadjustment.Upper
					: scrollPosition;
			}
		}

		public override void Destroy()
		{
			_deliveryPointRepository = null;
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
