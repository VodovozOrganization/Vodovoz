using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.Project.Dialogs;
using QS.Project.Services;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz.ViewWidgets.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NonSerialEquipmentReceptionView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private readonly INomenclatureRepository _nomenclatureRepository =
			new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		private GenericObservableList<ReceptionNonSerialEquipmentItemNode> ReceptionNonSerialEquipmentList = new GenericObservableList<ReceptionNonSerialEquipmentItemNode>();
		private bool? _userHasOnlyAccessToWarehouseAndComplaints;

		public IList<ReceptionNonSerialEquipmentItemNode> Items {
			get {
				return ReceptionNonSerialEquipmentList;
			}
		}

		public NonSerialEquipmentReceptionView()
		{
			this.Build();

			ytreeEquipment.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionNonSerialEquipmentItemNode>()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Name)
				.AddColumn("Забирали").AddNumericRenderer(node => node.NeedReceptionCount)
				.AddColumn("Получено ")
				.AddNumericRenderer(node => node.Amount, false)
					.Adjustment(new Adjustment(0, 0, 10000, 1, 10, 10))
					.Editing()
				.AddColumn("")
				.Finish();
			
			ytreeEquipment.ItemsDataSource = ReceptionNonSerialEquipmentList;
		}

		RouteList routeList;
		public RouteList RouteList {
			get {
				return routeList;
			}
			set {
				if(routeList == value)
					return;
				routeList = value;
				if(routeList != null) {
					FillListEquipmentFromRoute();
				} else {
					ReceptionNonSerialEquipmentList.Clear();
				}

			}
		}

		void FillListEquipmentFromRoute()
		{
			ReceptionNonSerialEquipmentList.Clear();
			ReceptionNonSerialEquipmentItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature NomenclatureAlias = null;
			var equipmentItems = MyOrmDialog.UoW.Session.QueryOver<RouteListItem>()
														.Where(r => r.RouteList.Id == RouteList.Id)
														.JoinAlias(rli => rli.Order, () => orderAlias)
														.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
														.Where(() => orderEquipmentAlias.Direction == Domain.Orders.Direction.PickUp)
														.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => NomenclatureAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
														.SelectList(list => list
														   .SelectGroup(() => NomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
														   .Select(() => NomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
														   .SelectSum(() => orderEquipmentAlias.Count).WithAlias(() => resultAlias.NeedReceptionCount)
														)
														.TransformUsing(Transformers.AliasToBean<ReceptionNonSerialEquipmentItemNode>())
														.List<ReceptionNonSerialEquipmentItemNode>();

			foreach(var equipment in equipmentItems)
				ReceptionNonSerialEquipmentList.Add(equipment);
		}

		protected void OnButtonAddEquipmentClicked(object sender, EventArgs e)
		{
			var filter = new NomenclatureFilterViewModel();
			filter.RestrictCategory = NomenclatureCategory.equipment;

			var nomenclatureJournalFactory = new NomenclatureJournalFactory();
			var journal = nomenclatureJournalFactory.CreateNomenclaturesJournalViewModel();
			journal.FilterViewModel = filter;
			journal.OnEntitySelectedResult += Journal_OnEntitySelectedResult;
			journal.Title = "Оборудование";
			
			if(_userHasOnlyAccessToWarehouseAndComplaints == null)
			{
				_userHasOnlyAccessToWarehouseAndComplaints =
					ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
						"user_have_access_only_to_warehouse_and_complaints")
					&& !ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;
			}

			if(_userHasOnlyAccessToWarehouseAndComplaints.Value)
			{
				journal.HideButtons();
			}

			MyTab.TabParent.AddSlaveTab(MyTab, journal);
		}

		private void Journal_OnEntitySelectedResult(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
		{
			var selectedNode = e.SelectedNodes.FirstOrDefault();
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
			ReceptionNonSerialEquipmentList.Add(node);
		}

		void RefWin_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Nomenclature nomenclature = (e.Subject as Nomenclature);
			if(nomenclature == null) {
				return;
			}
			var node = new ReceptionNonSerialEquipmentItemNode() {
				NomenclatureCategory = nomenclature.Category,
				NomenclatureId = nomenclature.Id,
				Name = nomenclature.Name
			};
			ReceptionNonSerialEquipmentList.Add(node);
		}
	}

	public class ReceptionNonSerialEquipmentItemNode : PropertyChangedBase
	{
		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		public int NeedReceptionCount { get; set; }

		int amount;
		public virtual int Amount {
			get { return amount; }
			set {
				SetField(ref amount, value, () => Amount);
			}
		}

		int returned;
		public int Returned {
			get {
				return returned;
			}
			set {
				SetField(ref returned, value, () => Returned);
			}
		}
	}
}
