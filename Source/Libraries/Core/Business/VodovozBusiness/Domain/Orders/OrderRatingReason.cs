using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Причины оценок заказов",
		Nominative = "Причина оценки заказа",
		Prepositional = "Причине оценки заказа",
		PrepositionalPlural = "Причинах оценок заказов"
	)]
	[EntityPermission]
	[HistoryTrace]
	public class OrderRatingReason : PropertyChangedBase, INamedDomainObject, IValidatableObject
	{
		private const int _nameMaxLength = 150;
		private string _name;
		private bool _isArchive;
		private bool _isForOneStarRating;
		private bool _isForTwoStarRating;
		private bool _isForThreeStarRating;
		private bool _isForFourStarRating;
		private bool _isForFiveStarRating;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "В архиве?")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
		
		[Display(Name = "Доступна для оценки в 1 звезду")]
		public virtual bool IsForOneStarRating
		{
			get => _isForOneStarRating;
			set => SetField(ref _isForOneStarRating, value);
		}
		
		[Display(Name = "Доступна для оценки в 2 звезды")]
		public virtual bool IsForTwoStarRating
		{
			get => _isForTwoStarRating;
			set => SetField(ref _isForTwoStarRating, value);
		}
		
		[Display(Name = "Доступна для оценки в 3 звезды")]
		public virtual bool IsForThreeStarRating
		{
			get => _isForThreeStarRating;
			set => SetField(ref _isForThreeStarRating, value);
		}
		
		[Display(Name = "Доступна для оценки в 4 звезды")]
		public virtual bool IsForFourStarRating
		{
			get => _isForFourStarRating;
			set => SetField(ref _isForFourStarRating, value);
		}
		
		[Display(Name = "Доступна для оценки в 5 звезд")]
		public virtual bool IsForFiveStarRating
		{
			get => _isForFiveStarRating;
			set => SetField(ref _isForFiveStarRating, value);
		}

		public virtual IEnumerable<int> GetRatingsArray()
		{
			var i = 0;
			var ratings = new List<int>();
			var ratingValues =
				typeof(OrderRatingReason)
					.GetProperties()
					.Where(x => x.Name.EndsWith("Rating"))
					.Select(x => (bool)x.GetValue(this));

			foreach(var ratingValue in ratingValues)
			{
				i++;
				if(ratingValue)
				{
					ratings.Add(i);
				}
			}

			return ratings;
		}
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название причины должно быть заполнено");
			}
			
			if(!string.IsNullOrWhiteSpace(Name) && Name.Length > _nameMaxLength)
			{
				yield return new ValidationResult($"Длина названия причины превышена на {_nameMaxLength - Name.Length}");
			}

			if(!IsForOneStarRating && !IsForTwoStarRating && !IsForThreeStarRating && !IsForFourStarRating && !IsForFiveStarRating)
			{
				yield return new ValidationResult("Нельзя создать причину оценки без выбора значения оценки");
			}
		}

		public override string ToString()
		{
			var entityName =
				typeof(OrderRatingReason)
					.GetCustomAttribute<AppellativeAttribute>(true)
					.Nominative;
			
			return Id > 0 ? $"{entityName} №{Id}" : $"Новая {entityName.ToLower()}";
		}
	}
}
