using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Logistic;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public class CarIsNotAtLineReportParametersViewModel : DialogViewModelBase
	{
		private DateTime _date;
		private int _countDays;

		private CarIsNotAtLineReport _report;
		private readonly IGenericRepository<CarEvent> _carEventRepository;

		public CarIsNotAtLineReportParametersViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IGenericRepository<CarEvent> carEventRepository,
			INavigationManager navigation)
			: base(navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			Title = "Отчёт по простою";

			UnitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);

			Date = DateTime.Today;
			CountDays = 4;

			GenerateReportCommand = new DelegateCommand(GenerateReport);

			var includeExludeFilterGroupViewModel = new IncludeExludeFilterGroupViewModel();

			includeExludeFilterGroupViewModel.InitializeFor(UnitOfWork, _carEventRepository);
			includeExludeFilterGroupViewModel.RefreshFilteredElementsCommand.Execute();

			IncludeExludeFilterGroupViewModel = includeExludeFilterGroupViewModel;

			_carEventRepository = carEventRepository;
		}

		public IncludeExludeFilterGroupViewModel IncludeExludeFilterGroupViewModel { get; }

		public DelegateCommand GenerateReportCommand { get; }

		public IUnitOfWork UnitOfWork { get; }

		public DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		public int CountDays
		{
			get => _countDays;
			set => SetField(ref _countDays, value);
		}

		private void GenerateReport()
		{
			_report = CarIsNotAtLineReport.Generate(
				Date,
				CountDays,
				IncludeExludeFilterGroupViewModel.IncludedElements.Select(e => e.Number),
				IncludeExludeFilterGroupViewModel.ExcludedElements.Select(e => e.Number));
		}

		private void ExportReport()
		{

		}
	}
}
