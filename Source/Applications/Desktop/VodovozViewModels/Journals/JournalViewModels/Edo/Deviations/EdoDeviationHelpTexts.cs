using Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo.Deviations
{
	/// <summary>
	/// Тексты справки по критериям отклонений документооборота ЭДО
	/// </summary>
	public static class EdoDeviationHelpTexts
	{
		/// <summary>
		/// Заголовок окна справки по критериям отклонений
		/// </summary>
		public const string HelpTitle = "Критерии отклонений документооборота ЭДО";

		/// <summary>
		/// Построение текста справки по критериям отклонений
		/// </summary>
		public static string BuildHelpMessage()
		{
			var message = new StringBuilder();

			message
				.AppendLine("Отклонение заводится, когда заявка или задача ЭДО задерживается на своей стадии дольше таймаута,"
					+ " заданного для этого типа отклонения в справочнике источников отклонений.")
				.AppendLine()
				.AppendLine("Общие условия, без которых отклонение по задаче не заводится:")
				.AppendLine("• задача не завершена и не отменена (только проверки результата ГИС МТ работают и по завершенным);")
				.AppendLine($"• по задаче нет активной проблемы и сама она не в статусе \"{EdoTaskStatus.Problem.GetEnumDisplayName()}\";")
				.AppendLine("• по задаче нет другого активного отклонения;")
				.AppendLine("• не сработал ни один тип отклонения, идущий раньше по ходу документооборота.")
				.AppendLine()
				.AppendLine("Критерии по типам отклонений:");

			foreach(EdoDeviationType deviationType in Enum.GetValues(typeof(EdoDeviationType)))
			{
				if(!_deviationCriteria.TryGetValue(deviationType, out var criteria))
				{
					continue;
				}

				message.AppendLine($"• {deviationType.GetEnumDisplayName()} - {criteria}");
			}

			return message.ToString();
		}

		/// <summary>
		/// Условие, при котором мониторинг заводит отклонение каждого типа
		/// </summary>
		private static readonly IDictionary<EdoDeviationType, string> _deviationCriteria =
			new Dictionary<EdoDeviationType, string>
			{
				{
					EdoDeviationType.TaskNotCreated,
					"заявка создана, задача ЭДО по ней так и не создана"
				},
				{
					EdoDeviationType.TaskNotStarted,
					$"задача создана и остается в статусе \"{EdoTaskStatus.New.GetEnumDisplayName()}\": обработчик к ней не приступал"
				},
				{
					EdoDeviationType.TransferNotStarted,
					"задача на стадии трансфера, но по заявкам на перенос кодов перенос не запущен"
				},
				{
					EdoDeviationType.DocumentNotSentToProvider,
					"исходящий документ создан, а документооборот у провайдера ЭДО не заведен"
				},
				{
					EdoDeviationType.ProviderNoResponse,
					"документооборот заведен, но провайдер ЭДО не прислал по нему ни одного действия"
				},
				{
					EdoDeviationType.ClientNotAcceptedDocflow,
					$"последнее действие документооборота - \"{EdoDocFlowStatus.Sent.GetEnumDisplayName()}\" или \"{EdoDocFlowStatus.InProgress.GetEnumDisplayName()}\":"
					+ " документ у клиента, и клиент его не завершает"
				},
				{
					EdoDeviationType.CancellationNotCompleted,
					$"последнее действие документооборота - \"{EdoDocFlowStatus.WaitingForCancellation.GetEnumDisplayName()}\""
				},
				{
					EdoDeviationType.GisMtResultMissing,
					"документооборот завершен и не аннулирован, но результат обработки кодов в ГИС МТ не пришел."
					+ " Проверяется и по завершенным задачам"
				},
				{
					EdoDeviationType.GisMtRejected,
					"последний статус ГИС МТ по документообороту - отказной: коды не приняты."
					+ " Проверяется и по завершенным задачам"
				},
				{
					EdoDeviationType.ReceiptNotFiscalized,
					"чек передан в отправку, но фискальный документ не создан либо не фискализирован."
					+ " Ожидание ответа кассы отклонением не считается"
				},
				{
					EdoDeviationType.TaskStalled,
					"задача не завершена, при этом ни один частный тип отклонения к ней не подходит:"
					+ " причина задержки не определена"
				},
				{
					EdoDeviationType.TransferWaitingRequestsTooLong,
					$"задача трансфера остается на стадии \"{EdoTransferTaskStage.WaitingRequests.GetEnumDisplayName()}\":"
					+ " досылка залежавшихся трансферов не сработала"
				},
				{
					EdoDeviationType.TransferDocumentNotCreated,
					"задача трансфера на стадии подготовки или отправки, документ на перенос кодов не создан"
				},
				{
					EdoDeviationType.TransferDocumentNotSentToProvider,
					"документ трансфера создан, а документооборот у провайдера ЭДО не заведен"
				},
				{
					EdoDeviationType.TransferProviderNoResponse,
					"документооборот трансфера заведен, ответа провайдера ЭДО по нему нет"
				},
				{
					EdoDeviationType.TransferGisMtResultMissing,
					"документооборот трансфера завершен, но результат обработки кодов в ГИС МТ не пришел."
					+ " Проверяется и по завершенным задачам"
				},
				{
					EdoDeviationType.TransferGisMtRejected,
					"ГИС МТ вернула отказной статус по документообороту трансфера."
					+ " Проверяется и по завершенным задачам"
				},
				{
					EdoDeviationType.TransferCodesNotMoved,
					"документооборот трансфера завершен, но коды не сменили владельца в ГИС МТ:"
					+ " по задаче висит незакрытая проблема ожидания перемещения"
				},
				{
					EdoDeviationType.TransferTooLong,
					"перенос кодов запущен и не завершается."
					+ " Ожидание перемещения кодов в ГИС МТ сюда не относится"
				},
				{
					EdoDeviationType.TransferStalled,
					"задача трансфера не завершена, при этом ни один частный тип отклонения к ней не подходит"
				}
			};
	}
}
