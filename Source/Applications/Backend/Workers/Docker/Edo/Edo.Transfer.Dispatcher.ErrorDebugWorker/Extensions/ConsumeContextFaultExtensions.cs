using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.ErrorDebugWorker.Extensions
{
	/// <summary>
	/// Вспомогательные методы для консьюмеров, читающих сообщения из очередей с ошибками (fault-очередей MassTransit),
	/// чтобы отделять "целевые" ошибки, которые нужно обработать, от прочих, которые должны остаться нетронутыми.
	/// Предназначены для одноразовых консьюмеров, которые запускают вручную для разбора
	/// конкретной проблемы в очереди с ошибками и останавливают после того, как нужные сообщения обработаны
	/// </summary>
	/// <example>
	/// Типичное использование в консьюмере, разбирающем один конкретный тип ошибки:
	/// <code>
	/// public async Task Consume(ConsumeContext&lt;DocumentTaskCreatedEvent&gt; context)
	/// {
	///     try
	///     {
	///         if(!context.IsTargetFault("Autofac.Core.DependencyResolutionException", "ITrueMarkCodesPool trueMarkCodesPool"))
	///         {
	///				// Не наш тип ошибки, возвращаем обратно в очередь и пропускаем
	///             await context.RequeueBackAsync(_logger);
	///             return;
	///         }
	///
	///         await _documentEdoTaskHandler.HandleNew(context.Message.Id, context.CancellationToken);
	///     }
	///     catch(Exception ex)
	///     {
	///         _logger.LogError(ex, "Error processing event");
	///         throw;
	///     }
	/// }
	/// </code>
	/// </example>
	public static class ConsumeContextFaultExtensions
	{
		/// <summary>
		/// Проверяет, соответствует ли сообщение, попавшее в fault-очередь, искомой ошибке.
		/// Сравнение идёт по заголовкам MassTransit, которые он проставляет при публикации fault-события:
		/// <c>MT-Fault-ExceptionType</c> (имя типа исключения) и, опционально,
		/// <c>MT-Fault-Message</c> (текст сообщения исключения)
		/// </summary>
		/// <param name="context">
		/// Контекст обработки сообщения, из которого читаются fault-заголовки (например, MT-Fault-ExceptionType)
		/// </param>
		/// <param name="targetExceptionType">
		/// Полное имя типа исключения, которое нужно отобрать, например
		/// <c>"Autofac.Core.DependencyResolutionException"</c>.
		/// Сравнивается как вхождение подстроки в значение заголовка <c>MT-Fault-ExceptionType</c>
		/// </param>
		/// <param name="targetMessageFragment">
		/// Необязательный фрагмент текста сообщения исключения (заголовок <c>MT-Fault-Message</c>),
		/// который дополнительно должен встречаться в сообщении, чтобы уточнить отбор —
		/// например, <c>"ITrueMarkCodesPool"</c>, если нужно отделить конкретную причину ошибки резолвинга
		/// от других ошибок того же типа исключения.
		/// Если параметр не задан (<see langword="null"/>), проверка по тексту сообщения не выполняется,
		/// и достаточно совпадения только по <paramref name="targetExceptionType"/>
		/// </param>
		/// <returns>
		/// <see langword="true"/>, если заголовки fault-сообщения соответствуют искомой ошибке
		/// и сообщение нужно обработать текущим консьюмером;
		/// <see langword="false"/>, если это другая ошибка и сообщение нужно вернуть в очередь нетронутым
		/// </returns>
		public static bool IsTargetFault(
			this ConsumeContext context,
			string targetExceptionType,
			string targetMessageFragment = null)
		{
			// Проверяем, есть ли контекст получения и транспортные заголовки
			if(context.ReceiveContext?.TransportHeaders == null)
			{
				return false;
			}

			var transportHeaders = context.ReceiveContext.TransportHeaders;

			// Ищем тип исключения в транспортных заголовках RabbitMQ
			if(!transportHeaders.TryGetHeader("MT-Fault-ExceptionType", out var exceptionTypeObj)
				|| exceptionTypeObj is not string exceptionType
				|| !exceptionType.Contains(targetExceptionType, StringComparison.Ordinal))
			{
				return false;
			}

			// Если передан фрагмент сообщения, ищем его там же
			if(targetMessageFragment != null
				&& (!transportHeaders.TryGetHeader("MT-Fault-Message", out var faultMessageObj)
					|| faultMessageObj is not string faultMessage
					|| !faultMessage.Contains(targetMessageFragment, StringComparison.Ordinal)))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Пересылает сообщение обратно в ту же очередь, из которой оно было получено
		/// (адрес берётся из <see cref="ReceiveContext.InputAddress"/> текущего консьюмера),
		/// сохраняя все исходные заголовки без изменений.
		/// Используется, чтобы "чужие" ошибки (не относящиеся к целевому типу) оставались
		/// в очереди с ошибками для последующего разбора, пока текущий консьюмер разбирает
		/// только нужный ему тип ошибок.
		/// </summary>
		/// <param name="context">
		/// Контекст обработки сообщения, которое нужно вернуть в исходную очередь.
		/// Тип сообщения <typeparamref name="T"/> должен совпадать с типом события, на которое подписан консьюмер
		/// </param>
		/// <param name="logger">
		/// Логгер консьюмера, используется для информирования о том, что сообщение возвращено в очередь нетронутым
		/// </param>
		/// <typeparam name="T">Тип сообщения MassTransit, реализующий контракт события</typeparam>
		/// <returns>Задача, завершающаяся после отправки сообщения обратно в очередь</returns>
		public static async Task RequeueBackAsync<T>(this ConsumeContext<T> context, ILogger logger)
			where T : class
		{
			var endpoint = await context.GetSendEndpoint(context.ReceiveContext.InputAddress);

			await endpoint.Send(context.Message, sendContext =>
			{
				// Читаем заголовки напрямую из транспортного уровня RabbitMQ
				if(context.ReceiveContext?.TransportHeaders != null)
				{
					foreach(var header in context.ReceiveContext.TransportHeaders.GetAll())
					{
						// Переносим все транспортные заголовки (включая MT-Fault-*) в новое сообщение
						sendContext.Headers.Set(header.Key, header.Value);
					}
				}
			});

			logger.LogInformation(
				"Сообщение {MessageId} не соответствует целевой ошибке, возвращено в очередь",
				context.MessageId);
		}

	}
}
