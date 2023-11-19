using Gamma.Utilities;
using QS.Dialog;
using QS.ViewModels;
using System;
using System.ComponentModel;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardOperatorViewModel : ViewModelBase
	{
		private readonly OperatorModel _operatorModel;
		private readonly IGuiDispatcher _guiDispatcher;

		private string _name;
		private string _phone;
		private string _state;

		public DashboardOperatorViewModel(OperatorModel operatorModel, IGuiDispatcher guiDispatcher)
		{
			_operatorModel = operatorModel ?? throw new ArgumentNullException(nameof(operatorModel));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			_operatorModel.PropertyChanged += OnModelPropertyChanged;
			Name = _operatorModel.Employee.GetPersonNameWithInitials();
		}

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

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(OperatorModel.CurrentState):
					var phone = _operatorModel.CurrentState.PhoneNumber;
					var state = AttributeUtil.GetEnumTitle(_operatorModel.CurrentState.State);

					_guiDispatcher.RunInGuiTread(() =>
					{
						Phone = phone;
						State = state;
					});
					break;
				default:
					break;
			}
		}
	}
}
