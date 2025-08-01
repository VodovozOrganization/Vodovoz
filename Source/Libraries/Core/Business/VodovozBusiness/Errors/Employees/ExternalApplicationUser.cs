using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Errors.Employees
{
	/// <summary>
	/// Ошибки, связанные с пользователями внешних приложений
	/// </summary>
	public static class ExternalApplicationUser
	{
		/// <summary>
		/// Пользователь не найден
		/// </summary>
		public static Error NotFound =>
			new Error(
				typeof(ExternalApplicationUser),
				nameof(NotFound),
				"Пользователь не найден");
	}
}
