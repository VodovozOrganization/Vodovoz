using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gtk;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject;
using Vodovoz.Domain.Client;
using QS.Project.Services;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPointsManagementView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private IUnitOfWorkGeneric<Counterparty> _deliveryPointUoW;
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory = new DeliveryPointViewModelFactory();
		private bool _canDeletePermission;

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
			treeDeliveryPoints.ColumnsConfig = FluentColumnsConfig<DeliveryPointByClientJournalNode>.Create()
				.AddColumn("Адрес").AddTextRenderer(node => node.CompiledAddress).WrapMode(Pango.WrapMode.WordChar).WrapWidth(1000)
				.AddColumn("Номер").AddTextRenderer(x => x.Id.ToString())
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
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

			var client = DeliveryPointUoW.Root;
			var dpViewModel = _deliveryPointViewModelFactory.GetForCreationDeliveryPointViewModel(client);
			MyTab.TabParent.AddSlaveTab(MyTab, dpViewModel);
			dpViewModel.EntitySaved += (o, args) => UpdateNodes();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			var dpId = treeDeliveryPoints.GetSelectedObject<DeliveryPointByClientJournalNode>().Id;
			var dpViewModel = _deliveryPointViewModelFactory.GetForOpenDeliveryPointViewModel(dpId);
			MyTab.TabParent.AddSlaveTab(MyTab, dpViewModel);
			dpViewModel.EntitySaved += (o, args) => UpdateNodes();
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var deliveryPoint = treeDeliveryPoints.GetSelectedObject<DeliveryPointByClientJournalNode>();
			if(OrmMain.DeleteObject(typeof(DeliveryPoint), deliveryPoint.Id))
			{
				UpdateNodes();
			}
		}

		private void UpdateNodes()
		{
			DeliveryPoint deliveryPointAlias = null;
			DeliveryPointByClientJournalNode resultAlias = null;
			Counterparty counterpartyAlias = null;

			var query = DeliveryPointUoW.Session.QueryOver(() => deliveryPointAlias)
				.JoinAlias(c => c.Counterparty, () => counterpartyAlias)
				.Where(() => counterpartyAlias.Id == DeliveryPointUoW.Root.Id);

			var result = query
				.SelectList(list => list
					.Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompiledAddress)
					.Select(() => deliveryPointAlias.IsActive).WithAlias(() => resultAlias.IsActive)
				)
				.TransformUsing(Transformers.AliasToBean<DeliveryPointByClientJournalNode>())
				.List<DeliveryPointByClientJournalNode>();

			treeDeliveryPoints.SetItemsSource(result);
		}
	}
}
