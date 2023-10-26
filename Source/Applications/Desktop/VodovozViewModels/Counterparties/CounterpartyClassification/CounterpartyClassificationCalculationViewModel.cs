using Microsoft.Extensions.Logging;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Domain.Client.CounterpartyClassification;

namespace Vodovoz.ViewModels.Counterparties.CounterpartyClassification
{
	public class CounterpartyClassificationCalculationViewModel : DialogTabViewModelBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<CounterpartyClassificationCalculationViewModel> _logger;

		public CounterpartyClassificationCalculationViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ILogger<CounterpartyClassificationCalculationViewModel> logger
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();

			Title = "Пересчёт классификации контрагентов";

			CreateCalculationSettings();
		}

		public CounterpartyClassificationCalculationSettings CalculationSettings { get; set; }

		private void CreateCalculationSettings()
		{
			CalculationSettings = new CounterpartyClassificationCalculationSettings();

			var lastSettings = _unitOfWork.GetAll<CounterpartyClassificationCalculationSettings>()
				.OrderByDescending(x => x.SettingsCreationDate)
				.FirstOrDefault();

			if(lastSettings != null)
			{
				CalculationSettings.PeriodInMonths = lastSettings.PeriodInMonths;
				CalculationSettings.BottlesCountAClassificationFrom = lastSettings.BottlesCountAClassificationFrom;
				CalculationSettings.BottlesCountCClassificationTo = lastSettings.BottlesCountCClassificationTo;
				CalculationSettings.OrdersCountXClassificationFrom = lastSettings.OrdersCountXClassificationFrom;
				CalculationSettings.OrdersCountZClassificationTo = lastSettings.OrdersCountZClassificationTo;
				CalculationSettings.SettingsCreationDate = DateTime.Now;
			}
		}

		#region IDisposable implementation
		public override void Dispose()
		{
			_unitOfWork?.Dispose();

			base.Dispose();
		}
		#endregion
	}
}
