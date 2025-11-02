using System;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core
{
	public static class OperatorStateTriggerExtensions
	{
		public static OperatorStateTrigger ToOperatorStateTrigger(this OperatorTrigger operatorTrigger)
		{
			switch(operatorTrigger)
			{
				case OperatorTrigger.Connect:
					return OperatorStateTrigger.Connect;
				case OperatorTrigger.StartWorkShift:
					return OperatorStateTrigger.StartWorkShift;
				case OperatorTrigger.TakeCall:
					return OperatorStateTrigger.TakeCall;
				case OperatorTrigger.EndCall:
					return OperatorStateTrigger.EndCall;
				case OperatorTrigger.StartBreak:
					return OperatorStateTrigger.StartBreak;
				case OperatorTrigger.EndBreak:
					return OperatorStateTrigger.EndBreak;
				case OperatorTrigger.ChangePhone:
					return OperatorStateTrigger.ChangePhone;
				case OperatorTrigger.EndWorkShift:
					return OperatorStateTrigger.EndWorkShift;
				case OperatorTrigger.Disconnect:
					return OperatorStateTrigger.Disconnect;
				default:
					throw new InvalidOperationException(
						$"Неизвестный триггер {operatorTrigger}. " +
						$"Необходимо проверить настройки состояний.");
			}
		}

		public static OperatorTrigger ToOperatorTrigger(this OperatorStateTrigger operatorStateTrigger)
		{
			switch(operatorStateTrigger)
			{
				case OperatorStateTrigger.Connect:
					return OperatorTrigger.Connect;
				case OperatorStateTrigger.StartWorkShift:
					return OperatorTrigger.StartWorkShift;
				case OperatorStateTrigger.TakeCall:
					return OperatorTrigger.TakeCall;
				case OperatorStateTrigger.EndCall:
					return OperatorTrigger.EndCall;
				case OperatorStateTrigger.StartBreak:
					return OperatorTrigger.StartBreak;
				case OperatorStateTrigger.EndBreak:
					return OperatorTrigger.EndBreak;
				case OperatorStateTrigger.ChangePhone:
					return OperatorTrigger.ChangePhone;
				case OperatorStateTrigger.EndWorkShift:
					return OperatorTrigger.EndWorkShift;
				case OperatorStateTrigger.Disconnect:
					return OperatorTrigger.Disconnect;
				case OperatorStateTrigger.KeepAlive:
				case OperatorStateTrigger.CheckInactivity:
				default:
					throw new InvalidOperationException(
						$"Триггер {operatorStateTrigger} не конвертируется в тип {nameof(OperatorTrigger)}, " +
						$"так как не предусмотрено его сохранение. " +
						$"Необходимо проверить настройки состояний.");
			}
		}
	}
}
