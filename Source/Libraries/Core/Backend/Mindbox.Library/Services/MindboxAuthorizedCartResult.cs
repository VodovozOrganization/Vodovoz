using System.Collections.Generic;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Результат расчёта бонусов заказа в Mindbox
	/// </summary>
	public enum MindboxAuthorizedCartOutcome
	{
		/// <summary>
		/// Расчёт выполнен
		/// </summary>
		Success = 0,

		/// <summary>
		/// Расчёт временно недоступен
		/// </summary>
		ServiceUnavailable = 1
	}

	/// <summary>
	/// Результат операции CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartResult
	{
		/// <summary>
		/// Признак успешного расчёта
		/// </summary>
		public bool IsSuccess => Outcome == MindboxAuthorizedCartOutcome.Success;

		/// <summary>
		/// Итог операции
		/// </summary>
		public MindboxAuthorizedCartOutcome Outcome { get; private set; }

		/// <summary>
		/// Доступно бонусов на счёте клиента
		/// </summary>
		public int? Balance { get; private set; }

		/// <summary>
		/// Сколько бонусов можно списать в заказе
		/// </summary>
		public int MaxBonusesToWriteOff { get; private set; }

		/// <summary>
		/// Сколько бонусов списано в расчёте
		/// </summary>
		public int BonusesWrittenOff { get; private set; }

		/// <summary>
		/// Сколько бонусов начислится за заказ
		/// </summary>
		public int BonusesToEarn { get; private set; }

		/// <summary>
		/// Списание бонусов по номерам строк заказа
		/// </summary>
		public IReadOnlyDictionary<int, int> BonusesByLineNumber { get; private set; }

		/// <summary>
		/// HTTP-код ответа Mindbox
		/// </summary>
		public int HttpStatusCode { get; private set; }

		/// <summary>
		/// Статус операции Mindbox
		/// </summary>
		public string Status { get; private set; }

		/// <summary>
		/// Статус обработки заказа Mindbox
		/// </summary>
		public string ProcessingStatus { get; private set; }

		/// <summary>
		/// Идентификатор ошибки Mindbox
		/// </summary>
		public string ErrorId { get; private set; }

		/// <summary>
		/// Описание ошибки для диагностики
		/// </summary>
		public string ErrorMessage { get; private set; }

		/// <summary>
		/// Категория ошибки Mindbox
		/// </summary>
		public MindboxSendOutcome? SendOutcome { get; private set; }

		/// <summary>
		/// Длительность вызова Mindbox
		/// </summary>
		public long DurationMs { get; private set; }

		/// <summary>
		/// Создаёт успешный результат расчёта
		/// </summary>
		/// <param name="balance">Доступно бонусов</param>
		/// <param name="maxBonusesToWriteOff">Сколько можно списать</param>
		/// <param name="bonusesWrittenOff">Сколько списано</param>
		/// <param name="bonusesToEarn">Сколько начислится</param>
		/// <param name="bonusesByLineNumber">Списание по строкам</param>
		/// <param name="httpStatusCode">HTTP-код ответа</param>
		/// <param name="status">Статус операции</param>
		/// <param name="processingStatus">Статус обработки заказа</param>
		/// <param name="errorId">Идентификатор ошибки</param>
		/// <param name="durationMs">Длительность вызова</param>
		/// <returns>Успешный результат</returns>
		public static MindboxAuthorizedCartResult Success(
			int? balance,
			int maxBonusesToWriteOff,
			int bonusesWrittenOff,
			int bonusesToEarn,
			IReadOnlyDictionary<int, int> bonusesByLineNumber,
			int httpStatusCode,
			string status,
			string processingStatus,
			string errorId,
			long durationMs)
		{
			return new MindboxAuthorizedCartResult
			{
				Outcome = MindboxAuthorizedCartOutcome.Success,
				Balance = balance,
				MaxBonusesToWriteOff = maxBonusesToWriteOff,
				BonusesWrittenOff = bonusesWrittenOff,
				BonusesToEarn = bonusesToEarn,
				BonusesByLineNumber = bonusesByLineNumber,
				HttpStatusCode = httpStatusCode,
				Status = status,
				ProcessingStatus = processingStatus,
				ErrorId = errorId,
				DurationMs = durationMs
			};
		}

		/// <summary>
		/// Создаёт результат недоступности расчёта
		/// </summary>
		/// <param name="errorMessage">Причина недоступности для диагностики</param>
		/// <param name="httpStatusCode">HTTP-код ответа</param>
		/// <param name="status">Статус операции</param>
		/// <param name="processingStatus">Статус обработки заказа</param>
		/// <param name="errorId">Идентификатор ошибки</param>
		/// <param name="sendOutcome">Категория ошибки Mindbox</param>
		/// <param name="durationMs">Длительность вызова</param>
		/// <returns>Результат недоступности</returns>
		public static MindboxAuthorizedCartResult Failure(
			string errorMessage,
			int httpStatusCode = 0,
			string status = null,
			string processingStatus = null,
			string errorId = null,
			MindboxSendOutcome? sendOutcome = null,
			long durationMs = 0)
		{
			return new MindboxAuthorizedCartResult
			{
				Outcome = MindboxAuthorizedCartOutcome.ServiceUnavailable,
				Balance = null,
				MaxBonusesToWriteOff = 0,
				BonusesWrittenOff = 0,
				BonusesToEarn = 0,
				BonusesByLineNumber = new Dictionary<int, int>(),
				HttpStatusCode = httpStatusCode,
				Status = status,
				ProcessingStatus = processingStatus,
				ErrorId = errorId,
				ErrorMessage = errorMessage,
				SendOutcome = sendOutcome,
				DurationMs = durationMs
			};
		}
	}
}
