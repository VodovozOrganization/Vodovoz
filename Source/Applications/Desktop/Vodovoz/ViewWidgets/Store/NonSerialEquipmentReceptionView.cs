using Autofac;
using Gtk;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QSOrmProject;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewWidgets.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NonSerialEquipmentReceptionView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private readonly INomenclatureRepository _nomenclatureRepository =
			new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		private GenericObservableList<ReceptionNonSerialEquipmentItemNode> ReceptionNonSerialEquipmentList = new GenericObservableList<ReceptionNonSerialEquipmentItemNode>();
		private bool? _userHasOnlyAccessToWarehouseAndComplaints;

		public IList<ReceptionNonSerialEquipmentItemNode> Items
		{
			get
			{
				return ReceptionNonSerialEquipmentList;
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

			ytreeEquipment.ItemsDataSource = ReceptionNonSerialEquipmentList;
		}

		RouteList routeList;
		public RouteList RouteList
		{
			get
			{
				return routeList;
			}
			set
			{
				if(routeList == value)
					return;
				routeList = value;
				if(routeList != null)
				{
					FillListEquipmentFromRoute();
				}
				else
				{
					ReceptionNonSerialEquipmentList.Clear();
				}

			}
		}

		public CarUnloadDocumentDlg Container { get; internal set; }

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
			var page = (Container.NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
				Container,
				filter => filter.RestrictCategory = NomenclatureCategory.equipment,
				OpenPageOptions.AsSlave,
				vievModel =>
				{
					vievModel.OnSelectResult += Journal_OnEntitySelectedResult;
					vievModel.Title = "Оборудование";

					if(_userHasOnlyAccessToWarehouseAndComplaints == null)
					{
						_userHasOnlyAccessToWarehouseAndComplaints =
							ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
								"user_have_access_only_to_warehouse_and_complaints")
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
			ReceptionNonSerialEquipmentList.Add(node);
		}

		void RefWin_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
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
			ReceptionNonSerialEquipmentList.Add(node);
		}

		public override void Destroy()
		{
			base.Destroy();
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
		}
	}

	public class ReceptionNonSerialEquipmentItemNode : PropertyChangedBase
	{
		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		public int NeedReceptionCount { get; set; }

		int amount;
		public virtual int Amount
		{
			get { return amount; }
			set
			{
				SetField(ref amount, value, () => Amount);
			}
		}

		int returned;
		public int Returned
		{
			get
			{
				return returned;
			}
			set
			{
				SetField(ref returned, value, () => Returned);
			}
		}
	}
}
