namespace Vodovoz.Domain.Roboats
{
	public enum RoboatsCallResult
	{
		Nothing,
		OrderCreated,
		OrderAccepted
	}

	public class RoboatsCallResultStringType : NHibernate.Type.EnumStringType
	{
		public RoboatsCallResultStringType() : base(typeof(RoboatsCallResult))
		{
		}
	}
}
