using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public class CarIsNotAtLineReportParametersViewModel : DialogViewModelBase
	{
		private DateTime _date;
		private int _countDays;

		private CarIsNotAtLineReport _report;

		public CarIsNotAtLineReportParametersViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
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
		}

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
			_report = CarIsNotAtLineReport.Generate();
		}

		private void ExportReport()
		{

		}
	}
}
