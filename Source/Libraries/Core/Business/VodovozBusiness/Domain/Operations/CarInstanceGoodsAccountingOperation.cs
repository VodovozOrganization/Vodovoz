using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class CarInstanceGoodsAccountingOperation : InstanceGoodsAccountingOperation
	{
		private Car _car;
		
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		public override OperationTypeByStore OperationTypeByStore => OperationTypeByStore.Car;
	}
}

