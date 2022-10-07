namespace Vodovoz.Domain.WageCalculation.CalculationServices
{
	public interface IWageCalculationService<TResult>
		where TResult : new()
	{
		TResult CalculateWage();
	}

	public interface IWageCalculationService : IWageCalculationService<decimal>
	{
	}
}
