using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "типы воды для Roboats",
		Nominative = "тип воды для Roboats")]
	[EntityPermission]
	[HistoryTrace]
	public class RoboatsWaterType : PropertyChangedBase, IDomainObject, IValidatableObject, IRoboatsEntity
	{
		private string _audioFilename;
		private Nomenclature _nomenclature;
		private Guid? _fileId;

		public virtual int Id { get; set; }

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Аудиофайл")]
		public virtual string RoboatsAudiofile
		{
			get => _audioFilename;
			set => SetField(ref _audioFilename, value);
		}

		[Display(Name = "Идентификатор файла")]
		public virtual Guid? FileId
		{
			get => _fileId;
			set => SetField(ref _fileId, value);
		}

		public virtual int? RoboatsId => Id;
		public virtual RoboatsEntityType RoboatsEntityType => RoboatsEntityType.WaterTypes;

		public virtual string NewRoboatsAudiofile { get; set; }

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Nomenclature == null)
			{
				yield return new ValidationResult("Необходимо выбрать номенклатуру", new [] { nameof(Nomenclature) } );
			}

			if(string.IsNullOrWhiteSpace(NewRoboatsAudiofile) && string.IsNullOrWhiteSpace(RoboatsAudiofile))
			{
				yield return new ValidationResult("Необходимо выбрать аудиофайл", new[] { nameof(NewRoboatsAudiofile) });
			}
		}

		#endregion
	}
}
