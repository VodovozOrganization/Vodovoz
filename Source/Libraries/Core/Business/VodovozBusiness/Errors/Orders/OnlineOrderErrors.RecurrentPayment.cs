using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	public static partial class OnlineOrderErrors
	{
		public class RecurrentPayment
		{
			public static Error IsEmptyRecurrentPayment =>
				new Error(
					typeof(OnlineOrderErrors),
					nameof(IsEmptyRecurrentPayment),
					"Рекуррентный платеж не заполнен");
			
			public static Error IsEmptyToken =>
				new Error(
					typeof(OnlineOrderErrors),
					nameof(IsEmptyToken),
					"Токен рекуррентного платежа не заполнен");
			
			public static Error IsEmptyAccountId =>
				new Error(
					typeof(OnlineOrderErrors),
					nameof(IsEmptyAccountId),
					"Id аккаунта рекуррентного платежа не заполнен");
		}
	}
}
