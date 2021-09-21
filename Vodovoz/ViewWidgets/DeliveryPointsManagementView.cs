using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gtk;
using QSOrmProject;
using Vodovoz.Domain.Client;
using QS.Project.Services;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Factories;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPointsManagementView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private Counterparty _counterparty;
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory = new DeliveryPointViewModelFactory();
		private readonly bool _canDeletePermission;
		private readonly IDeliveryPointRepository _deliveryPointRepository = new DeliveryPointRepository();

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

		private bool CanDelete()
		{
			return _canDeletePermission && treeDeliveryPoints.Selection.CountSelectedRows() > 0;
		}

		public DeliveryPointsManagementView()
		{
			this.Build();

			treeDeliveryPoints.Selection.Changed += OnSelectionChanged;
			treeDeliveryPoints.RowActivated += (o, args) => buttonEdit.Click();
			treeDeliveryPoints.ColumnsConfig = FluentColumnsConfig<DeliveryPoint>.Create()
				.AddColumn("Адрес").AddTextRenderer(node => node.CompiledAddress).WrapMode(Pango.WrapMode.WordChar).WrapWidth(1000)
				.AddColumn("Номер").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.IsActive ? "black" : "red")
				.Finish();
			_canDeletePermission =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete_counterparty_and_deliverypoint");
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
			var result = _deliveryPointRepository.GetDeliveryPointsByClientId(UoW, _counterparty.Id);
			treeDeliveryPoints.SetItemsSource(result);
		}
	}
}
