using Gamma.Utilities;
using QS.Dialog;
using QS.ViewModels;
using System;
using System.ComponentModel;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardCallViewModel : ViewModelBase
	{
		private readonly CallModel _model;
		private readonly IGuiDispatcher _guiDispatcher;

		private string _time;
		private string _phone;
		private string _operator;
		private string _state;

		public DashboardCallViewModel(CallModel callModel, IGuiDispatcher guiDispatcher)
		{
			_model = callModel ?? throw new ArgumentNullException(nameof(callModel));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			_model.PropertyChanged += OnModelPropertyChanged;
			Time = _model.CurrentState.EventTime.ToString("HH:mm:ss");
			Phone = _model.CurrentState.FromNumber
				+ (string.IsNullOrWhiteSpace(_model.CurrentState.FromExtension) ? "" : $" ({_model.CurrentState.FromExtension})");
			Operator = GetOperator();
			State = GetState();
		}

		public CallModel Model => _model;

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
					var oper = GetOperator();
					var state = GetState();

					_guiDispatcher.RunInGuiTread(() =>
					{
						Operator = oper;
						State = state;
					});
					break;
				default:
					break;
			}
		}

		private string GetOperator()
		{
			if(_model.Operator == null)
			{
				return "";
			}
			return _model.Operator.Employee.GetPersonNameWithInitials();
		}

		private string GetState()
		{
			return AttributeUtil.GetEnumTitle(_model.CurrentState.CallState);
		}
	}
}
