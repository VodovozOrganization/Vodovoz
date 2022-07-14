using QS.Project.Filter;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class DeliveryScheduleFilterViewModel : FilterViewModelBase<DeliveryScheduleFilterViewModel>
	{
		private bool _isNotArchive;
		private bool _canChangeIsNotArchive;

		public DeliveryScheduleFilterViewModel()
		{
			CanChangeIsNotArchive = true;
		}

		public bool RestrictIsNotArchive
		{
			get => IsNotArchive;
			set
			{
				IsNotArchive = value;
				CanChangeIsNotArchive = false;
			}
		}

		public bool IsNotArchive
		{
			get => _isNotArchive;
			set => UpdateFilterField(ref _isNotArchive, value);
		}

		public virtual bool CanChangeIsNotArchive
		{
			get => _canChangeIsNotArchive;
			set => SetField(ref _canChangeIsNotArchive, value);
		}
	}
}
