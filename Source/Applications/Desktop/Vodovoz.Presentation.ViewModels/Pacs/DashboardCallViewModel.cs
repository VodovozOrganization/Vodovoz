using Gamma.Utilities;
using QS.Dialog;
using QS.ViewModels;
using System;
using System.ComponentModel;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardCallViewModel : ViewModelBase
	{
		private readonly CallModel _callModel;
		private readonly IGuiDispatcher _guiDispatcher;

		private string _time;
		private string _phone;
		private string _operator;
		private string _state;

		public DashboardCallViewModel(CallModel callModel, IGuiDispatcher guiDispatcher)
		{
			_callModel = callModel ?? throw new ArgumentNullException(nameof(callModel));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			_callModel.PropertyChanged += OnModelPropertyChanged;
			Time = _callModel.CurrentState.EventTime.ToString("HH:mm:ss");
			Phone = _callModel.CurrentState.FromNumber
				+ (string.IsNullOrWhiteSpace(_callModel.CurrentState.FromExtension) ? "" : $" ({_callModel.CurrentState.FromExtension})");
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

		public virtual string Operator
		{
			get => _operator;
			private set => SetField(ref _operator, value);
		}

		public virtual string State
		{
			get => _state;
			private set => SetField(ref _state, value);
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(CallModel.CurrentState):
					var oper = _callModel.Operator;
					var state = AttributeUtil.GetEnumTitle(_callModel.CurrentState.CallState);

					_guiDispatcher.RunInGuiTread(() =>
					{
						Operator = oper.Employee.GetPersonNameWithInitials();
						State = state;
					});
					break;
				default:
					break;
			}
		}
	}
}
