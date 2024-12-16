using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "версии районов",
		Nominative = "версия районов")]
	[EntityPermission]
	[HistoryTrace]
	public class DistrictsSet : PropertyChangedBase, IDomainObject, IValidatableObject, ICloneable
	{
		private string _name;
		private Employee _author;
		private DistrictsSetStatus _status;
		private DateTime _dateCreated;
		private DateTime? _dateActivated;
		private DateTime? _dateClosed;
		private string _comment;
		private IList<District> _districts = new List<District>();
		private decimal _onlineStoreOrderSumForFreeDelivery;
		private GenericObservableList<District> _observableDistricts;

		public const int NameMaxLength = 50;
		public virtual int Id { get; set; }

		[Display(Name = "Название версии районов")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Автор")]
		public virtual Employee Author
		{
			get => _author;
			set => SetField(ref _author, value);
		}

		[Display(Name = "Статус")]
		public virtual DistrictsSetStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Время создания")]
		public virtual DateTime DateCreated
		{
			get => _dateCreated;
			set => SetField(ref _dateCreated, value);
		}

		[Display(Name = "Время активации")]
		public virtual DateTime? DateActivated
		{
			get => _dateActivated;
			set => SetField(ref _dateActivated, value);
		}

		[Display(Name = "Время закрытия")]
		public virtual DateTime? DateClosed
		{
			get => _dateClosed;
			set => SetField(ref _dateClosed, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		public virtual IList<District> Districts
		{
			get => _districts;
			set => SetField(ref _districts, value);
		}

		[Display(Name = "Минимальная сумма заказа для бесплатной доставки")]
		public virtual decimal OnlineStoreOrderSumForFreeDelivery
		{
			get => _onlineStoreOrderSumForFreeDelivery;
			set => SetField(ref _onlineStoreOrderSumForFreeDelivery, value);
		}

		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<District> ObservableDistricts =>
			_observableDistricts ?? (_observableDistricts = new GenericObservableList<District>(Districts));

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Название версии должно быть обязательно заполнено",
					new[] { this.GetPropertyName(x => x.Name) }
				);
			}
			if(Name?.Length > NameMaxLength)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина названия версии ({Name.Length}/{NameMaxLength})",
					new[] { nameof(Name) }
				);
			}
			const int commentLength = 500;
			if(Comment?.Length > commentLength)
			{
				yield return new ValidationResult($"Слишком длинный комментарий. Максимальное число символов: {commentLength}",
					new[] { this.GetPropertyName(x => x.Comment) }
				);
			}
			if(Districts == null || !Districts.Any())
			{
				yield return new ValidationResult("В версии районов должен присутствовать хотя бы один район",
					new[] { this.GetPropertyName(x => x.Districts) }
				);
			}
			if(Districts != null)
			{
				foreach(District district in Districts)
				{
					foreach(var validationResult in district.Validate(validationContext))
					{
						yield return validationResult;
					}
				}
			}
		}

		public virtual object Clone()
		{
			var newDistrictsSet = new DistrictsSet
			{
				Name = Name,
				Districts = new List<District>()
			};

			foreach(var district in Districts)
			{
				var newDistrict = (District)district.Clone();
				newDistrict.DistrictsSet = newDistrictsSet;
				newDistrict.CopyOf = district;
				newDistrictsSet.Districts.Add(newDistrict);
				district.DistrictCopyItems.Add(new DistrictCopyItem { District = district, CopiedToDistrict = newDistrict });
			}

			return newDistrictsSet;
		}
	}

	public enum DistrictsSetStatus
	{
		[Display(Name = "Черновик")]
		Draft,
		[Display(Name = "Активна")]
		Active,
		[Display(Name = "Закрыта")]
		Closed
	}
}
