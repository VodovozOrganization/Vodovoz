using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.EntityRepositories.Fuel;

namespace Vodovoz.Domain.Fuel
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "топливные карты",
		Nominative = "топливная карта",
		Genitive = "топливной карты",
		GenitivePlural = "топливных карт")]
	[EntityPermission]
	[HistoryTrace]
	public class FuelCard : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private const int _cardNumberLength = 16;

		private string _cardId;
		private string _cardNumber;
		private bool _isArchived;

		public virtual int Id { get; set; }

		[Display(Name = "Id топливной карты")]
		public virtual string CardId
		{
			get => _cardId;
			set => SetField(ref _cardId, value);
		}

		[Display(Name = "Номер топливной карты")]
		[PropertyChangedAlso(nameof(IsCardNumberValid))]
		public virtual string CardNumber
		{
			get => _cardNumber;
			set => SetField(ref _cardNumber, value);
		}

		[Display(Name = "Архивная")]
		public virtual bool IsArchived
		{
			get => _isArchived;
			set => SetField(ref _isArchived, value);
		}

		public virtual string Title => $"{CardNumber}";

		public virtual bool IsCardNumberValid =>
			CardNumber?.Length == 16
			&& CardNumber.All(char.IsDigit);

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(CardId))
			{
				yield return new ValidationResult(
					"ID карты должен быть обязательно заполнен",
					new[] { nameof(CardId) });
			}

			if(!IsCardNumberValid)
			{
				yield return new ValidationResult(
					$"Номер карты должен содержать {_cardNumberLength} цифр",
					new[] { nameof(CardNumber) });
			}

			if(!string.IsNullOrWhiteSpace(CardId) && !string.IsNullOrWhiteSpace(CardNumber))
			{
				var unitOfWorkFactory = validationContext.GetService<IUnitOfWorkFactory>();
				var fuelRepository = validationContext.GetService<IFuelRepository>();

				using(var uow = unitOfWorkFactory.CreateWithoutRoot("Валидация при сохранении топливной карты"))
				{
					var isCardIdDuplicate =
						fuelRepository.GetFuelCardsByCardId(uow, CardId).Any(c => c.Id != Id);

					if(isCardIdDuplicate)
					{
						yield return new ValidationResult(
							$"Карта с указанным ID уже существует",
							new[] { nameof(CardId) });
					}

					var isCardNumberDuplicate =
						fuelRepository.GetFuelCardsByCardNumber(uow, CardNumber).Any(c => c.Id != Id);

					if(isCardNumberDuplicate)
					{
						yield return new ValidationResult(
							$"Карта с указанным номером уже существует",
							new[] { nameof(CardNumber) });
					}

					if(IsArchived && Id > 0)
					{
						var acitveFuelCardVersionsHavingCard =
							fuelRepository.GetActiveVersionsOnDateHavingFuelCard(uow, DateTime.Now, Id);

						if(acitveFuelCardVersionsHavingCard.Any())
						{
							yield return new ValidationResult(
								$"Нельзя добавить карту в архив, т.к. она используется в карточках авто: {string.Join(", ", acitveFuelCardVersionsHavingCard.Select(v => v.Car.RegistrationNumber))}",
								new[] { nameof(CardNumber) });
						}
					}
				}
			}
		}
	}
}
