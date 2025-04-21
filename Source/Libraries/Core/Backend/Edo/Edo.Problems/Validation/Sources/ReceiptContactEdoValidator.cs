﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Edo.Common;
using Microsoft.Extensions.DependencyInjection;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public class ReceiptContactEdoValidator : EdoTaskProblemValidatorSource, IEdoTaskValidator
	{
		public override string Name
		{
			get => "Receipt.ContactValid";
		}

		public override EdoProblemImportance Importance
		{
			get => EdoProblemImportance.Problem;
		}

		public override string Message
		{
			get => "На стадии подготовки данных в задаче на отправку обнаружен невалидный контакт";
		}

		public override string Description
		{
			get => "Проверяет, что контакт валидный";
		}

		public override string Recommendation
		{
			get => "Исправить на валидный контакт";
		}

		public string GetTemplateMessage(EdoTask edoTask)
		{
			return $"Для задачи  №{edoTask.Id} в заказе невалидный контакт";
		}

		public override bool IsApplicable(EdoTask edoTask)
		{
			return edoTask is ReceiptEdoTask;
		}

		public override async Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var edoOrderContactProvider = serviceProvider.GetRequiredService<IEdoOrderContactProvider>();

			if(!(edoTask is ReceiptEdoTask receiptEdoTask))
			{
				return EdoValidationResult.Invalid(this);
			}

			var contact = edoOrderContactProvider.GetContact(receiptEdoTask.OrderEdoRequest.Order);

			var validationEmail = new EmailEntity { Address = contact };

			if(validationEmail.IsValidEmail)
			{
				return EdoValidationResult.Valid(this);
			}

			var validationPhone = new PhoneEntity { Number = contact };

			if(validationPhone.IsValidPhoneNumber)
			{
				return EdoValidationResult.Valid(this);
			}

			return EdoValidationResult.Invalid(this);
		}
	}
}
