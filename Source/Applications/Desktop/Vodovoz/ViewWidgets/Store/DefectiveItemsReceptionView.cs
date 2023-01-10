﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using NHibernate.Util;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Project.Services;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz.ViewWidgets.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DefectiveItemsReceptionView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		GenericObservableList<DefectiveItemNode> defectiveList = new GenericObservableList<DefectiveItemNode>();
		private bool? _userHasOnlyAccessToWarehouseAndComplaints;
		
		public IList<DefectiveItemNode> Items => defectiveList;

		public void AddItem(DefectiveItemNode item) => defectiveList.Add(item);

		public DefectiveItemsReceptionView()
		{
			this.Build();

			List<CullingCategory> types;
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				types = uow.GetAll<CullingCategory>().OrderBy(c => c.Name).ToList();
			}
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			var colorLightRed = new Gdk.Color(0xff, 0x66, 0x66);

			ytreeReturns.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<DefectiveItemNode>()
				.AddColumn ("Номенклатура").AddTextRenderer (node => node.Name)
				.AddColumn ("Кол-во").AddNumericRenderer (node => node.Amount)
				.Adjustment (new Adjustment (0, 0, 9999, 1, 100, 0))
				.Editing (true)
				.AddColumn("Тип брака")
					.AddComboRenderer(x => x.TypeOfDefect)
					.SetDisplayFunc(x => x.Name)
					.FillItems(types)
					.AddSetter(
						(c, n) => {
							c.Editable = true;
							c.BackgroundGdk = n.TypeOfDefect == null
								? colorLightRed
								: colorWhite;
						}
					)
				.AddColumn("Источник\nбрака")
					.AddEnumRenderer(x => x.Source, true, new Enum[] { DefectSource.None })
					.AddSetter(
						(c, n) => {
							c.Editable = true;
							c.BackgroundGdk = n.Source == DefectSource.None
								? colorLightRed
								: colorWhite;
						}
					)
				.AddColumn("")
				.Finish ();

			ytreeReturns.ItemsDataSource = defectiveList;
		}

		private IUnitOfWork uow;

		public IUnitOfWork UoW {
			get {
				return uow;
			}
			set {
				if(uow == value)
					return;
				uow = value;
			}
		}

		Warehouse warehouse;
		public Warehouse Warehouse {
			get {
				return warehouse;
			}
			set {
				warehouse = value;
				FillDefectiveListFromRoute();
			}
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
					FillDefectiveListFromRoute();
				} else {
					defectiveList.Clear();
				}

			}
		}

		public new bool Sensitive {
			set => ytreeReturns.Sensitive = buttonAddNomenclature.Sensitive = value;
		}

		void FillDefectiveListFromRoute()
		{
			if(Warehouse == null || RouteList == null)
				return;

			DefectiveItemNode resultAlias = null;
			WarehouseMovementOperation warehouseMovementOperationAlias = null;
			CarUnloadDocument carUnloadDocumentAlias = null;
			CarUnloadDocumentItem carUnloadDocumentItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			Warehouse warehouseAlias = null;

			var defectiveItems = UoW.Session.QueryOver<CarUnloadDocumentItem>(() => carUnloadDocumentItemAlias)
									.Left.JoinAlias(() => carUnloadDocumentItemAlias.Document, () => carUnloadDocumentAlias)
									.Where(() => carUnloadDocumentAlias.RouteList.Id == RouteList.Id)
									.Left.JoinAlias(() => carUnloadDocumentItemAlias.WarehouseMovementOperation, () => warehouseMovementOperationAlias)
									.Left.JoinAlias(() => warehouseMovementOperationAlias.Nomenclature, () => nomenclatureAlias)
									.Where(() => nomenclatureAlias.IsDefectiveBottle)
									.SelectList(
										list => list
										.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
										.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
										.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
				                        .Select(() => warehouseMovementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
				                        .Select(() => carUnloadDocumentItemAlias.WarehouseMovementOperation).WithAlias(() => resultAlias.MovementOperation)
				                        .Select(() => carUnloadDocumentItemAlias.DefectSource).WithAlias(() => resultAlias.Source)
				                        .Select(() => carUnloadDocumentItemAlias.TypeOfDefect).WithAlias(() => resultAlias.TypeOfDefect)
									   )
									.TransformUsing(Transformers.AliasToBean<DefectiveItemNode>())
									.List<DefectiveItemNode>();

			defectiveItems.ForEach(i => defectiveList.Add(i));
		}

		protected void OnButtonAddNomenclatureClicked(object sender, EventArgs e)
		{
			var filter = new NomenclatureFilterViewModel();
			filter.IsDefectiveBottle = true;

			var nomenclatureJournalFactory = new NomenclatureJournalFactory();
			var journal = nomenclatureJournalFactory.CreateNomenclaturesJournalViewModel(filter, true);
			journal.OnEntitySelectedResult += Journal_OnEntitySelectedResult;
			
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
			if(!e.SelectedNodes.Any())
			{
				return;
			}

			var nomenclatures = UoW.GetById<Nomenclature>(e.SelectedNodes.Select(x => x.Id));
			foreach(var nomenclature in nomenclatures)
			{
				defectiveList.Add(new DefectiveItemNode(nomenclature, 0));
			}
		}
	}

	public class DefectiveItemNode : PropertyChangedBase
	{
		public DefectiveItemNode(Nomenclature nomenclature, int amount)
		{
			Name = nomenclature.Name;
			NomenclatureId = nomenclature.Id;
			NomenclatureCategory = nomenclature.Category;
			this.amount = amount;
		}

		public DefectiveItemNode(WarehouseMovementOperation movementOperation) : this(movementOperation.Nomenclature, (int)movementOperation.Amount)
		{
			this.movementOperation = movementOperation;
		}

		public DefectiveItemNode(CarUnloadDocumentItem carUnloadDocumentItem) : this(carUnloadDocumentItem.WarehouseMovementOperation)
		{
			this.carUnloadDocumentItem = carUnloadDocumentItem;
		}

		public DefectiveItemNode(){}

		CarUnloadDocumentItem carUnloadDocumentItem = new CarUnloadDocumentItem();
		public virtual CarUnloadDocumentItem CarUnloadDocumentItem {
			get { return carUnloadDocumentItem; }
			set { SetField(ref carUnloadDocumentItem, value, () => CarUnloadDocumentItem); }
		}

		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		decimal amount;
		public virtual decimal Amount {
			get { return amount; }
			set { SetField(ref amount, value, () => Amount); }
		}

		WarehouseMovementOperation movementOperation;
		public virtual WarehouseMovementOperation MovementOperation {
			get { return movementOperation; }
			set { SetField(ref movementOperation, value, () => MovementOperation); }
		}

		CullingCategory typeOfDefect;
		[Display(Name = "Тип брака")]
		public virtual CullingCategory TypeOfDefect {
			get { return typeOfDefect; }
			set { SetField(ref typeOfDefect, value, () => TypeOfDefect); }
		}

		DefectSource source = DefectSource.Driver;
		[Display(Name = "Источник брака")]
		public virtual DefectSource Source {
			get { return source; }
			set { SetField(ref source, value, () => Source); }
		}
	}
}
