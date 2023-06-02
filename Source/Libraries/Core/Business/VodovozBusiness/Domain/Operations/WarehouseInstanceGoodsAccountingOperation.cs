﻿using QS.DomainModel.Entity;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции передвижения товаров по складу(экземплярный учет)",
		Nominative = "операция передвижения товаров по складу(экземплярный учет)")]
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

