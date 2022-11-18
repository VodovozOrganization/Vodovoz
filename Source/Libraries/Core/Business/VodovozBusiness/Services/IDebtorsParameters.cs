namespace Vodovoz.Services
{
	public interface IDebtorsParameters
	{
		int GetSuspendedCounterpartyId { get; }

		int GetCancellationCounterpartyId { get; }
	}
}
