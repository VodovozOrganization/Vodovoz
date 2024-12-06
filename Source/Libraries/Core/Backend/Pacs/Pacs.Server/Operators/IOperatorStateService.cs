using Pacs.Core.Messages.Commands;
using Pacs.Core.Messages.Events;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public interface IOperatorStateService
	{
		/// <summary>
		/// Смена телефона оператора
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="phoneNumber">Новый номер телефона</param>
		/// <returns></returns>
		Task<OperatorResult> ChangePhone(int operatorId, string phoneNumber);

		/// <summary>
		/// Подключение оператора
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		Task<OperatorResult> Connect(int operatorId);

		/// <summary>
		/// Отключение оператора
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		Task<OperatorResult> Disconnect(int operatorId);

		/// <summary>
		/// Начало перерыва
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="breakType"><see cref="OperatorBreakType">Тип перерыва</see></param>
		/// <returns></returns>
		Task<OperatorResult> StartBreak(int operatorId, OperatorBreakType breakType);

		/// <summary>
		/// Завершение перерыва
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		Task<OperatorResult> EndBreak(int operatorId);

		/// <summary>
		/// Начало смены
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="phoneNumber">Номер телефона</param>
		/// <returns></returns>
		Task<OperatorResult> StartWorkShift(int operatorId, string phoneNumber);

		/// <summary>
		/// Завершение смены оператора
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="reason">Причина</param>
		/// <returns></returns>
		Task<OperatorResult> EndWorkShift(int operatorId, string reason);

		/// <summary>
		/// Поддержание состояния подключения
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		Task KeepAlive(int operatorId);

		/// <summary>
		/// Начало перерыва администратором
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="breakType"><see cref="OperatorBreakType">Тип перерыва</see></param>
		/// <param name="adminId">Идентификатор администратора</param>
		/// <param name="reason">Причина</param>
		/// <returns></returns>
		Task<OperatorResult> AdminStartBreak(int operatorId, OperatorBreakType breakType, int adminId, string reason);

		/// <summary>
		/// Завершение перерыва администратором
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="adminId">Идентификатор администратора</param>
		/// <param name="reason">Причина</param>
		/// <returns></returns>
		Task<OperatorResult> AdminEndBreak(int operatorId, int adminId, string reason);

		/// <summary>
		/// Завершение смены оператора администратором
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <param name="adminId">Идентификатор администратора</param>
		/// <param name="reason">Причина</param>
		/// <returns></returns>
		Task<OperatorResult> AdminEndWorkShift(int operatorId, int adminId, string reason);

		/// <summary>
		/// Принятие звонка
		/// </summary>
		/// <param name="toExtension">Добавочный номер оператора</param>
		/// <param name="callId">Идентификатор звонка</param>
		/// <returns></returns>
		Task TakeCall(string toExtension, string callId);

		/// <summary>
		/// Завершение звонка
		/// </summary>
		/// <param name="toExtension">Добавочный номер оператора</param>
		/// <param name="callId">Идентификатор звонка</param>
		/// <returns></returns>
		Task EndCall(string toExtension, string callId);

		/// <summary>
		/// Получение текущей возможности взять перерыв
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		Task<OperatorBreakAvailability> GetBreakAvailability(int operatorId);
	}
}
