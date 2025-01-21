using Microsoft.Extensions.Logging;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Fuel;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Transport.Reports.IncorrectFuel
{
	public class IncorrectFuelReportViewModel : DialogTabViewModelBase
	{
		private readonly ILogger<IncorrectFuelReportViewModel> _logger;
		private readonly IInteractiveService _interactiveService;

		private DateTime _startDate;
		private DateTime _endDate;
		private Car _car;
		private FuelCard _fuelCard;

		public IncorrectFuelReportViewModel(
			ILogger<IncorrectFuelReportViewModel> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_interactiveService = interactiveService ?? throw new System.ArgumentNullException(nameof(interactiveService));
		}

		public DateTime StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		public FuelCard FuelCard
		{
			get => _fuelCard;
			set => SetField(ref _fuelCard, value);
		}

	}
}
