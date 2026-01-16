using Vodovoz.Core.Domain.Results;

namespace CustomerAppsApi.Library.Errors
{
	public static class LegalCounterpartyControllerError
	{
		/// <summary>
		/// Нет юр лиц с указанной активной электронной почтой
		/// </summary>
		/// <returns></returns>
		public static Error NotExistsActiveEmail()
			=> new Error(
				"404",
				"Нет юр лиц с указанной активной электронной почтой",
				typeof(LegalCounterpartyControllerError)
				);
		
		/// <summary>
		/// Обнаружено больше одной активной почты
		/// </summary>
		/// <param name="message">Сообщение</param>
		/// <returns></returns>
		public static Error ActiveEmailCountGreater1(string message)
			=> new Error(
				"500",
				message,
				typeof(LegalCounterpartyControllerError)
				);

		/// <summary>
		/// Неверный пароль аккаунта
		/// </summary>
		/// <returns></returns>
		public static Error WrongAccountPassword()
			=> new Error(
				"401",
				"Неверный пароль. Попробуйте еще раз.",
				typeof(LegalCounterpartyControllerError)
				);
	}
}
