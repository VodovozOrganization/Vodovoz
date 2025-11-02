using Pacs.Core.Messages.Events;

namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Результат команды оператора
	/// </summary>
	public class OperatorResult : CommandResult
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		public OperatorResult()
		{
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="actualState">Событие состояния оператора</param>
		public OperatorResult(OperatorStateEvent actualState)
		{
			OperatorState = actualState;
			Result = Result.Success;
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="actualState">Событие состояния оператора</param>
		/// <param name="failureDescription">Описание ошибки выполнения команды</param>
		public OperatorResult(OperatorStateEvent actualState, string failureDescription)
		{
			OperatorState = actualState;
			Result = Result.Failure;
			FailureDescription = failureDescription;
		}

		/// <summary>
		/// Событие состояния оператора
		/// </summary>
		public OperatorStateEvent OperatorState { get; set; }
	}
}
