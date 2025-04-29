namespace Pacs.Server.Phones
{
	public interface IOperatorPhoneService
	{
		/// <summary>
		/// Проверяет известен ли номер в системе
		/// </summary>
		bool ValidatePhone(string phone);

		/// <summary>
		/// Проверяет можно ли назначить номер телефона оператору
		/// </summary>
		/// <param name="phone">Номер телефона который необходимо проверить</param>
		/// <param name="operatorId">Id оператора на которого проверяется назначение телефона</param>
		/// <returns></returns>
		bool CanAssign(string phone, int operatorId);

		/// <summary>
		/// Назначает номер телефона оператору
		/// </summary>
		/// <param name="phone">Номер телефона который необходимо назначить</param>
		/// <param name="operatorId">Id оператора на которого необходимо назначить телефон</param>
		/// <exception cref="PacsPhoneException">Если телефон не известен 
		/// или уже назначен другому оператору</exception>
		void AssignPhone(string phone, int operatorId);

		/// <summary>
		/// Снимает назначение номера телефона от оператора
		/// </summary>
		/// <param name="phone">Номер телефона который необходимо снять</param>
		/// <exception cref="PacsPhoneException">Если телефон не известен</exception>
		void ReleasePhone(string phone);
	}
}
