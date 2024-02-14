using Gamma.Utilities;
using QS.Dialog;
using QS.ViewModels;
using System;
using System.ComponentModel;
using Vodovoz.Application.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardOperatorViewModel : ViewModelBase
	{
		private readonly OperatorModel _model;
		private readonly IGuiDispatcher _guiDispatcher;

		private string _name;
		private string _phone;
		private string _state;
		private string _connectedToCall;

		public DashboardOperatorViewModel(OperatorModel operatorModel, IGuiDispatcher guiDispatcher)
		{
			_model = operatorModel ?? throw new ArgumentNullException(nameof(operatorModel));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			_model.PropertyChanged += OnModelPropertyChanged;
			Name = _model.Employee.GetPersonNameWithInitials();
			Phone = GetPhone();
			State = GetState();
			ConnectedToCall = GetConnectedCall();
		}

		public OperatorModel Model => _model;

		public virtual string Name
		{
			get => _name;
			private set => SetField(ref _name, value);
		}

		public virtual string Phone
		{
			get => _phone;
			private set => SetField(ref _phone, value);
		}

		public virtual string State
		{
			get => _state;
			private set => SetField(ref _state, value);
		}

		public virtual string ConnectedToCall
		{
			get => _connectedToCall;
			private set => SetField(ref _connectedToCall, value);
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(OperatorModel.CurrentState):
					var _phone = GetPhone();
					var state = GetState();
					var connectedToCall = GetConnectedCall();

					_guiDispatcher.RunInGuiTread(() =>
					{
						Phone = _phone;
						State = state;
						ConnectedToCall = connectedToCall;
					});
					break;
				default:
					break;
			}
		}

		private string GetPhone()
		{
			return _model.CurrentState.PhoneNumber;
		}

		private string GetState()
		{
			return AttributeUtil.GetEnumTitle(_model.CurrentState.State);
		}

		private string GetConnectedCall()
		{
			return _model.GetConnectedCallNumber();
		}
	}
}
