using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "имена контрагентов",
		Nominative = "имя контрагента")]
	[EntityPermission]
	[HistoryTrace]
	public class RoboAtsCounterpartyName : PropertyChangedBase, IDomainObject, IValidatableObject, IRoboatsEntity
	{
		private string _name;
		private string _accent;
		private string _roboatsAudiofile;
		private Guid? _fileId;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Имя ")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
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
		public virtual RoboatsEntityType RoboatsEntityType => RoboatsEntityType.CounterpartyName;

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Имя должно быть заполнено.",
					new[] { nameof(Name) });
			}

			if(string.IsNullOrEmpty(Accent))
			{
				yield return new ValidationResult("Ударение должно быть заполнено.",
					new[] { nameof(Accent) });
			}

			if(Name?.Length > 20)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина имени ({Name.Length}/20).",
					new[] { nameof(Name) });
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
