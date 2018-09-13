using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSBusinessCommon.Domain;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModel
{
	public class StockBalanceVM : RepresentationModelWithoutEntityBase<StockBalanceVMNode>
	{
		public StockBalanceFilter Filter {
			get {
				return RepresentationFilter as StockBalanceFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			Nomenclature nomenclatureAlias = null;
			MeasurementUnits unitAlias = null;
			StockBalanceVMNode resultAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And ((Filter == null || Filter.RestrictWarehouse == null) 
					? Restrictions.IsNotNull (Projections.Property<WarehouseMovementOperation> (o => o.IncomingWarehouse)) 
					: Restrictions.Eq (Projections.Property<WarehouseMovementOperation> (o => o.IncomingWarehouse), Filter.RestrictWarehouse))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			var subqueryRemove = QueryOver.Of<WarehouseMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And ((Filter == null || Filter.RestrictWarehouse == null) 
					? Restrictions.IsNotNull (Projections.Property<WarehouseMovementOperation> (o => o.WriteoffWarehouse)) 
					: Restrictions.Eq (Projections.Property<WarehouseMovementOperation> (o => o.WriteoffWarehouse), Filter.RestrictWarehouse))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			var queryStock = UoW.Session.QueryOver<Nomenclature>(() => nomenclatureAlias);
			if(!Filter.ShowArchive){
				queryStock = queryStock.Where(n => n.IsArchive == false);
			}

			var stocklist = queryStock
				.JoinQueryOver(n => n.Unit, () => unitAlias)
				.SelectList(list => list
				            .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
				            .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
				            .Select(() => nomenclatureAlias.IsArchive).WithAlias(() => resultAlias.NomenclatureIsArchive)
				            .Select(() => nomenclatureAlias.MinStockCount).WithAlias(() => resultAlias.NomenclatureMinCount)
				            .Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
				            .Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
				            .SelectSubQuery (subqueryAdd).WithAlias(() => resultAlias.Append)
				            .SelectSubQuery (subqueryRemove).WithAlias(() => resultAlias.Removed)
				)
				.TransformUsing(Transformers.AliasToBean<StockBalanceVMNode>())
				.List<StockBalanceVMNode>().Where(r => r.Amount != 0).ToList ();

			SetItemsSource (stocklist);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <StockBalanceVMNode>
			.Create()
			.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
			.AddColumn("Номенклатура").SetDataProperty (node => node.NomenclatureName)
			.AddColumn("Кол-во").SetDataProperty (node => node.CountText)
			.AddColumn("Мин кол-во\n на складе").SetDataProperty(node => node.MinCountText)
			.AddColumn("Разница").SetDataProperty(node => node.DiffCountText)
			.RowCells ().AddSetter<CellRendererText> ((c, n) => c.Foreground = n.RowColor)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			//FIXME Пока простая проверка.
			return true; //(updatedSubject is Nomenclature || updatedSubject is GoodsMovementOperation);
		}

		#endregion

		public StockBalanceVM (StockBalanceFilter filter) : this(filter.UoW)
		{
			Filter = filter;
		}

		public StockBalanceVM () 
			: this(UnitOfWorkFactory.CreateWithoutRoot ()) 
		{}

		public StockBalanceVM (IUnitOfWork uow) : base(typeof(Nomenclature), typeof(WarehouseMovementOperation))
		{
			this.UoW = uow;
		}
	}
		
	public class StockBalanceVMNode
	{
		[UseForSearch]
		public int Id{ get; set;}

		public decimal Append{ get; set;}

		public decimal Removed{ get; set;}

		public string UnitName{ get; set;}

		public short UnitDigits{ get; set;}

		public bool NomenclatureIsArchive { get; set; }

		string nomenclatureName { get; set; }
		[UseForSearch]
		public string NomenclatureName { 
			get{ return string.Format("{0}{1}", nomenclatureName, NomenclatureIsArchive ? " (АРХИВ)" : ""); } 
			set{ nomenclatureName = value; }
		}

		public string CountText { get { return String.Format ("{0:" + String.Format ("F{0}", UnitDigits) + "} {1}", 
			Amount,
			UnitName);
		}}

		public string MinCountText {
			get {
				return String.Format("{0:" + String.Format("F{0}", UnitDigits) + "} {1}",
				                     NomenclatureMinCount,
				                     UnitName);
			}
		}

		public string DiffCountText {
			get {
				return String.Format("{0:" + String.Format("F{0}", UnitDigits) + "} {1}",
				                     DiffCount,
				                     UnitName);
			}
		}


		public decimal Amount { get { return Append - Removed; }}

		public decimal NomenclatureMinCount { get; set; }

		public decimal DiffCount { get { return Amount - NomenclatureMinCount; } }

		public string RowColor {
			get {
				if (Amount < 0)
					return "red";
				else
					return "black";
			}
		}
	}
}

