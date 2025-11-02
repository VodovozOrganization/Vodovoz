namespace Vodovoz.EntityRepositories.Nodes
{
	public class FastDeliveryVerificationParameter<T>
		where T : struct
	{
		public T ParameterValue { get; set; }
		public bool IsValidParameter { get; set; }
	}
}
