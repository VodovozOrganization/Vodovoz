using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции передвижения товаров по автомобилю(объемный учет)",
		Nominative = "операция передвижения товаров по автомобилю(объемный учет)")]
	public class CarBulkGoodsAccountingOperation : BulkGoodsAccountingOperation
	{
		private Car _car;
		
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		public override OperationType OperationType => OperationType.CarBulkGoodsAccountingOperation;
	}
}

