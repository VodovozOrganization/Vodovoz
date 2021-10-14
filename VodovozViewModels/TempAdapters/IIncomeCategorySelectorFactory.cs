using QS.Project.Journal.EntitySelector;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IIncomeCategorySelectorFactory
	{
		/// <summary>
		/// Фабрика для простого журнала - без уровней
		/// </summary>
		/// <returns></returns>
		IEntityAutocompleteSelectorFactory CreateSimpleIncomeCategoryAutocompleteSelectorFactory();
		/// <summary>
		/// Фабрика для журнала с уровнями
		/// </summary>
		/// <returns></returns>
		IEntityAutocompleteSelectorFactory CreateDefaultIncomeCategoryAutocompleteSelectorFactory();
	}
}
