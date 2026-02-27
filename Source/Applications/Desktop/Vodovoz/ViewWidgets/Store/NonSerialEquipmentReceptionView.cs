using Autofac;
using Gtk;
using NHibernate.Transform;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QSOrmProject;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewWidgets.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NonSerialEquipmentReceptionView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private GenericObservableList<ReceptionNonSerialEquipmentItemNode> _receptionNonSerialEquipmentList = new GenericObservableList<ReceptionNonSerialEquipmentItemNode>();
		private bool? _userHasOnlyAccessToWarehouseAndComplaints;

		public IList<ReceptionNonSerialEquipmentItemNode> Items
		{
			get
			{
				return _receptionNonSerialEquipmentList;
			}
		}

		public NonSerialEquipmentReceptionView()
		{
			Build();

			ytreeEquipment.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionNonSerialEquipmentItemNode>()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Name)
				.AddColumn("Забирали").AddNumericRenderer(node => node.NeedReceptionCount)
				.AddColumn("Получено ")
				.AddNumericRenderer(node => node.Amount, false)
					.Adjustment(new Adjustment(0, 0, 10000, 1, 10, 10))
					.Editing()
				.AddColumn("")
				.Finish();

			ytreeEquipment.ItemsDataSource = _receptionNonSerialEquipmentList;
		}

		private RouteList _routeList;
		public RouteList RouteList
		{
			get
			{
				return _routeList;
			}
			set
			{
				if(_routeList == value)
				{
					return;
				}

				_routeList = value;
				if(_routeList != null)
				{
					FillListEquipmentFromRoute();
				}
				else
				{
					_receptionNonSerialEquipmentList.Clear();
				}

			}
		}

		public CarUnloadDocumentDlg Container { get; internal set; }

		private void FillListEquipmentFromRoute()
		{
			_receptionNonSerialEquipmentList.Clear();
			ReceptionNonSerialEquipmentItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature NomenclatureAlias = null;
			var equipmentItems = MyOrmDialog.UoW.Session.QueryOver<RouteListItem>()
				.Where(r => r.RouteList.Id == RouteList.Id)
				.JoinAlias(rli => rli.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.Where(() => orderEquipmentAlias.Direction == Core.Domain.Orders.Direction.PickUp)
				.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => NomenclatureAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList(list => list
					.SelectGroup(() => NomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => NomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.SelectSum(() => orderEquipmentAlias.Count).WithAlias(() => resultAlias.NeedReceptionCount)
				)
				.TransformUsing(Transformers.AliasToBean<ReceptionNonSerialEquipmentItemNode>())
				.List<ReceptionNonSerialEquipmentItemNode>();

			foreach(var equipment in equipmentItems)
			{
				_receptionNonSerialEquipmentList.Add(equipment);
			}
		}

		protected void OnButtonAddEquipmentClicked(object sender, EventArgs e)
		{
			var page = (Container.NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
				Container,
				filter => filter.RestrictCategory = NomenclatureCategory.equipment,
				OpenPageOptions.AsSlave,
				vievModel =>
				{
					vievModel.OnSelectResult += Journal_OnEntitySelectedResult;
					vievModel.Title = "Оборудование";
					vievModel.SelectionMode = JournalSelectionMode.Single;

					if(_userHasOnlyAccessToWarehouseAndComplaints == null)
					{
						_userHasOnlyAccessToWarehouseAndComplaints =
							ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
								Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
							&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;
					}

					if(_userHasOnlyAccessToWarehouseAndComplaints.Value)
					{
						vievModel.HideButtons();
					}
				});
		}

		private void Journal_OnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();

			if(selectedNode == null)
			{
				return;
			}

			var selectedNomenclature = UoW.GetById<Nomenclature>(selectedNode.Id);
			var node = new ReceptionNonSerialEquipmentItemNode()
			{
				NomenclatureCategory = selectedNomenclature.Category,
				NomenclatureId = selectedNomenclature.Id,
				Name = selectedNomenclature.Name
			};

			_receptionNonSerialEquipmentList.Add(node);
		}

		private void RefWin_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Nomenclature nomenclature = (e.Subject as Nomenclature);
			if(nomenclature == null)
			{
				return;
			}
			var node = new ReceptionNonSerialEquipmentItemNode()
			{
				NomenclatureCategory = nomenclature.Category,
				NomenclatureId = nomenclature.Id,
				Name = nomenclature.Name
			};
			_receptionNonSerialEquipmentList.Add(node);
		}

		public override void Destroy()
		{
			base.Destroy();
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
		}
	}
}
