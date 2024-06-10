using Gamma.Utilities;
using QS.Dialog;
using QS.ViewModels;
using System;
using System.ComponentModel;
using Vodovoz.Application.Pacs;

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
		private string _result;

		public DashboardCallViewModel(CallModel callModel, IGuiDispatcher guiDispatcher)
		{
			_model = callModel ?? throw new ArgumentNullException(nameof(callModel));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			_model.PropertyChanged += OnModelPropertyChanged;
			Time = _model.Started.ToString("HH:mm:ss");
			Phone = _model.Call.FromNumber;
			Operator = GetOperator();
			State = GetState();
			Result = GetResult();
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

		public virtual string Result
		{
			get => _result;
			private set => SetField(ref _result, value);
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(CallModel.Call):
					var oper = GetOperator();
					var state = GetState();
					var result = GetResult();

					_guiDispatcher.RunInGuiTread(() =>
					{
						Operator = oper;
						State = state;
						Result = result;
					});
					break;
				case nameof(CallModel.Operator):
					var oper2 = GetOperator();

					_guiDispatcher.RunInGuiTread(() =>
					{
						Operator = oper2;
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
			return AttributeUtil.GetEnumTitle(_model.Call.Status);
		}

		private string GetResult()
		{
			if(_model.Call.EntryResult == null)
			{
				return "";
			}
			return AttributeUtil.GetEnumTitle(_model.Call.EntryResult);
		}
	}
}
