namespace Vodovoz.Settings.Nomenclature
{
	public interface IRecomendationSettings
	{
		int RobotCount { get; }
		int OperatorCount { get; }
		int IpzCount { get; }

		void SetRobotCount(int count);
		void SetOperatorCount(int count);
		void SetIpzCount(int count);
	}
}
