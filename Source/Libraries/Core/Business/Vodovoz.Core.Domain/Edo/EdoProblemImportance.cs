namespace Vodovoz.Core.Domain.Edo
{
	public enum EdoProblemImportance
	{
		/// <summary>
		/// Ожидание совершения определенного действия, 
		/// после которого проблема решится сама собой
		/// </summary>
		Waiting,

		/// <summary>
		/// Проблема требующая вмешательства пользователя
		/// </summary>
		Problem,
	}
}
