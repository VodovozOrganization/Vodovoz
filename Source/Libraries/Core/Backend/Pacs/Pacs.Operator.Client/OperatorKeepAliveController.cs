using Pacs.Core.Messages.Events;
using System;
using System.Timers;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Settings.Pacs;

namespace Pacs.Operators.Client
{
	public class OperatorKeepAliveController
	{
		private readonly IOperatorClient _operatorClient;
		private readonly IPacsSettings _pacsSettings;
		private Timer _timer;

		public OperatorKeepAliveController(IOperatorClient operatorClient, IPacsSettings pacsSettings)
		{
			_operatorClient = operatorClient ?? throw new ArgumentNullException(nameof(operatorClient));
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));
			_operatorClient.StateChanged += StateChanged;
		}

		private void StateChanged(object sender, OperatorStateEvent e)
		{
			switch(e.State.State)
			{
				case OperatorStateType.Connected:
				case OperatorStateType.WaitingForCall:
				case OperatorStateType.Talk:
				case OperatorStateType.Break:
					Start();
					break;
				case OperatorStateType.New:
				case OperatorStateType.Disconnected:
				default:
					Stop();
					break;
			}
		}

		public void Start()
		{
			if(_operatorClient.OperatorId == null)
			{
				return;
			}
			if(_timer != null)
			{
				return;
			}
			_timer = new Timer();
			_timer.Interval = _pacsSettings.OperatorKeepAliveInterval.TotalMilliseconds;
			_timer.Elapsed += TimerElapsed;
			_timer.Start();
		}

		private void TimerElapsed(object sender, ElapsedEventArgs e)
		{
			_operatorClient.KeepAlive();
		}

		public void Stop()
		{
			_timer?.Dispose();
			_timer = null;
		}
	}
}
