namespace Mango.Domain.Enums
{
	public enum CallDirect
	{
		None = 0,
		
		/// <summary>
		/// Входящий
		/// </summary>
		Inbound = 1,
		
		/// <summary>
		/// Исходящий
		/// </summary>
		Outbound = 2,
		
		/// <summary>
		/// Внутренний
		/// </summary>
		Inner = 3
	}
}
