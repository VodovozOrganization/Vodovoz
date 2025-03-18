using System.Collections.Generic;

namespace Pacs.Server.Phones
{
	/// <summary>
	/// Сервис для работы с телефонами операторов
	/// </summary>
	public interface IOperatorPhoneService
	{
		/// <summary>
		/// Возвращает идентификатор оператора, которому назначен телефон
		/// </summary>
		/// <param name="phone">Внутренний номер телефона</param>
		/// <returns></returns>
		int? GetAssignedOperator(string phone);
		/// <summary>
		/// Возвращает телефон, назначенный оператору
		/// </summary>
		/// <param name="operatorId">Идентификатор оператора</param>
		/// <returns></returns>
		string GetAssignedPhone(int operatorId);
		/// <summary>
		/// Возвращает все назначения телефонов
		/// </summary>
		/// <returns></returns>
		IEnumerable<PhoneAssignment> GetPhoneAssignments();
		/// <summary>
		/// Проверяет существует ли телефон
		/// </summary>
		/// <param name="phone">Внутренний номер телефона</param>
		/// <returns></returns>
		bool PhoneExists(string phone);
	}
}
