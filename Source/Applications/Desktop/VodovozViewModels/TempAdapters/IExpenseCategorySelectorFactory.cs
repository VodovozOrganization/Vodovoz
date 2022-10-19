using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IExpenseCategorySelectorFactory
	{
		/// <summary>
		/// Фабрика для простого журнала - без уровней
		/// </summary>
		/// <returns></returns>
		IEntityAutocompleteSelectorFactory CreateSimpleExpenseCategoryAutocompleteSelectorFactory();
		/// <summary>
		/// Фабрика для журнала с уровнями
		/// </summary>
		/// <returns></returns>
		IEntityAutocompleteSelectorFactory CreateDefaultExpenseCategoryAutocompleteSelectorFactory();
	}
}
