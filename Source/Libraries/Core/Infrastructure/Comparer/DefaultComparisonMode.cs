namespace Comparer
{
	/// <summary>
	/// Режим выбора свойств для сравнения по умолчанию.
	/// </summary>
	public enum DefaultComparisonMode
	{
		/// <summary>
		/// Сравниваются все простые свойства, кроме исключённых через Exclude.
		/// </summary>
		Include,
		/// <summary>
		/// Сравниваются только свойства, включённые через Include или настроенные через CompareObject.
		/// </summary>
		Exclude
	}
}
