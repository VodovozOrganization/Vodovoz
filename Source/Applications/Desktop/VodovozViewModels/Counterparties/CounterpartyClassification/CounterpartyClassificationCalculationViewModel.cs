using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Domain.Client.CounterpartyClassification;

namespace Vodovoz.ViewModels.Counterparties.CounterpartyClassification
{
	public class CounterpartyClassificationCalculationViewModel : WindowDialogViewModelBase, IDisposable
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<CounterpartyClassificationCalculationViewModel> _logger;

		public CounterpartyClassificationCalculationViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			ILogger<CounterpartyClassificationCalculationViewModel> logger
			) : base(navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();

			CalculationSettings = new CounterpartyClassificationCalculationSettings();
		}

		public CounterpartyClassificationCalculationSettings CalculationSettings { get; set; }

		#region IDisposable implementation
		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
		#endregion
	}
}
