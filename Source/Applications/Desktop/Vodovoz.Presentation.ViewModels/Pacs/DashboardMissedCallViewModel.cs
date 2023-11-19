using QS.ViewModels;
using System;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardMissedCallViewModel : ViewModelBase
	{
		private readonly MissedCallModel _missedCallModel;

		private string _time;
		private string _phone;
		private string _possibleOperatorsCount;

		public DashboardMissedCallViewModel(MissedCallModel missedCallModel)
		{
			_missedCallModel = missedCallModel ?? throw new ArgumentNullException(nameof(missedCallModel));

			Time = _missedCallModel.Started.ToString("HH:mm:ss");
			Phone = _missedCallModel.Call.CurrentState.FromNumber
				+ (string.IsNullOrWhiteSpace(_missedCallModel.Call.CurrentState.FromExtension) ? "" : $" ({_missedCallModel.Call.CurrentState.FromExtension})");
			PossibleOperatorsCount = _missedCallModel.PossibleOperatorsCount.ToString();
		}

		public virtual string Time
		{
			get => _time;
			private set => SetField(ref _time, value);
		}

		public virtual string Phone
		{
			get => _phone;
			private set => SetField(ref _phone, value);
		}

		public virtual string PossibleOperatorsCount
		{
			get => _possibleOperatorsCount;
			private set => SetField(ref _possibleOperatorsCount, value);
		}
	}
}
