using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.V5
{
	/// <summary>
	/// Контроллер для эндпойнтов с контрольной суммой
	/// </summary>
	public class SignatureControllerBase : VersionedController
	{
		private const string _invalidSignature = "Неккоректная контрольная сумма";
		
		public SignatureControllerBase(ILogger<SignatureControllerBase> logger) : base(logger) { }

		/// <summary>
		/// Неверная контрольная сумма
		/// </summary>
		/// <param name="requestSignature">Контрольная сумма запроса</param>
		/// <param name="generatedSignature">Расчитанная контрольная сумма</param>
		/// <returns></returns>
		protected IActionResult InvalidSignature(string requestSignature, string generatedSignature)
		{
			_logger.LogWarning(
				"{InvalidSignature}. Пришла {ReceivedSign}, должна быть {GeneratedSign}",
				_invalidSignature,
				requestSignature,
				generatedSignature
			);

			return ValidationProblem(_invalidSignature);
		}
	}
}
