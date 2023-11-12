namespace Vodovoz.Core.Domain.Pacs
{
	public enum OperatorStateType
	{
		New,
		Connected,
		WaitingForCall,
		Talk,
		Break,
		Disconnected
	}

	public enum AdministratorStateType
	{
		New,
		Connected,
		Disconnected
	}
}
