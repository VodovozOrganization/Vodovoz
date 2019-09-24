namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public interface IOrderItemWageCalculationSource
	{
		int InitialCount { get; }
		int? ActualCount { get; }
		decimal Price { get; }
		decimal DiscountMoney { get; }
		decimal PercentForMaster { get; }
		bool IsMasterNomenclature { get; }
	}
}