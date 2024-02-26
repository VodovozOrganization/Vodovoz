using System;
using QS.ViewModels;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using QS.DomainModel.Entity;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Subdivisions;
using QS.DomainModel.NotifyChange;
using Vodovoz.Domain.Fuel;
using System.Linq;

namespace Vodovoz.ViewModels.Dialogs.Fuel
{
	public class FuelBalanceViewModel : UoWWidgetViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly IFuelRepository fuelRepository;

		public FuelBalanceViewModel(IUnitOfWorkFactory uowFactory, ISubdivisionRepository subdivisionRepository, IFuelRepository fuelRepository)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));

			UoW = _uowFactory.CreateWithoutRoot();

			ConfigEntityUpdateSubscribes();
			Update();
		}

		private Dictionary<FuelType, decimal> allFuelsBalance;
		[PropertyChangedAlso(nameof(HasAllFuelsBalance))]
		public virtual Dictionary<FuelType, decimal> AllFuelsBalance {
			get => allFuelsBalance;
			set => SetField(ref allFuelsBalance, value, () => AllFuelsBalance);
		}

		public bool HasAllFuelsBalance => AllFuelsBalance != null && AllFuelsBalance.Any();

		private Dictionary<Subdivision, Dictionary<FuelType,decimal>> subdivisionsFuelsBalance;
		[PropertyChangedAlso(nameof(HasSubdivisionsFuelsBalance))]
		public virtual Dictionary<Subdivision, Dictionary<FuelType,decimal>> SubdivisionsFuelsBalance {
			get => subdivisionsFuelsBalance;
			set => SetField(ref subdivisionsFuelsBalance, value, () => SubdivisionsFuelsBalance);
		}

		public bool HasSubdivisionsFuelsBalance => SubdivisionsFuelsBalance != null && SubdivisionsFuelsBalance.Any();

		private void ConfigEntityUpdateSubscribes()
		{
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<FuelType, FuelTransferDocument, FuelIncomeInvoice, FuelDocument>((changeEvents) => { Update(); });
		}

		private void Update()
		{
			AllFuelsBalance = fuelRepository.GetAllFuelsBalance(UoW);

			var cashSubdivisions = subdivisionRepository.GetCashSubdivisions(UoW);
			var subdivisionsBalance = new Dictionary<Subdivision, Dictionary<FuelType, decimal>>();
			foreach(var cashSubdivision in cashSubdivisions) {
				var fuelBalance = fuelRepository.GetAllFuelsBalanceForSubdivision(UoW, cashSubdivision);
				subdivisionsBalance.Add(cashSubdivision, fuelBalance);
			}
			SubdivisionsFuelsBalance = subdivisionsBalance;
		}
	}
}
