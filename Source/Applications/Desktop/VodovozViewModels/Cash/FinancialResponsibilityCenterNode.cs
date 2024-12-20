namespace Vodovoz.ViewModels.Cash
{
	/// <summary>
	/// Нода журнала центров финансоой ответственности
	/// </summary>
	public class FinancialResponsibilityCenterNode
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Архивный
		/// </summary>
		public bool IsArchive { get; set; }
	}
}
