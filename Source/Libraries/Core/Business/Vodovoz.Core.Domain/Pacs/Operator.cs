using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;

namespace Vodovoz.Core.Domain.Pacs
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "операторы",
		Nominative = "оператор")]
	[EntityPermission]
	[HistoryTrace]
	public class Operator : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _id;
		private WorkShift _workShift;
		private bool _pacsEnabled;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Рабочая смена")]
		public virtual WorkShift WorkShift
		{
			get => _workShift;
			set => SetField(ref _workShift, value);
		}

		[Display(Name = "Включен в СКУД")]
		public virtual bool PacsEnabled
		{
			get => _pacsEnabled;
			set => SetField(ref _pacsEnabled, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var unitOfWorkFactory = validationContext.GetService<IUnitOfWorkFactory>();

			var operatorRepository = validationContext.GetService<IGenericRepository<Operator>>();

			using(var unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Валидация"))
			{
				if(validationContext.Items.TryGetValue("isNew", out var isNew)
					&& isNew is bool isNewValue
					&& isNewValue
					&& operatorRepository.GetValue(unitOfWork, o => o.Id, o => o.Id == Id).Any())
				{
					yield return new ValidationResult("Оператор уже существует", new[] { nameof(Id) });
				}

				if(Id == 0)
				{
					yield return new ValidationResult("Необходимо выбрать оператора", new[] { nameof(Operator) });
				}

				if(WorkShift == null)
				{
					yield return new ValidationResult("Необходимо выбрать смену", new[] { nameof(WorkShift) });
				}
			}
		}
	}
}
