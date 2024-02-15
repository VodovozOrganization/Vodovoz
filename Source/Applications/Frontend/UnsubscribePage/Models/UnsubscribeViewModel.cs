﻿using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;
using Vodovoz.Parameters;

namespace UnsubscribePage.Models
{
	public class UnsubscribeViewModel : IValidatableObject
	{
		private IList<BulkEmailEventReason> _reasonsList;

		public UnsubscribeViewModel() { }

		public UnsubscribeViewModel(Guid guid, IEmailRepository emailRepository, IEmailParametersProvider emailParametersProvider)
		{
			Initialize(guid, emailRepository, emailParametersProvider);
		}

		private void Initialize(Guid guid, IEmailRepository emailRepository, IEmailParametersProvider emailParametersProvider)
		{
			OtherReasonId = emailParametersProvider.BulkEmailEventOtherReasonId;
			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Инициализация страницы отписки"))
			{
				CounterpartyId = emailRepository.GetCounterpartyIdByEmailGuidForUnsubscribing(unitOfWork, guid);
				_reasonsList = emailRepository.GetUnsubscribingReasons(unitOfWork, emailParametersProvider, isForUnsubscribePage: true);
			}
			ReasonsListSerialized = JsonSerializer.Serialize<IList<BulkEmailEventReason>>(_reasonsList);
		}

		public int CounterpartyId { get; set; }
		public int EmailEventId { get; set; }
		public string ReasonsListSerialized { get; set; }
		public int SelectedReasonId { get; set; }
		public int OtherReasonId { get; set; }
		public string OtherReason { get; set; }
		public IList<BulkEmailEventReason> ReasonsList =>
			_reasonsList ??= ReasonsListSerialized == null ? null : JsonSerializer.Deserialize<IList<BulkEmailEventReason>>(ReasonsListSerialized);

		public void SaveUnsubscribe(BulkEmailEventReason reason = null)
		{
			UnsubscribingBulkEmailEvent unsubscribingEvent = new UnsubscribingBulkEmailEvent
			{
				Id = EmailEventId,
				Reason = reason,
				ReasonDetail = reason?.Id == OtherReasonId ? OtherReason : null,
				Counterparty = new Counterparty
				{
					Id = CounterpartyId
				}
			};

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Отписка от массовой рассылки"))
			{
				unitOfWork.Save(unsubscribingEvent);
				unitOfWork.Commit();
			}

			EmailEventId = unsubscribingEvent.Id;
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var selectedReason = ReasonsList?.FirstOrDefault(x => x.Id == SelectedReasonId);

			List<ValidationResult> errors = new List<ValidationResult>();

			if(selectedReason != null && selectedReason.Id == OtherReasonId && string.IsNullOrWhiteSpace(OtherReason))
			{
				errors.Add(new ValidationResult("Введите текст в поле для другой причины\"", new[] { nameof(OtherReason) }));
			}

			if(selectedReason == null)
			{
				errors.Add(new ValidationResult("Выберите один из вариантов", new[] { nameof(SelectedReasonId) }));
			}

			return errors;
		}
	}
}
