namespace Mango.Domain.Enums
{
	public enum CallDirect
	{
		None,
		
		/// <summary>
		/// Входящий
		/// </summary>
		Inbound,
		
		/// <summary>
		/// Исходящий
		/// </summary>
		Outbound,
		
		/// <summary>
		/// Внутренний
		/// </summary>
		Inner
	}
}
