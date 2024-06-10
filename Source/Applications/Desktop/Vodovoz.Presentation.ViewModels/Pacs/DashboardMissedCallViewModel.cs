using QS.ViewModels;
using System;
using Vodovoz.Application.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardMissedCallViewModel : ViewModelBase
	{
		private readonly MissedCallModel _model;

		private string _time;
		private string _phone;
		private string _possibleOperatorsCount;

		public DashboardMissedCallViewModel(MissedCallModel missedCallModel)
		{
			_model = missedCallModel ?? throw new ArgumentNullException(nameof(missedCallModel));

			Time = _model.Started.ToString("HH:mm:ss");
			Phone = _model.CallModel.Call.FromNumber;
			PossibleOperatorsCount = _model.PossibleOperatorsCount.ToString();
		}

		public MissedCallModel Model => _model;

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
