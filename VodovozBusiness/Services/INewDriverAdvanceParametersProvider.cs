namespace Vodovoz.Parameters
{
	public interface INewDriverAdvanceParametersProvider
	{
		int NewDriverAdvanceFirstDay { get; }
		int NewDriverAdvanceLastDay { get; }
		decimal NewDriverAdvanceSum { get; }
		bool IsNewDriverAdvanceEnabled { get; }
	}
}
