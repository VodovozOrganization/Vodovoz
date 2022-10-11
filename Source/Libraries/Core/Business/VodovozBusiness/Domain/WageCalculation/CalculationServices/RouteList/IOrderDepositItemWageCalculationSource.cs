namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public interface IOrderDepositItemWageCalculationSource
	{
		int InitialCount { get; }
		int? ActualCount { get; }
		decimal Deposit { get; }
	}
}