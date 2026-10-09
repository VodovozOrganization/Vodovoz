namespace Vodovoz.Settings.Sale
{
	public interface IRepeatTemplateSettings
	{
		/// <summary>
		/// Минимальное значение периодичности повторений по неделям
		/// </summary>
		int RepeatEveryWeeksMin { get; }
		/// <summary>
		/// Максимальное значение периодичности повторений по неделям
		/// </summary>
		int RepeatEveryWeeksMax { get; }
		/// <summary>
		/// Минимальное значение числа месяца для доставок
		/// </summary>
		int MonthDayMin { get; }
		/// <summary>
		/// Максимальное значение числа месяца для доставок
		/// </summary>
		int MonthDayMax { get; }
		int[] RepeatEveryWeeksDefault();
	}
}
