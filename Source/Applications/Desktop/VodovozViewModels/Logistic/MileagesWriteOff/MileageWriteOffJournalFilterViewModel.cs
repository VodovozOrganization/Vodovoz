using QS.Project.Filter;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Logistic.MileagesWriteOff
{
	public class MileageWriteOffJournalFilterViewModel : FilterViewModelBase<MileageWriteOffJournalFilterViewModel>
	{
		private DateTime? _writeOffDateFrom;
		private DateTime? _writeOffDateTo;
		private Car _car;
		private Employee _driver;
		private Employee _author;

		public DateTime? WriteOffDateFrom
		{
			get => _writeOffDateFrom;
			set => UpdateFilterField(ref _writeOffDateFrom, value);
		}

		public DateTime? WriteOffDateTo
		{
			get => _writeOffDateTo;
			set => UpdateFilterField(ref _writeOffDateTo, value);
		}

		public Car Car
		{
			get => _car;
			set => UpdateFilterField(ref _car, value);
		}

		public Employee Driver
		{
			get => _driver;
			set => UpdateFilterField(ref _driver, value);
		}

		public Employee Author
		{
			get => _author;
			set => UpdateFilterField(ref _author, value);
		}
	}
}
