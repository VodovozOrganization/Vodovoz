namespace Vodovoz.Domain.Roboats
{
	public enum RoboatsCallStatus
	{
		InProgress,
		Aborted,
		Fail,
		Success
	}

	public class RoboatsCallStatusStringType : NHibernate.Type.EnumStringType
	{
		public RoboatsCallStatusStringType() : base(typeof(RoboatsCallStatus))
		{
		}
	}
}
