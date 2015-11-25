using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.Domain.Operations;
using Gamma.ColumnConfig;

namespace Vodovoz.ViewModel
{
	public class StockBalanceVM : RepresentationModelWithoutEntityBase<StockBalanceVMNode>
	{
		public StockBalanceFilter Filter {
			get {
				return RepresentationFilter as StockBalanceFilter;
			}
			set { RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region IRepresentationModel implementation

		public override void UpdateNodes ()
		{
			Nomenclature nomenclatureAlias = null;
			MeasurementUnits unitAlias = null;
			StockBalanceVMNode resultAlias = null;
			GoodsMovementOperation operationAddAlias = null;
			GoodsMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<GoodsMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And ((Filter == null || Filter.RestrictWarehouse == null) 
					? Restrictions.IsNotNull (Projections.Property<GoodsMovementOperation> (o => o.IncomingWarehouse)) 
					: Restrictions.Eq (Projections.Property<GoodsMovementOperation> (o => o.IncomingWarehouse), Filter.RestrictWarehouse))
				.Select (Projections.Sum<GoodsMovementOperation> (o => o.Amount));

			var subqueryRemove = QueryOver.Of<GoodsMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And ((Filter == null || Filter.RestrictWarehouse == null) 
					? Restrictions.IsNotNull (Projections.Property<GoodsMovementOperation> (o => o.WriteoffWarehouse)) 
					: Restrictions.Eq (Projections.Property<GoodsMovementOperation> (o => o.WriteoffWarehouse), Filter.RestrictWarehouse))
				.Select (Projections.Sum<GoodsMovementOperation> (o => o.Amount));

			var stocklist = UoW.Session.QueryOver<Nomenclature> (() => nomenclatureAlias)
				.JoinQueryOver(n => n.Unit, () => unitAlias)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
					.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
					.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
					.SelectSubQuery (subqueryAdd).WithAlias(() => resultAlias.Append)
					.SelectSubQuery (subqueryRemove).WithAlias(() => resultAlias.Removed)
				)
				.TransformUsing(Transformers.AliasToBean<StockBalanceVMNode>())
				.List<StockBalanceVMNode>().Where(r => r.Amount != 0).ToList ();

			SetItemsSource (stocklist);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <StockBalanceVMNode>.Create ()
			.AddColumn("Номенклатура").SetDataProperty (node => node.NomenclatureName)
			.AddColumn ("Кол-во").SetDataProperty (node => node.CountText)
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

		public StockBalanceVM (IUnitOfWork uow) : base(typeof(Nomenclature), typeof(GoodsMovementOperation))
		{
			this.UoW = uow;
		}
	}
		
	public class StockBalanceVMNode
	{

		public int Id{ get; set;}

		public decimal Append{ get; set;}

		public decimal Removed{ get; set;}

		public string UnitName{ get; set;}

		public short UnitDigits{ get; set;}

		[UseForSearch]
		public string NomenclatureName { get; set;}

		public string CountText { get { return String.Format ("{0:" + String.Format ("F{0}", UnitDigits) + "} {1}", 
			Amount,
			UnitName);
		}}

		public decimal Amount { get { return Append - Removed; }}

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

