using QS.DomainModel.UoW;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.ViewModels.Factories
{
	/// <summary>
	/// Фабрика для создания экземпляров <see cref="AdditionalFuelTypeManagementViewModel"/>
	/// </summary>
	public interface IAdditionalFuelTypeManagementViewModelFactory
	{
		/// <summary>
		/// Создает экземпляр <see cref="AdditionalFuelTypeManagementViewModel"/>
		/// </summary>
		/// <param name="car">Автомобиль</param>
		/// <param name="uow">Unit of Work</param>
		/// <param name="parentDialog">Родительский диалог</param>
		/// <returns></returns>
		AdditionalFuelTypeManagementViewModel CreateAdditionalFuelTypeManagementViewModel(Car car, IUnitOfWork uow, DialogViewModelBase parentDialog);
	}
}
