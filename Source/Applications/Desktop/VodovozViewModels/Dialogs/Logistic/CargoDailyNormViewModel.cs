using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.ViewModels.Dialogs.Logistic
{
	public class CargoDailyNormViewModel : UowDialogViewModelBase
	{
		private readonly IRouteListRepository _routeListRepository;
		private readonly CarTypeOfUse[] _excludeCarTypeOfUses;

		public CargoDailyNormViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IRouteListRepository routeListRepository,
			CarTypeOfUse[] excludeCarTypeOfUses)
			: base(unitOfWorkFactory, navigation)
		{
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_excludeCarTypeOfUses = excludeCarTypeOfUses;

			Initialize();
		}


		private void Initialize()
		{
			var carTypeOfUses = Enum.GetValues(typeof(CarTypeOfUse)).Cast<CarTypeOfUse>()
				.Where(x => !_excludeCarTypeOfUses.Contains(x))
				.ToList();

			foreach(var carTypeOfUse in carTypeOfUses)
			{
				var amount = _routeListRepository.GetCargoDailyNorm(carTypeOfUse);

				CargoDailyNormNodes.Add(
					new CargoDailyNormNode
					{
						CarTypeOfUse = carTypeOfUse,
						Amount = amount
					});
			}
		}

		public override bool Save()
		{
			var cargoDailyNormNodesDictionary =
				CargoDailyNormNodes.ToDictionary(x => x.CarTypeOfUse, x => x.Amount);

			_routeListRepository.SaveCargoDailyNorms(cargoDailyNormNodesDictionary);

			return true;
		}

		public List<CargoDailyNormNode> CargoDailyNormNodes { get; } = new List<CargoDailyNormNode>();

		public override string Title => "Настройка нормы вывоза в день";
	}
}
