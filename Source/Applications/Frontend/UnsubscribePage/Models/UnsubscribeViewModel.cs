using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories;

namespace UnsubscribePage.Models
{
	/// <summary>
	/// ViewModel страницы отписки от email-рассылки.
	/// </summary>
	public class UnsubscribeViewModel : IValidatableObject
	{
		private IList<BulkEmailEventReason> _reasonsList;
		private GuidCounterpartyEmailNode _guidCounterpartyEmail;

		/// <summary>
		/// Идентификатор события отписки.
		/// </summary>
		public int EmailEventId { get; set; }

		/// <summary>
		/// Идентификатор выбранной причины отписки.
		/// </summary>
		public int SelectedReasonId { get; set; }

		/// <summary>
		/// Идентификатор причины "Другое".
		/// </summary>
		public int OtherReasonId { get; set; }

		/// <summary>
		/// Текст причины, введённой пользователем.
		/// </summary>
		public string OtherReason { get; set; }

		/// <summary>
		/// Сериализованный список причин отписки.
		/// Нужен для сохранения данных между GET и POST.
		/// </summary>
		public string ReasonsListSerialized { get; set; }

		/// <summary>
		/// Сериализованный GuidCounterpartyEmail.
		/// Нужен для сохранения данных между GET и POST.
		/// </summary>
		public string GuidCounterpartyEmailSerialized { get; set; }

		/// <summary>
		/// Список причин отписки.
		/// </summary>
		public IList<BulkEmailEventReason> ReasonsList =>
			_reasonsList ??=
				string.IsNullOrWhiteSpace(ReasonsListSerialized)
					? new List<BulkEmailEventReason>()
					: JsonSerializer.Deserialize<IList<BulkEmailEventReason>>(ReasonsListSerialized);

		/// <summary>
		/// Информация о контрагенте и типе email.
		/// </summary>
		public GuidCounterpartyEmailNode GuidCounterpartyEmail =>
			_guidCounterpartyEmail ??=
				string.IsNullOrWhiteSpace(GuidCounterpartyEmailSerialized)
					? null
					: JsonSerializer.Deserialize<GuidCounterpartyEmailNode>(GuidCounterpartyEmailSerialized);

		/// <summary>
		/// Выполняет валидацию данных формы.
		/// </summary>
		/// <param name="validationContext">Контекст валидации.</param>
		/// <returns>Коллекция ошибок валидации.</returns>
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var selectedReason = ReasonsList?
				.FirstOrDefault(x => x.Id == SelectedReasonId);

			var errors = new List<ValidationResult>();

			if(selectedReason == null)
			{
				errors.Add(new ValidationResult(
					"Выберите один из вариантов",
					new[] { nameof(SelectedReasonId) }));
			}

			if(selectedReason?.Id == OtherReasonId
				&& string.IsNullOrWhiteSpace(OtherReason))
			{
				errors.Add(new ValidationResult(
					"Введите текст в поле для другой причины",
					new[] { nameof(OtherReason) }));
			}

			return errors;
		}
	}
}
