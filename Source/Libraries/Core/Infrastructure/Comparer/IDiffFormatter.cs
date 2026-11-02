namespace Comparer
{
	/// <summary>
	/// Форматтер отображения пары значений, например подсветка отличающихся частей разметкой.
	/// Применяется к значениям простых свойств; результат попадает в
	/// <see cref="PropertyDifference.FormattedFirstValue"/> и <see cref="PropertyDifference.FormattedSecondValue"/>.
	/// </summary>
	public interface IDiffFormatter
	{
		/// <summary>
		/// Форматирует пару значений для отображения рядом, например подсвечивая отличающиеся части.
		/// </summary>
		void SideBySideDiff(string oldValue, string newValue, out string oldDiff, out string newDiff);
	}
}
