using Gamma.Utilities;
using QS.Dialog;
using QS.ViewModels;
using System;
using System.ComponentModel;
using System.Timers;
using Vodovoz.Application.Pacs;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class DashboardOperatorOnBreakViewModel : ViewModelBase, IDisposable
	{
		private readonly Timer _timer;
		private readonly OperatorModel _model;
		private readonly IGuiDispatcher _guiDispatcher;
		private IPacsDomainSettings _settings;
		private DateTime? _breakStartTime;
		private bool _breakTimeGone;
		private OperatorBreakType _breakType;
		private string _name;
		private string _phone;


		public DashboardOperatorOnBreakViewModel(OperatorModel operatorModel, IGuiDispatcher guiDispatcher)
		{
			_model = operatorModel ?? throw new ArgumentNullException(nameof(operatorModel));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_settings = operatorModel.Settings;

			if(_model.CurrentState.BreakType.HasValue)
			{
				BreakType = _model.CurrentState.BreakType.Value;
			}
			Name = _model.Employee.GetPersonNameWithInitials();
			Phone = _model.CurrentState.PhoneNumber;
			BreakStartTime = _model.CurrentState.Started;

			_timer = new Timer(1000);
			_timer.Elapsed += OnTick;
			_timer.Start();

			_model.PropertyChanged += OnModelPropertyChanged;
		}

		private void OnTick(object sender, ElapsedEventArgs e)
		{
			_guiDispatcher.RunInGuiTread(() => {
				OnPropertyChanged(nameof(TimeRemains));
			});
		}

		public OperatorModel Model => _model;

		public virtual OperatorBreakType BreakType
		{
			get => _breakType;
			set => SetField(ref _breakType, value);
		}

		public string Break => BreakType.GetEnumTitle();

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

		public virtual DateTime? BreakStartTime
		{
			get => _breakStartTime;
			private set => SetField(ref _breakStartTime, value);
		}

		public virtual bool BreakTimeGone
		{
			get => _breakTimeGone;
			private set => SetField(ref _breakTimeGone, value);
		}

		public string TimeRemains
		{
			get
			{
				if(BreakStartTime == null)
				{
					return "";
				}

				TimeSpan remains;
				if(BreakType == OperatorBreakType.Long)
				{
					remains = BreakStartTime.Value + _settings.LongBreakDuration - DateTime.Now;
				}
				else
				{
					remains = BreakStartTime.Value + _settings.ShortBreakDuration - DateTime.Now;
				}

				BreakTimeGone = remains < TimeSpan.Zero;
				var format = (_breakTimeGone ? "\\-" : "") + "hh\\ч\\.\\ mm\\м\\.";
				return remains.ToString(format);
			}
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(OperatorModel.Settings):
					_settings = _model.Settings;
					_guiDispatcher.RunInGuiTread(() => {
						OnPropertyChanged(nameof(TimeRemains));
					});
					break;
				case nameof(OperatorModel.CurrentState):
					var phone = _model.CurrentState.PhoneNumber;
					var breakStartTime = _model.CurrentState.Started;

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
			_timer?.Dispose();
		}
	}
}
