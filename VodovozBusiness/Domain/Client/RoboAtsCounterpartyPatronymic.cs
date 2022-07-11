using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "отчества контрагентов",
		Nominative = "отчество контрагента")]
	[EntityPermission]
	[HistoryTrace]
	public class RoboAtsCounterpartyPatronymic : PropertyChangedBase, IDomainObject, IValidatableObject, IRoboatsEntity
	{
		private string _patronymic;
		private string _accent;
		private Guid? _fileId;
		private string _roboatsAudiofile;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Отчество")]
		public virtual string Patronymic
		{
			get => _patronymic;
			set => SetField(ref _patronymic, value);
		}

		[Display(Name = "Ударение")]
		public virtual string Accent
		{
			get => _accent;
			set => SetField(ref _accent, value);
		}


		[Display(Name = "Имя аудиозаписи Roboats")]
		public virtual string RoboatsAudiofile
		{
			get => _roboatsAudiofile;
			set => SetField(ref _roboatsAudiofile, value);
		}
		public virtual string NewRoboatsAudiofile { get; set; }

		[Display(Name = "Идентификатор файла")]
		public virtual Guid? FileId
		{
			get => _fileId;
			set => SetField(ref _fileId, value);
		}

		#endregion

		public virtual int? RoboatsId => Id;
		public virtual RoboatsEntityType RoboatsEntityType => RoboatsEntityType.CounterpartyPatronymic;

		public virtual string Title => Patronymic;

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Patronymic))
			{
				yield return new ValidationResult("Отчество должно быть заполнено.",
					new[] { nameof(Patronymic) });
			}

			if(string.IsNullOrEmpty(Accent))
			{
				yield return new ValidationResult("Ударение должно быть заполнено.",
					new[] { nameof(Accent) });
			}

			if(Patronymic?.Length > 20)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина отчества ({Patronymic.Length}/20).",
					new[] { nameof(Patronymic) });
			}

			if(Accent?.Length > 20)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина ударения ({Accent.Length}/20).",
					new[] { nameof(Accent) });
			}

			if(string.IsNullOrWhiteSpace(NewRoboatsAudiofile) && string.IsNullOrWhiteSpace(RoboatsAudiofile))
			{
				yield return new ValidationResult("Необходимо выбрать аудиофайл", new[] { nameof(NewRoboatsAudiofile) });
			}
		}

		#endregion
	}
}
