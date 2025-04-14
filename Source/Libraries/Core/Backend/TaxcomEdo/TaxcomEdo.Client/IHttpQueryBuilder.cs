namespace TaxcomEdo.Client
{
	/// <summary>
	/// Формирование строки запроса с параметрами
	/// </summary>
	public interface IHttpQueryBuilder
	{
		/// <summary>
		/// Добавление параметра.
		/// Работает только с простыми типами и строками,
		/// чтобы использовать сложные объекты, такие как классы,
		/// нужно передавать их строковое представление, например, JSON
		/// </summary>
		/// <param name="parameter">Значение параметра</param>
		/// <param name="parameterName">Имя параметра</param>
		/// <typeparam name="T">Тип параметра</typeparam>
		/// <returns></returns>
		IHttpQueryBuilder AddParameter<T>(T parameter, string parameterName);
		/// <summary>
		/// Выдача строки запроса
		/// </summary>
		/// <returns>Строка запроса с параметрами</returns>
		string ToString();
	}
}
