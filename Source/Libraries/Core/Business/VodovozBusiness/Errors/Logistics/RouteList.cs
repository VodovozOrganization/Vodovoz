using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Errors.Logistics
{
	public static partial class RouteList
	{
		public static Error NotFound =>
			new Error(
				typeof(RouteList),
				nameof(NotFound),
				"Маршрутный лист не найден");

		public static Error CreateNotFound(int? id) =>
			id is null ? NotFound : new Error(
				typeof(RouteList),
				nameof(NotFound),
				$"Маршрутный лист #{id} не найден");

		[Display(Name = "Маршрутный лист не в пути")]
		public static Error NotEnRouteState =>
			new Error(
				typeof(RouteList),
				nameof(NotEnRouteState),
				$"Маршрутный лист не в статусе {RouteListStatus.EnRoute}");

		public static Error NotFoundAssociatedWithOrder =>
			new Error(
				typeof(RouteList),
				nameof(NotFoundAssociatedWithOrder),
				$"Не найден маршрутный лист содержащий заказ");

		public static Error ContainsCanceledOrdersOnAccept(int[] canceledOrdersIds) =>
			new Error(
				typeof(RouteList),
				nameof(ContainsCanceledOrdersOnAccept),
				$"В МЛ находятся отменённые заказы, необходимо удалить их: {string.Join(", ", canceledOrdersIds)}.");

		public static Error CarIsEmpty =>
			new Error(
				typeof(RouteList),
				nameof(CarIsEmpty),
				"Не заполнен автомобиль");

		public static Error IncorrectStatusForAccept =>
			new Error(
				typeof(RouteList),
				nameof(IncorrectStatusForAccept),
				"Неподходящий статус для подтверждения МЛ");

		public static Error IncorrectStatusForEdit =>
			new Error(
				typeof(RouteList),
				nameof(IncorrectStatusForEdit),
				"Неподходящий статус для редактирования МЛ");

		public static Error HasCarLoadingDocuments =>
			new Error(
				typeof(RouteList),
				nameof(HasCarLoadingDocuments),
				"К МЛ привязаны документы погрузки");

		public static Error ValidationFailure =>
			new Error(
				typeof(RouteList),
				nameof(ValidationFailure),
				"МЛ не прошёл валидацию");

		public static Error Overweighted(decimal overweight) =>
			new Error(
				typeof(RouteList),
				nameof(Overweighted),
				$"Вес груза превышен на {overweight}");

		public static Error Overvolumed(decimal overvolume) =>
			new Error(
				typeof(RouteList),
				nameof(Overvolumed),
				$"Объём груза превышен на {overvolume}");

		public static Error InsufficientFreeVolumeForReturn(decimal needFreeVolume) =>
			new Error(
				typeof(RouteList),
				nameof(InsufficientFreeVolumeForReturn),
				$"Объём возвращаемого груза превышен на {needFreeVolume}");

		public static string[] OverfilledErrorCodes => new[]
		{
			Error.GenerateCode(
				typeof(RouteList),
				nameof(Overweighted)),
			Error.GenerateCode(
				typeof(RouteList),
				nameof(Overvolumed)),
			Error.GenerateCode(
				typeof(RouteList),
				nameof(InsufficientFreeVolumeForReturn)),
		};
	}
}
