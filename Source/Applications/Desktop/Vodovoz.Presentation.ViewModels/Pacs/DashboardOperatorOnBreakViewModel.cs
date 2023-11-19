using QS.Dialog;
using QS.ViewModels;
using System;
using System.ComponentModel;
using System.Timers;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardOperatorOnBreakViewModel : ViewModelBase, IDisposable
	{
		private readonly Timer _timer;
		private readonly OperatorModel _operatorModel;
		private readonly IGuiDispatcher _guiDispatcher;

		private DateTime _breakStartTime;
		private string _name;
		private string _phone;

		public DashboardOperatorOnBreakViewModel(OperatorModel operatorModel, IGuiDispatcher guiDispatcher)
		{
			_operatorModel = operatorModel ?? throw new ArgumentNullException(nameof(operatorModel));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			Name = _operatorModel.Employee.GetPersonNameWithInitials();

			_timer = new Timer(60000);
			_timer.Elapsed += OnTick;
			_timer.Start();

			_operatorModel.PropertyChanged += OnModelPropertyChanged;
		}

		private void OnTick(object sender, ElapsedEventArgs e)
		{
			_guiDispatcher.RunInGuiTread(() => { 
				OnPropertyChanged(nameof(TimeRemains));
			});
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

		public virtual DateTime BreakStartTime
		{
			get => _breakStartTime;
			private set => SetField(ref _breakStartTime, value);
		}

		public string TimeRemains => (DateTime.Now - BreakStartTime).ToString("hh:mm");

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(OperatorModel.CurrentState):
					var phone = _operatorModel.CurrentState.PhoneNumber;
					var breakStartTime = _operatorModel.CurrentState.Started;

					_guiDispatcher.RunInGuiTread(() => {
						Phone = phone;
						BreakStartTime = breakStartTime;
						OnPropertyChanged(nameof(TimeRemains));
					});
					break;
				default:
					break;
			}
		}

		public void Dispose()
		{
			_timer.Dispose();
		}
	}
}
