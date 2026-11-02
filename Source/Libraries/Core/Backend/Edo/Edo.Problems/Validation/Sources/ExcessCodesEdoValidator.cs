using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	/// <summary>
	/// Проверяет общее количество кодов перед подготовкой УПД или чека.
	/// </summary>
	public class ExcessCodesEdoValidator : OrderEdoValidatorBase
	{
		/// <inheritdoc/>
		[Display(Name = "Превышение кодов в заказе")]
		public override string Name => "Order.ExcessCodes";

		/// <inheritdoc/>
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;

		/// <inheritdoc/>
		public override string Message => "Количество кодов превышает необходимое количество кодов в заказе";

		/// <inheritdoc/>
		public override string Description => "Проверяет общее количество кодов для маркируемых товаров заказа";

		/// <inheritdoc/>
		public override string Recommendation => "Проверьте коды документа и удалите лишние перед повторной обработкой";

		/// <inheritdoc/>
		public override bool IsApplicable(EdoTask edoTask)
		{
			if(edoTask.Status == EdoTaskStatus.Completed
				|| edoTask.Status == EdoTaskStatus.Cancelled
				|| edoTask.Status == EdoTaskStatus.InCancellation)
			{
				return false;
			}

			return edoTask is DocumentEdoTask documentTask
				&& documentTask.DocumentType == EdoDocumentType.UPD
				&& documentTask.Stage == DocumentEdoTaskStage.New
				|| edoTask is ReceiptEdoTask receiptTask && receiptTask.ReceiptStatus == EdoReceiptStatus.New;
		}

		/// <inheritdoc/>
		public override Task<EdoValidationResult> ValidateAsync(
			EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			var task = (OrderEdoTask)edoTask;
			if(GetCodesCount(task) <= GetRequiredCount(task))
			{
				return Task.FromResult(EdoValidationResult.Valid(this));
			}

			var messageFactory = serviceProvider.GetRequiredService<IEdoNotificationMessageFactory>();
			return Task.FromResult(EdoValidationResult.InvalidWithNotification(this, CreateNotification(task, messageFactory)));
		}

		/// <inheritdoc/>
		public override string GetTemplatedMessage(EdoTask edoTask)
		{
			var task = (OrderEdoTask)edoTask;
			return $"В заказе №{task.FormalEdoRequest.Order.Id} кодов: {GetCodesCount(task)}, "
				+ $"необходимо: {GetRequiredCount(task)}. Распределение кодов остановлено.";
		}

		private EdoNotificationMessage CreateNotification(EdoTask task, IEdoNotificationMessageFactory messageFactory)
		{
			var orderTask = (OrderEdoTask)task;
			return messageFactory.Create(
				EdoNotificationType.ExcessCodes,
				("OrderId", orderTask.FormalEdoRequest.Order.Id.ToString(CultureInfo.InvariantCulture)),
				("EdoTaskId", orderTask.Id.ToString(CultureInfo.InvariantCulture)),
				("ProblemMessage", GetTemplatedMessage(task)),
				("Recommendation", Recommendation));
		}

		private static int GetCodesCount(OrderEdoTask task) => task.Items.Count(x =>
			x.ProductCode.SourceCode != null || x.ProductCode.ResultCode != null);

		private static decimal GetRequiredCount(OrderEdoTask task) => task.FormalEdoRequest.Order.OrderItems
			.Where(x => x.Nomenclature.IsAccountableInTrueMark)
			.Sum(x => x.CurrentCount);
	}
}
