using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Rules.Edo;
using Vodovoz.Settings;

namespace Edo.Problems.Validation.Sources
{
	public class ClosedTaxPeriodEdoValidator : OrderEdoValidatorBase, IEdoTaskValidator
	{
		[Display(Name = "Закрытый бухгалтерский период")]
		public override string Name
		{
			get => "Order.ClosedTaxPeriod";
		}

		public override EdoProblemImportance Importance
		{
			get => EdoProblemImportance.Problem;
		}

		public override string Message
		{
			get => "Документ в закрытом бухгалтерском периоде. Для переотправки обратитесь в бухгалтерию";
		}

		public override string Description
		{
			get => "Дата доставки заказа находится в закрытом бухгалтерском периоде. "
				+ "Период считается закрытым по датам закрытия из настроек бухгалтерского учёта. "
				+ "Отправка таких документов заблокирована.";
		}

		public override string Recommendation
		{
			get => "Обратитесь в бухгалтерию. Повторная отправка возможна только пользователем с правом "
				+ "«Отправка ЭДО документов за прошлые бухгалтерские периоды» через кнопку «Переотправить» "
				+ "в диалоге заказа или через кнопку «Отправить все неотправленные документы» "
				+ "во вкладке «Документы по ЭДО» в диалоге контрагента.";
		}

		public override string GetTemplatedMessage(EdoTask edoTask)
		{
			return Message;
		}

		public override Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			var closedPeriodRule = serviceProvider.GetRequiredService<CanProcessOrSendEdoByClosingAccountingDate>();

			var accountingDate = GetAccountingDate(edoTask);

			if(!accountingDate.HasValue)
			{
				return Task.FromResult(EdoValidationResult.Valid(this));
			}

			try
			{
				var invalid = !closedPeriodRule.Check(accountingDate);
				return Task.FromResult(invalid ? EdoValidationResult.Invalid(this) : EdoValidationResult.Valid(this));
			}
			catch(System.Exception ex) when (ex is InvalidOperationException || ex is SettingException)
			{
				return Task.FromResult(EdoValidationResult.Valid(this));
			}
		}

		private DateTime? GetAccountingDate(EdoTask edoTask)
		{
			var orderEdoRequest = GetEdoRequest(edoTask);

			if(orderEdoRequest?.Order?.DeliveryDate != null)
			{
				return orderEdoRequest.Order.DeliveryDate;
			}

			if(edoTask is ReceiptEdoTask receiptTask && receiptTask.FiscalDocuments.Any())
			{
				return receiptTask.FiscalDocuments.Max(x => x.FiscalTime ?? x.CheckoutTime);
			}

			return edoTask.CreationTime;
		}
	}
}
