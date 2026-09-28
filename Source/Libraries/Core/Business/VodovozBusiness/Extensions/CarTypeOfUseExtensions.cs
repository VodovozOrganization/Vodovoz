using System;
using Vodovoz.Domain.Logistic.Cars;

namespace VodovozBusiness.Extensions
{
	public static class CarTypeOfUseExtensions
	{
		/// <summary>
		/// Типы моделей авто для исключения
		/// </summary>
		/// <returns>Коллекция энамов моделей авто для исключения</returns>
		public static readonly Enum[] CarTypeOfUseForExcludeAsEnum = new Enum[]
		{
			CarTypeOfUse.Loader,
			CarTypeOfUse.Semitrailer
		};

		/// <summary>
		/// Типы моделей авто для исключения
		/// </summary>
		/// <returns>Коллекция типов моделей авто для исключения</returns>
		public static readonly CarTypeOfUse[] CarTypeOfUseForExclude = new[]
		{
			CarTypeOfUse.Loader,
			CarTypeOfUse.Semitrailer
		};
	}
}
