namespace Vodovoz.Errors.Common
{
	public static class Repository
	{
		/// <summary>
		/// Ошибка получения данных
		/// </summary>
		public static Error DataRetrievalError => new Error(
			typeof(Repository),
			nameof(DataRetrievalError),
			"Ошибка получения данных");
	}
}
