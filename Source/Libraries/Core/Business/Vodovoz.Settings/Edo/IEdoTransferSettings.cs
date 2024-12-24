namespace Vodovoz.Settings.Edo
{
	public interface IEdoTransferSettings
	{
		int TransferTaskTimeoutMinute { get; }
		int TransferTaskTimeoutCheckIntervalSecond { get; }
		int MinCodesCountForStartTransfer { get; }
	}
}
