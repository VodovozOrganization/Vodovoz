using System.Collections.Generic;

namespace VodovozHealthCheck.Dto
{
	/// <summary>
	///		Модель результата проверки работоспособности службы.
	///		Содержит флаг состояния и набор дополнительных сообщений об ошибках для неуспешных проверок.
	/// </summary>
	public class VodovozHealthResultDto
	{
		/// <summary>
		///		Признак успешности проверки.
		///		Значение true означает, что проверка пройдена успешно; false — обнаружены ошибки/несоответствия.
		/// </summary>
		public bool IsHealthy { get; set; }

		/// <summary>
		///		Коллекция дополнительных сообщений об ошибках или причинах непрохождения проверки.
		///		Инициализируется пустым набором по умолчанию.
		/// </summary>
		public HashSet<string> AdditionalUnhealthyResults { get; set; } = new();

		/// <summary>
		///		Создаёт и возвращает успешный результат проверки.
		/// </summary>
		/// <returns>Экземпляр <see cref="VodovozHealthResultDto"/> с <see cref="IsHealthy"/> = true.</returns>
		public static VodovozHealthResultDto HealthyResult() => new() { IsHealthy = true };

		/// <summary>
		///		Создаёт и возвращает неуспешный результат проверки с одним сообщением об ошибке.
		/// </summary>
		/// <param name="errorMessage">Текст сообщения об ошибке. По умолчанию: "Не прошёл валидацию".</param>
		/// <returns>Экземпляр <see cref="VodovozHealthResultDto"/> с <see cref="IsHealthy"/> = false и заполненным сообщением.</returns>
		public static VodovozHealthResultDto UnhealthyResult(string errorMessage = "Не прошёл валидацию")
		{
			return new VodovozHealthResultDto
			{
				IsHealthy = false,
				AdditionalUnhealthyResults = new HashSet<string> { errorMessage }
			};
		}

		/// <summary>
		///		Формирует результат проверки на основе булевого условия и опционального сообщения об ошибке.
		///		Если <paramref name="isHealthy"/> = true — возвращается успешный результат.
		///		Если false — формируется неуспешный результат с сообщением, содержащим имя проверяемого метода и причину.
		/// </summary>
		/// <param name="checkMethodName">Название проверяемого метода или эндпоинта для включения в сообщение об ошибке.</param>
		/// <param name="isHealthy">Флаг, указывающий, прошла ли проверка.</param>
		/// <param name="errorMessage">Дополнительное сообщение об ошибке. Если null, будет использовано сообщение по умолчанию.</param>
		/// <returns>Экземпляр <see cref="VodovozHealthResultDto"/> представляющий итог проверки.</returns>
		public static VodovozHealthResultDto FromCondition(
			string checkMethodName,
			bool isHealthy,
			string errorMessage = null)
		{
			return isHealthy
				? new VodovozHealthResultDto { IsHealthy = true }
				: new VodovozHealthResultDto
				{
					IsHealthy = false,
					AdditionalUnhealthyResults = new HashSet<string> { $"Проверка эндпоинта <{checkMethodName}> не пройдена  : {errorMessage ?? "Условие не выполнено"}" }
				};
		}
	}
}
