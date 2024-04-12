using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "топливные карты",
		Nominative = "топливная карта")]
	[EntityPermission]
	[HistoryTrace]
	public class FuelCard : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _cardNumberLength = 16;

		private string _cardId;
		private string _cardNumber;
		private string _comment;
		private bool _isArchived;

		public virtual int Id { get; set; }

		[Display(Name = "Id топливной карты")]
		public virtual string CardId
		{
			get => _cardId;
			set => SetField(ref _cardId, value);
		}

		[Display(Name = "Номер топливной карты")]
		public virtual string CardNumber
		{
			get => _cardNumber;
			set => SetField(ref _cardNumber, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Архивная")]
		public virtual bool IsArchived
		{
			get => _isArchived;
			set => SetField(ref _isArchived, value);
		}

		public virtual string Title => $"Топливная карта №{CardNumber}";

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(CardId))
			{
				yield return new ValidationResult(
					"ID карты должен быть обязательно заполнен",
					new[] { nameof(CardId) });
			}

			if(string.IsNullOrWhiteSpace(CardNumber))
			{
				yield return new ValidationResult(
					"Номер карты должен быть обязательно заполнен",
					new[] { nameof(CardNumber) });
			}

			if(!string.IsNullOrWhiteSpace(CardNumber))
			{
				if(CardNumber.Length != _cardNumberLength)
				{
					yield return new ValidationResult(
						$"Номер карты должен содержать {_cardNumberLength} символов",
						new[] { nameof(CardNumber) });
				}

				if(CardNumber.All(char.IsDigit))
				{
					yield return new ValidationResult(
						$"Номер карты должен состоять только из цифр",
						new[] { nameof(CardNumber) });
				}
			}

			if(Comment?.Length > 500)
			{
				yield return new ValidationResult(
					$"Длина комментария не должна превышать 500 символов",
					new[] { nameof(Comment) });
			}

			if(!string.IsNullOrWhiteSpace(CardId) && !string.IsNullOrWhiteSpace(CardNumber))
			{
				using(var uow = validationContext.GetService<IUnitOfWork>())
				{
					var isCardIdDuplicate = uow.Session.Query<FuelCard>().Any(c => c.CardId == CardId);

					if(isCardIdDuplicate)
					{
						yield return new ValidationResult(
							$"Карта с указанным ID уже существует",
							new[] { nameof(CardId) });
					}

					var isCardNumberDuplicate = uow.Session.Query<FuelCard>().Any(c => c.CardNumber == CardNumber);

					if(isCardNumberDuplicate)
					{
						yield return new ValidationResult(
							$"Карта с указанным номером уже существует",
							new[] { nameof(CardNumber) });
					}

					if(IsArchived)
					{
						var carsUsingCard = uow.Session.Query<Car>().Where(c => c.FuelCard.Id == Id);

						if(carsUsingCard.Any())
						{
							yield return new ValidationResult(
								$"Нельзя добавить карту в архив, т.к. она используется в карточках авто: {string.Join(", ", carsUsingCard.Select(c => c.RegistrationNumber))}",
								new[] { nameof(CardNumber) });
						}
					}
				}
			}
		}
	}
}
