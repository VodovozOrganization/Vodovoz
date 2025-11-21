using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods
{
	public abstract class NomenclatureOnlineCatalog : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private Guid? _externalId;
		
		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}
		
		[Display(Name = "Номер каталога в ИПЗ")]
		public virtual Guid? ExternalId
		{
			get => _externalId;
			set => SetField(ref _externalId, value);
		}
		
		public abstract GoodsOnlineParameterType Type { get; }
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Не заполнено наименование");
			}
			
			if(ExternalId is null)
			{
				yield return new ValidationResult(
					"Не заполнен или некорректно заполнен внешний номер каталога." +
					"Номер должен совпадать с шаблоном ХХХХХХХХ-ХХХХ-ХХХХ-ХХХХ-ХХХХХХХХХХХХ");
			}
		}
	}
}
