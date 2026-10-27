namespace Vodovoz.Core.Domain.Results
{
	/// <summary>
	/// Класс для контроля упешной/не успешной операции с возможностью добавления конкретизирующей инфы
	/// </summary>
	public class OkResult
	{
		private OkResult(bool ok, string description = null)
		{
			Ok = ok;
			Description = description;
		}
		
		/// <summary>
		/// Успех/не успех
		/// </summary>
		public bool Ok { get; }
		/// <summary>
		/// Описание проблемы
		/// </summary>
		public string Description { get; }
		/// <summary>
		/// Неуспешный результат с описанием проблемы
		/// </summary>
		public bool IsFailureWithDescription => !Ok && !string.IsNullOrWhiteSpace(Description);

		public static OkResult Success() => new OkResult(true);
		public static OkResult Failure(string description = null) => new OkResult(false, description);
	}
}
