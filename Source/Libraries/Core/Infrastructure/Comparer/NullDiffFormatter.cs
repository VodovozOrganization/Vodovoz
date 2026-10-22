namespace Comparer
{
	/// <summary>
	/// Базовая реализация <see cref="IDiffFormatter"/>, которая не выполняет никакого форматирования
	/// — возвращает исходные значения без изменений.
	/// Используется по умолчанию, пока не подключен конкретный форматтер (например, ThemmedDiffFormater).
	/// </summary>
	public class NullDiffFormatter : IDiffFormatter
	{
		/// <summary>
		/// Общий экземпляр форматтера.
		/// </summary>
		public static readonly NullDiffFormatter Instance = new NullDiffFormatter();

		/// <summary>
		/// Возвращает значения без изменений.
		/// </summary>
		public void SideBySideDiff(string oldValue, string newValue, out string oldDiff, out string newDiff)
		{
			oldDiff = oldValue;
			newDiff = newValue;
		}
	}
}
