using TrueMark.Contracts;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Application.Errors
{
	public static class TrueMarkApi
	{
		public static Error UnknownCode => new Error(
			typeof(TrueMarkApi),
			nameof(UnknownCode),
			"Ошибка при запросе к Api TrueMark, нет информации о коде");

		public static Error CodeNotInCorrectStatus => new Error(
			typeof(TrueMarkApi),
			nameof(CodeNotInCorrectStatus),
			"Ошибка при запросе к Api TrueMark, код должен быть в статусе " +
			$"{ProductInstanceStatusEnum.Introduced}, {ProductInstanceStatusEnum.Applied} " +
			$"или {ProductInstanceStatusEnum.AppliedPaid}");

		public static Error CallFailed => new Error(
			typeof(TrueMarkApi),
			nameof(CallFailed),
			"Ошибка при запросе к Api TrueMark");

		public static Error ErrorResponse => new Error(
			typeof(TrueMarkApi),
			nameof(ErrorResponse),
			"Ошибка при запросе к Api TrueMark, ошибка в ответе от Api");
	}
}
