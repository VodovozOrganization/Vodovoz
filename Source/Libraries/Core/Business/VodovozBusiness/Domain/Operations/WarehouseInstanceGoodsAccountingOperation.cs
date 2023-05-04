using QS.DomainModel.Entity;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class WarehouseInstanceGoodsAccountingOperation : InstanceGoodsAccountingOperation
	{
		private Warehouse _warehouse;
		
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}

		public override OperationType OperationType => OperationType.WarehouseInstanceGoodsAccountingOperation;
	}
}

