using System;
using System.Linq;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using VodovozBusiness.Domain.Logistic;
using VodovozBusiness.Errors.Fuel;

namespace Vodovoz.Core.Application.Logistics.Fuel
{
	/// <summary>
	/// Сервис управления дополнительными видами топлива для автомобиля
	/// </summary>
	public class AdditionalFuelTypeManagementService
	{
		private Car _car;

		private bool IsInitialized => _car != null;

		/// <summary>
		/// Инициализирует сервис с указанным автомобилем
		/// </summary>
		/// <param name="car">Автомобиль</param>
		public void Initialize(Car car)
		{
			if(car is null)
			{
				throw new ArgumentNullException(nameof(car));
			}

			if(_car != null && _car.Id != car.Id)
			{
				throw new InvalidOperationException("Невозможно инициализировать сервис с другим автомобилем.");
			}

			_car = car;
		}

		/// <summary>
		/// Добавляет новый дополнительный вид топлива
		/// </summary>
		/// <param name="fuelType">Вид топлива</param>
		/// <returns>Результат операции</returns>
		public Result AddFuelType(FuelType fuelType)
		{
			if(fuelType is null)
			{
				throw new ArgumentNullException(nameof(fuelType));
			}

			ValidateInitialization();

			if(IsMainFuelType(fuelType))
			{
				return Result.Failure(AdditionalFuelTypeErrors.AlreadyMainFuelType);
			}

			if(IsFuelTypeAlreadyAdded(fuelType))
			{
				return Result.Failure(AdditionalFuelTypeErrors.AdditionalFuelTypeAlreadyAdded);
			}

			AddAdditionalFuelType(fuelType);

			return Result.Success();
		}

		/// <summary>
		/// Удаляет дополнительный вид топлива
		/// </summary>
		/// <param name="additionalFuelType">Дополнительный вид топлива</param>
		/// <returns>Результат операции</returns>
		public Result RemoveFuelType(AdditionalFuelType additionalFuelType)
		{
			if(additionalFuelType is null)
			{
				throw new ArgumentNullException(nameof(additionalFuelType));
			}

			ValidateInitialization();

			if(!IsAdditionalFuelTypeAlreadyAdded(additionalFuelType))
			{
				return Result.Failure(AdditionalFuelTypeErrors.AdditionalFuelTypeNotFound);
			}

			_car.AdditionalFuelTypes.Remove(additionalFuelType);

			return Result.Success();
		}

		/// <summary>
		/// Проверяет возможность добавления нового типа топлива
		/// </summary>
		/// <param name="fuelType">Тип топлива</param>
		/// <returns>Результат операции</returns>
		public bool CanAddFuelType(FuelType fuelType)
		{
			if(fuelType is null)
			{
				throw new ArgumentNullException(nameof(fuelType));
			}

			ValidateInitialization();

			if(_car.FuelType.Id == fuelType.Id)
			{
				return false;
			}
			
			return !IsFuelTypeAlreadyAdded(fuelType);
		}

		/// <summary>
		/// Проверяет возможность удаления вида топлива
		/// </summary>
		/// <param name="additionalFuelType">Дополнительный вид топлива</param>
		/// <returns>Результат операции</returns>
		public bool CanRemoveFuelType(AdditionalFuelType additionalFuelType)
		{
			if(additionalFuelType is null)
			{
				throw new ArgumentNullException(nameof(additionalFuelType));
			}

			ValidateInitialization();
			
			return IsAdditionalFuelTypeAlreadyAdded(additionalFuelType);
		}

		private bool IsMainFuelType(FuelType fuelType) =>
			_car.FuelType?.Id == fuelType.Id;

		private void AddAdditionalFuelType(FuelType fuelType)
		{
			var additionalFuelType = new AdditionalFuelType
			{
				Car = _car,
				FuelType = fuelType
			};

			_car.AdditionalFuelTypes.Add(additionalFuelType);
		}

		private AdditionalFuelType GetAdditionalFuelType(FuelType fuelType) =>
			_car.AdditionalFuelTypes
			.FirstOrDefault(aft => aft.FuelType.Id == fuelType.Id);

		private bool IsFuelTypeAlreadyAdded(FuelType fuelType) =>
			GetAdditionalFuelType(fuelType) != null;

		private bool IsAdditionalFuelTypeAlreadyAdded(AdditionalFuelType additionalFuelType) =>
			GetAdditionalFuelType(additionalFuelType.FuelType) != null;

		private void ValidateInitialization()
		{
			if(!IsInitialized)
			{
				throw new InvalidOperationException(
					"Сервис не инициализирован. Вызовите метод Initialize с автомобилем перед выполнением операции");
			}
		}
	}
}
