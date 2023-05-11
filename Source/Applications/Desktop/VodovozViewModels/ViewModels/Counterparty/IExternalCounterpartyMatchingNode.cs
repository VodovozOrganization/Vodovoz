namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public interface IExternalCounterpartyMatchingNode
	{
		int EntityId { get; }
		string PersonTypeShort { get; }
		string Title { get; }
		bool Matching { get; }
		string LastOrderDateString { get; }
		int? ExternalCounterpartyId { get; }
		bool HasOtherExternalCounterparty { get; }
	}
}
