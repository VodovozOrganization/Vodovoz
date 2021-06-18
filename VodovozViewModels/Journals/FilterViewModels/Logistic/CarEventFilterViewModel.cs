using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class CarEventFilterViewModel : FilterViewModelBase<CarEventFilterViewModel>
	{
		private DateTime? _createEventDateFrom;
		private DateTime? _createEventDateEndTo;
		private DateTime? _startEventDateFrom;
		private DateTime? _startEventDateTo;
		private DateTime? _endEventDateFrom;
		private DateTime? _endEventDateTo;
		private Employee _author;
		private Car _car;
		private Employee _driver;
		private CarEventType _carEventType;
		public CarEventFilterViewModel(ICarJournalFactory carJournalFactory, ICarEventTypeJournalFactory carEventTypeJournalFactory)
		{
			CarSelectorFactory = carJournalFactory.CreateCarAutocompleteSelectorFactory();
			CarEventTypeSelectorFactory = carEventTypeJournalFactory.CreateCarEventTypeAutocompleteSelectorFactory();
		}

		public IEntityAutocompleteSelectorFactory CarSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory CarEventTypeSelectorFactory { get; }

		public DateTime? CreateEventDateFrom
		{
			get => _createEventDateFrom;
			set => UpdateFilterField(ref _createEventDateFrom, value);
		}

		public DateTime? CreateEventDateTo
		{
			get => _createEventDateEndTo;
			set => UpdateFilterField(ref _createEventDateEndTo, value);
		}

		public DateTime? StartEventDateFrom
		{
			get => _startEventDateFrom;
			set => UpdateFilterField(ref _startEventDateFrom, value);
		}

		public DateTime? StartEventDateTo
		{
			get => _startEventDateTo;
			set => UpdateFilterField(ref _startEventDateTo, value);
		}

		public DateTime? EndEventDateFrom
		{
			get => _endEventDateFrom;
			set => UpdateFilterField(ref _endEventDateFrom, value);
		}

		public DateTime? EndEventDateTo
		{
			get => _endEventDateTo;
			set => UpdateFilterField(ref _endEventDateTo, value);
		}

		public Employee Author
		{
			get => _author;
			set => UpdateFilterField(ref _author, value);
		}

		public Car Car
		{
			get => (_car);
			set => UpdateFilterField(ref _car, value);
		}

		public Employee Driver
		{
			get => _driver;
			set => UpdateFilterField(ref _driver, value);
		}

		public CarEventType CarEventType
		{
			get => _carEventType;
			set => UpdateFilterField(ref _carEventType, value);
		}
	}
}
