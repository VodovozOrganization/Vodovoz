using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Extensions;

namespace Vodovoz.Errors.Edo
{
	public static partial class EdoErrors
	{
		public static Error AlreadyPaidUpd =>
			new Error(typeof(EdoErrors),
				nameof(AlreadyPaidUpd),
				"Маршрутный лист не найден");

		public static Error CreateAlreadyPaidUpd(int orderId, DocumentContainerType type) =>
			 new Error(
				typeof(EdoErrors),
				nameof(AlreadyPaidUpd),
				$"Счет по заказу №{orderId} оплачен.\r\nПроверьте, пожалуйста, статус {type.GetEnumDisplayName()} в ЭДО перед повторной отправкой на предмет аннулирован/не аннулирован, подписан/не подписан.");

		public static Error AlreadySuccefullSended =>
			new Error(typeof(EdoErrors),
				nameof(AlreadySuccefullSended),
				"Документы уже успешно отправлены");

		public static Error CreateAlreadySuccefullSended(EdoContainer edoContainer) =>
			 new Error(
				typeof(EdoErrors),
				nameof(AlreadySuccefullSended),
				$"Для заказа №" +
				 $"{edoContainer.Order?.Id ?? edoContainer.OrderWithoutShipmentForDebt?.Id ?? edoContainer.OrderWithoutShipmentForPayment?.Id ?? edoContainer.OrderWithoutShipmentForAdvancePayment?.Id} " +
				 $"имеется документ со статусом \"{edoContainer.EdoDocFlowStatus.GetEnumDisplayName()}\"");
	}
}
