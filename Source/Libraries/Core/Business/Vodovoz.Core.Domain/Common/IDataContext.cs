namespace Vodovoz.Core.Domain.Common
{
	/// <summary>
	/// Общий интерфейс для передачи в методы, содержащий аргументы
	/// </summary>
	public interface IDataContext
	{
		/// <summary>
		/// Данные для передачи в роли аргументов
		/// </summary>
		object Data { get; }
	}
	
	/// <summary>
	/// Интерфейс для передачи в различные методы
	/// </summary>
	/// <typeparam name="T">Данные для передачи в роли аргументов</typeparam>
	public interface IDataContext<out T>
	{
		/// <summary>
		/// Данные для передачи в роли аргументов
		/// </summary>
		T Data { get; }
	}
}
