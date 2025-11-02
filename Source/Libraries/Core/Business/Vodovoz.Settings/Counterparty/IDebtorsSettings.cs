namespace Vodovoz.Settings.Counterparty
{
	public interface IDebtorsSettings
	{
		int GetSuspendedCounterpartyId { get; }

		int GetCancellationCounterpartyId { get; }
	}
}
