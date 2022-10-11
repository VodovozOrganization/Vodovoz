namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public interface IOrderItemWageCalculationSource
	{
		decimal InitialCount { get; }
		decimal? ActualCount { get; }
		decimal Price { get; }
		decimal DiscountMoney { get; }
		decimal PercentForMaster { get; }
		bool IsMasterNomenclature { get; }
	}
}