using Gamma.ColumnConfig;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QSOrmProject.RepresentationModel;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure;

namespace Vodovoz.ViewModel
{
	/// <summary>
	/// Модель отображения в списке количества оборудования определенного типа.
	/// </summary>
	public class EquipmentKindsForRentVM :RepresentationModelWithoutEntityBase<EquipmentKindsForRentVMNode>
	{
		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes()
		{
			Nomenclature nomenclatureAlias = null;
			EquipmentKind equipmentKindAlias = null;
			NomenclatureForSaleVMNode resultAlias = null;
			GoodsAccountingOperation operationAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Equipment equipmentAlias = null;

			var subQueryBalance = QueryOver.Of(() => operationAlias)
				.JoinAlias(() => operationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Kind.Id == equipmentKindAlias.Id)
				.Select(Projections.Sum<GoodsAccountingOperation> (o => o.Amount));

			var subqueryReserved = QueryOver.Of(() => orderAlias)
				.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.JoinAlias(() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Kind.Id == equipmentKindAlias.Id)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Accepted
			           || orderAlias.OrderStatus == OrderStatus.InTravelList
			           || orderAlias.OrderStatus == OrderStatus.OnLoading)
				.Select (Projections.Count (() => orderEquipmentAlias.Id));

			var equipment = UoW.Session.QueryOver(()=>equipmentKindAlias)
				.SelectList(list => list
					.SelectGroup(()=> equipmentKindAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => equipmentKindAlias.Name).WithAlias(() => resultAlias.Name)
					.SelectSubQuery(subQueryBalance).WithAlias(() => resultAlias.InStock)
					.SelectSubQuery(subqueryReserved).WithAlias(() => resultAlias.Reserved)
				)
				.TransformUsing(Transformers.AliasToBean<EquipmentKindsForRentVMNode>())
				.List<EquipmentKindsForRentVMNode>();

			SetItemsSource(equipment);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig <EquipmentKindsForRentVMNode>.Create ()
			.AddColumn ("Вид оборудования").AddTextRenderer(node => node.Name)
			.AddColumn ("На складе").AddTextRenderer (node => node.InStockText)
			.AddColumn ("Зарезервировано").AddTextRenderer (node => node.ReservedText)
			.AddColumn ("Доступно").AddTextRenderer (node => node.AvailableText)
			.AddSetter ((cell, node) => cell.ForegroundGdk = node.Available > 0 ? GdkColors.PrimaryText : GdkColors.DangerText)
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		public EquipmentKindsForRentVM () 
			: this(ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot ()) 
		{}

		public EquipmentKindsForRentVM (IUnitOfWork uow) : base(typeof(Nomenclature), typeof(GoodsAccountingOperation))
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

	public class EquipmentKindsForRentVMNode
	{
		public int Id{get;set;}

		[UseForSearch]
		public string Name{ get; set; }
		public decimal InStock { get; set; }
		public int Reserved{ get; set; }
		public decimal Available => InStock - Reserved;

		public string UnitName { get; set; }
		public short UnitDigits { get; set; }

		public string InStockText => InStock.ToString("N0");
		public string ReservedText => Reserved.ToString();
		public string AvailableText => Available.ToString("N0");
	}

	public class NomenclatureForSaleVMNode
	{
		[UseForSearch]
		public int Id { get; set; }

		[UseForSearch]
		public string Name { get; set; }
		public NomenclatureCategory Category { get; set; }
		public decimal InStock => Added - Removed;
		public decimal? Reserved { get; set; }
		public decimal Available => InStock - Reserved.GetValueOrDefault();
		public decimal Added { get; set; }
		public decimal Removed { get; set; }
		public string UnitName { get; set; }
		public short UnitDigits { get; set; }
		public bool IsEquipmentWithSerial { get; set; }
		private string Format(decimal value) => string.Format("{0:F" + UnitDigits + "} {1}", value, UnitName);

		private bool UsedStock => Nomenclature.GetCategoriesForGoods().Contains(Category);

		public string InStockText => UsedStock ? Format(InStock) : string.Empty;
		public string ReservedText => UsedStock && Reserved.HasValue ? Format(Reserved.Value) : string.Empty;
		public string AvailableText => UsedStock ? Format(Available) : string.Empty;
	}
}

