using Pacs.Core.Messages.Commands;
using System;
using System.Threading.Tasks;

namespace Pacs.Server.Operators
{
	public interface IOperatorStateService
	{
		Task<OperatorResult> ChangePhone(int operatorId, string phoneNumber);
		Task<OperatorResult> Connect(int operatorId);
		Task<OperatorResult> Disconnect(int operatorId);
		Task<OperatorResult> StartBreak(int operatorId, OperatorBreakType breakType);
		Task<OperatorResult> EndBreak(int operatorId);
		Task<OperatorResult> StartWorkShift(int operatorId, string phoneNumber);
		Task<OperatorResult> EndWorkShift(int operatorId, string reason);
		Task KeepAlive(int operatorId);

		[Obsolete("Старый метод, нужно заменить логику в месте использования", true)]
		OperatorController GetOperatorController(int operatorId);

		[Obsolete("Старый метод, нужно заменить логику в месте использования", true)]
		OperatorController GetOperatorController(string phoneNumber);
	}
}
