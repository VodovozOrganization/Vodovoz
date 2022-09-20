namespace Vodovoz.Tools
{
	public interface IDataArchiver
	{
		void ArchiveMonitoring();
		void ArchiveTrackPoints();
		void DeleteDistanceCache();
	}
}
