using CustomerApp.Contracts.Sale.Templates;

namespace CustomerOrdersApi.Library.V8.Factories
{
	/// <summary>
	/// Фабрика по созданию информации еженедельных повторений автозаказа
	/// </summary>
	public interface IRepeatTemplateEveryWeeksFactory
	{
		/// <summary>
		/// Создание информации по еженедельным повторениям автозаказа
		/// </summary>
		/// <returns></returns>
		RepeatTemplateEveryWeeks Create();
	}
}
