namespace Vodovoz.Presentation.WebApi.Caching.Idempotency
{
	/// <summary>
	/// Результат попытки поставить метку «в обработке»
	/// </summary>
	public enum IdempotencyBeginResult
	{
		/// <summary>
		/// Метка поставлена - текущий запрос выполняет метод
		/// </summary>
		Started = 1,

		/// <summary>
		/// Запись с таким ключом уже есть: запрос выполняется другим исполнителем или уже выполнен
		/// </summary>
		AlreadyExists = 2,

		/// <summary>
		/// Хранилище недоступно
		/// </summary>
		Unavailable = 3
	}
}
