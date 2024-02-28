namespace Vodovoz.Settings.Logistics
{
	public interface INewDriverAdvanceSettings
	{
		int NewDriverAdvanceFirstDay { get; }
		int NewDriverAdvanceLastDay { get; }
		decimal NewDriverAdvanceSum { get; }
		bool IsNewDriverAdvanceEnabled { get; }
	}
}
