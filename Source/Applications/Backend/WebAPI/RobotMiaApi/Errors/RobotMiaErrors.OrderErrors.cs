using Vodovoz.Core.Domain.Results;

namespace RobotMiaApi.Errors
{
public static partial class RobotMiaErrors
	{
		/// <summary>
		/// Ошибки заказов
		/// </summary>
		public static class OrderErrors
		{
			/// <summary>
			/// Ошибка добавления номенклатуры
			/// </summary>
			/// <param name="message"></param>
			/// <returns></returns>
			public static Error AddNomenclatureError(string message)
				=> new Error(typeof(OrderErrors),
					nameof(AddNomenclatureError),
					message);
		}
	}
}
