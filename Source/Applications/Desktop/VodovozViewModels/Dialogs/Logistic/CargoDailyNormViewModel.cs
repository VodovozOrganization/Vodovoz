using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Dialogs.Logistic
{
	public class CargoDailyNormViewModel : UowDialogViewModelBase
	{
		private readonly IRouteListParametersProvider _routeListParametersProvider;

		public CargoDailyNormViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IRouteListParametersProvider routeListParametersProvider)
			: base(unitOfWorkFactory, navigation)
		{
			_routeListParametersProvider = routeListParametersProvider ??
			                               throw new ArgumentNullException(nameof(routeListParametersProvider));

			Initialize();
		}


		private void Initialize()
		{
			var carTypesOfUse = Enum.GetValues(typeof(CarTypeOfUse)).Cast<CarTypeOfUse>().ToList();

			foreach(var carType in carTypesOfUse)
			{
				var amount = _routeListParametersProvider.GetCargoDailyNorm(carType);

				CargoDailyNormNodes.Add(
					new CargoDailyNormNode
					{
						CarTypeOfUse = carType,
						Amount = amount
					});
			}
		}

		public override bool Save()
		{
			var cargoDailyNormNodesDictionary =
				CargoDailyNormNodes.ToDictionary(x => x.CarTypeOfUse, x => x.Amount);

			_routeListParametersProvider.SaveCargoDailyNorms(cargoDailyNormNodesDictionary);

			return true;
		}

		public List<CargoDailyNormNode> CargoDailyNormNodes { get; } = new List<CargoDailyNormNode>();

		public override string Title => "Настройка нормы вывоза в день";
	}
}
