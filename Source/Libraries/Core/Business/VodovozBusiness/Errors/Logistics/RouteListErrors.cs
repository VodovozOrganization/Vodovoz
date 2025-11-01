using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Errors.Logistics
{
	public static partial class RouteListErrors
	{
		public static Error NotFound =>
			new Error(
				typeof(RouteListErrors),
				nameof(NotFound),
				"Маршрутный лист не найден");

		public static Error CreateNotFound(int? id) =>
			id is null ? NotFound : new Error(
				typeof(RouteListErrors),
				nameof(NotFound),
				$"Маршрутный лист #{id} не найден");

		[Display(Name = "Маршрутный лист не в пути")]
		public static Error NotEnRouteState =>
			new Error(
				typeof(RouteListErrors),
				nameof(NotEnRouteState),
				$"Маршрутный лист не в статусе {RouteListStatus.EnRoute}");

		[Display(Name = "Неподходящий статус для закрытия МЛ")]
		public static Error IncorrectStatusForClose(string message = null) =>
			new Error(
				typeof(RouteListErrors),
				nameof(IncorrectStatusForClose),
				$"Неподходящий статус для закрытия МЛ. {message}");

		public static Error NotFoundAssociatedWithOrder =>
			new Error(
				typeof(RouteListErrors),
				nameof(NotFoundAssociatedWithOrder),
				$"Не найден маршрутный лист содержащий заказ");

		public static Error ContainsCanceledOrdersOnAccept(int[] canceledOrdersIds) =>
			new Error(
				typeof(RouteListErrors),
				nameof(ContainsCanceledOrdersOnAccept),
				$"В МЛ находятся отменённые заказы, необходимо удалить их: {string.Join(", ", canceledOrdersIds)}.");

		public static Error CarIsEmpty =>
			new Error(
				typeof(RouteListErrors),
				nameof(CarIsEmpty),
				"Не заполнен автомобиль");

		[Display(Name = "Должен быть заполнен кассир")]
		public static Error CashierIsEmpty =>
			new Error(
				typeof(RouteListErrors),
				nameof(CashierIsEmpty),
				"Должен быть заполнен кассир");

		public static Error IncorrectStatusForAccept =>
			new Error(
				typeof(RouteListErrors),
				nameof(IncorrectStatusForAccept),
				"Неподходящий статус для подтверждения МЛ");

		public static Error IncorrectStatusForEdit =>
			new Error(
				typeof(RouteListErrors),
				nameof(IncorrectStatusForEdit),
				"Неподходящий статус для редактирования МЛ");


		public static Error HasCarLoadingDocuments =>
			new Error(
				typeof(RouteListErrors),
				nameof(HasCarLoadingDocuments),
				"К МЛ привязаны документы погрузки");

		public static Error ValidationFailure =>
			new Error(
				typeof(RouteListErrors),
				nameof(ValidationFailure),
				"МЛ не прошёл валидацию");

		public static Error Overweighted(decimal overweight) =>
			new Error(
				typeof(RouteListErrors),
				nameof(Overweighted),
				$"Вес груза превышен на {overweight}");

		public static Error Overvolumed(decimal overvolume) =>
			new Error(
				typeof(RouteListErrors),
				nameof(Overvolumed),
				$"Объём груза превышен на {overvolume}");

		public static Error InsufficientFreeVolumeForReturn(decimal needFreeVolume) =>
			new Error(
				typeof(RouteListErrors),
				nameof(InsufficientFreeVolumeForReturn),
				$"Объём возвращаемого груза превышен на {needFreeVolume}");

		public static string[] OverfilledErrorCodes => new[]
		{
			Error.GenerateCode(
				typeof(RouteListErrors),
				nameof(Overweighted)),
			Error.GenerateCode(
				typeof(RouteListErrors),
				nameof(Overvolumed)),
			Error.GenerateCode(
				typeof(RouteListErrors),
				nameof(InsufficientFreeVolumeForReturn)),
		};
	}
}
