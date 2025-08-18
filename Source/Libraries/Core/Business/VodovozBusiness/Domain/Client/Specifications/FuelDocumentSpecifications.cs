using System;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Logistic;

namespace VodovozBusiness.Domain.Client.Specifications
{
	public static class FuelDocumentSpecifications
	{
		/// <summary>
		/// Создает спецификацию для фильтрации документов выдачи топлива по дате
		/// </summary>
		/// <param name="date">Дата документа</param>
		/// <returns></returns>
		public static ExpressionSpecification<FuelDocument> CreateForDate(DateTime date)
			=> new ExpressionSpecification<FuelDocument>(x => x.Date >= date.Date && x.Date < date.Date.AddDays(1));

		/// <summary>
		/// Создает спецификацию для фильтрации документов выдачи топлива по автомобилю, на который выдано топливо
		/// </summary>
		/// <param name="carId">Идентификатор автомобиля</param>
		/// <returns></returns>
		public static ExpressionSpecification<FuelDocument> CreateForCarId(int carId)
			=> new ExpressionSpecification<FuelDocument>(x => x.Car.Id == carId);

		/// <summary>
		/// Создает спецификацию для фильтрации документов выдачи топлива на текущую дату по автомобилю
		/// </summary>
		/// <param name="carId">Идентификатор автомобиля</param>
		/// <returns></returns>
		public static ExpressionSpecification<FuelDocument> CreateForTodayGivedFuelByCarId(int carId)
			=> CreateForDate(DateTime.Today)
				& CreateForCarId(carId);
	}
}
