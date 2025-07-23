namespace SecureCodeSenderApi.Services
{
	/// <summary>
	/// Интерфейс общего валидатора
	/// </summary>
	public interface ISecureCodeServiceValidator
	{
		/// <summary>
		/// Проверка на корректность входящих данных
		/// </summary>
		/// <param name="data">Входящие данные</param>
		/// <returns>Описание ошибок в случае наличия или null</returns>
		string Validate(object data);
	}
}
