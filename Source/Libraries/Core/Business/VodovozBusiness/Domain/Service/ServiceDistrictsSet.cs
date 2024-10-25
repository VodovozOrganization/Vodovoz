using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace VodovozBusiness.Domain.Service
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "версии районов сервисных работ",
		Nominative = "версия районов сервисных работ")]
	[EntityPermission]
	[HistoryTrace]
	public class ServiceDistrictsSet : PropertyChangedBase, IDomainObject, IValidatableObject, ICloneable
	{
		private string _name;
		private Employee _author;
		private ServiceDistrictsSetStatus _status;
		private DateTime _dateCreated;
		private DateTime? _dateActivated;
		private DateTime? _dateClosed;
		private string _comment;
		private IObservableList<ServiceDistrict> _districts = new ObservableList<ServiceDistrict>();

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
		public virtual ServiceDistrictsSetStatus Status
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

		public virtual IObservableList<ServiceDistrict> ServiceDistricts
		{
			get => _districts;
			set => SetField(ref _districts, value);
		}

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
			if(ServiceDistricts == null || !ServiceDistricts.Any())
			{
				yield return new ValidationResult("В версии районов должен присутствовать хотя бы один район",
					new[] { this.GetPropertyName(x => x.ServiceDistricts) }
				);
			}
			if(ServiceDistricts != null)
			{
				foreach(ServiceDistrict district in ServiceDistricts)
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
			var newServiceDistrictsSet = new ServiceDistrictsSet
			{
				Name = Name,
				ServiceDistricts = new ObservableList<ServiceDistrict>()
			};

			foreach(var serviceDistrict in ServiceDistricts)
			{
				var newServiceDistrict = (ServiceDistrict)serviceDistrict.Clone();
				newServiceDistrict.ServiceDistrictsSet = newServiceDistrictsSet;
				newServiceDistrict.CopyOf = serviceDistrict;
				newServiceDistrictsSet.ServiceDistricts.Add(newServiceDistrict);
				serviceDistrict.ServiceDistrictCopyItems.Add(new ServiceDistrictCopyItem { ServiceDistrict = serviceDistrict, CopiedToServiceDistrict = newServiceDistrict });
			}

			return newServiceDistrictsSet;
		}
	}
}
