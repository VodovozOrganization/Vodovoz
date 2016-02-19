using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModel
{
	public class NomenclatureForSaleVM:RepresentationModelWithoutEntityBase<NomenclatureForSaleVMNode>
	{
		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes ()
		{
			Nomenclature nomenclatureAlias = null;
			MeasurementUnits unitAlias = null;
			NomenclatureForSaleVMNode resultAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;

			var subqueryAdded = QueryOver.Of<WarehouseMovementOperation> (() => operationAddAlias)
				.Where (() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where (Restrictions.IsNotNull (Projections.Property<WarehouseMovementOperation> (o => o.IncomingWarehouse)))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			var subqueryRemoved = QueryOver.Of<WarehouseMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull (Projections.Property<WarehouseMovementOperation> (o => o.WriteoffWarehouse)))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			var subqueryReserved = QueryOver.Of<Vodovoz.Domain.Orders.Order> (() => orderAlias)
				.JoinAlias (() => orderAlias.OrderItems, () => orderItemsAlias)
				.Where (()=>orderItemsAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(()=>orderAlias.OrderStatus==OrderStatus.Accepted)
				.Select (Projections.Sum (() => orderItemsAlias.Count));

			var items = UoW.Session.QueryOver<Nomenclature>(()=>nomenclatureAlias)
				.Where(Restrictions.In(Projections.Property(()=>nomenclatureAlias.Category),Nomenclature.GetCategoriesForSale()))
				.JoinAlias(()=>nomenclatureAlias.Unit,()=>unitAlias).Where(()=>!nomenclatureAlias.Serial)
				.SelectList(list=>list
					.SelectGroup(()=>nomenclatureAlias.Id).WithAlias(()=>resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(()=>nomenclatureAlias.Category).WithAlias(()=>resultAlias.Category)
					.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
					.SelectSubQuery (subqueryAdded).WithAlias(() => resultAlias.Added)
					.SelectSubQuery (subqueryRemoved).WithAlias(() => resultAlias.Removed)
					.SelectSubQuery(subqueryReserved).WithAlias(()=>resultAlias.Reserved)
				)
				.TransformUsing(Transformers.AliasToBean<NomenclatureForSaleVMNode>())
				.List<NomenclatureForSaleVMNode>();

			var equipment = Repository.EquipmentRepository.AvailableEquipmentQuery()
				.GetExecutableQueryOver(UoW.Session)
				.Where (eq => !eq.OnDuty)
				.JoinAlias (eq => eq.Nomenclature, () => nomenclatureAlias)
				.JoinAlias (() => nomenclatureAlias.Unit, () => unitAlias)
				.SelectList(list=>list
					.SelectGroup(()=>nomenclatureAlias.Id).WithAlias(()=>resultAlias.Id)
					.Select(()=>true).WithAlias(()=>resultAlias.IsEquipmentWithSerial)
					.SelectSum(()=>(decimal)1).WithAlias(()=>resultAlias.Added)
					.SelectSubQuery(subqueryReserved).WithAlias(()=>resultAlias.Reserved)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(()=>nomenclatureAlias.Category).WithAlias(()=>resultAlias.Category)
					.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
				)
				.TransformUsing(Transformers.AliasToBean<NomenclatureForSaleVMNode>())
				.List<NomenclatureForSaleVMNode>();
			
			List<NomenclatureForSaleVMNode> forSale = new List<NomenclatureForSaleVMNode>();
			forSale.AddRange (items);
			forSale.AddRange (equipment);
			SetItemsSource (forSale);
		}

		static Gdk.Color colorBlack = new Gdk.Color (0, 0, 0);
		static Gdk.Color colorRed = new Gdk.Color (0xff, 0, 0);

		IColumnsConfig columnsConfig = FluentColumnsConfig <NomenclatureForSaleVMNode>.Create ()
			.AddColumn ("Номенклатура").SetDataProperty (node => node.Name)
			.AddColumn ("Категория").SetDataProperty (node => node.Category.GetEnumTitle())
			.AddColumn ("Кол-во").AddTextRenderer (node => node.InStockText)
			.AddColumn ("Зарезервировано").AddTextRenderer (node => node.ReservedText)
			.AddColumn ("Доступно").AddTextRenderer (node => node.AvailableText)
			.AddSetter ((cell, node) => cell.ForegroundGdk = node.Available > 0 ? colorBlack : colorRed)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		public NomenclatureForSaleVM () 
			: this(UnitOfWorkFactory.CreateWithoutRoot ()) 
		{}

		public NomenclatureForSaleVM (IUnitOfWork uow) : base(typeof(Nomenclature), typeof(WarehouseMovementOperation))
		{
			this.UoW = uow;
		}

		#region implemented abstract members of RepresentationModelWithoutEntityBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return true;
		}

		#endregion
	}

	public class NomenclatureForSaleVMNode
	{
		public int Id{get;set;}

		[UseForSearch]
		public string Name{get;set;}
		public NomenclatureCategory Category{ get; set; }
		public decimal InStock{ get{ return Added - Removed; } }
		public int Reserved{ get; set; }
		public decimal Available{get{ return InStock - Reserved; }}
		public decimal Added{ get; set; }
		public decimal Removed{ get; set; }
		public string UnitName{ get; set;}
		public short UnitDigits{ get; set;}
		public bool IsEquipmentWithSerial{ get; set; }
		private string Format(decimal value)
		{
			return String.Format ("{0:F" + UnitDigits + "} {1}", value, UnitName);
		}

		public string InStockText{get{ return Format(InStock);} }
		public string ReservedText{get{ return Format(Reserved);} }
		public string AvailableText{get{ return Format(Available);} }

	
	}
}

