using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции передвижения товаров по автомобилю(экземплярный учет)",
		Nominative = "операция передвижения товаров по автомобилю(экземплярный учет)")]
	public class CarInstanceGoodsAccountingOperation : InstanceGoodsAccountingOperation
	{
		private Car _car;
		
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		public override OperationType OperationType => OperationType.CarInstanceGoodsAccountingOperation;
	}
}

