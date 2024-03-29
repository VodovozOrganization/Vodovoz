﻿using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Roboats
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "улицы для Roboats",
		Nominative = "улица для Roboats")]
	[EntityPermission]
	[HistoryTrace]
	public class RoboatsStreet : PropertyChangedBase, IDomainObject, IValidatableObject, IRoboatsEntity
	{
		private string _name;
		private string _type;
		private string _audioFilename;
		private Guid? _fileId;

		public virtual int Id { get; set; }

		[Display(Name = "Название улицы")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Тип улицы")]
		public virtual string Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		[Display(Name = "Имя аудиозаписи Roboats")]
		public virtual string RoboatsAudiofile
		{
			get => _audioFilename;
			set => SetField(ref _audioFilename, value);
		}
		public virtual string NewRoboatsAudiofile { get; set; }

		[Display(Name = "Идентификатор файла")]
		public virtual Guid? FileId
		{
			get => _fileId;
			set => SetField(ref _fileId, value);
		}

		public virtual int? RoboatsId => Id;
		public virtual RoboatsEntityType RoboatsEntityType => RoboatsEntityType.Streets;

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrWhiteSpace(Type))
			{
				yield return new ValidationResult("Необходимо заполнить тип улицы", new[] { nameof(Type) });
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Необходимо заполнить название улицы", new[] { nameof(Name) });
			}

			if(string.IsNullOrWhiteSpace(NewRoboatsAudiofile) && string.IsNullOrWhiteSpace(RoboatsAudiofile))
			{
				yield return new ValidationResult("Необходимо выбрать аудиофайл", new[] { nameof(NewRoboatsAudiofile) });
			}
		}

		#endregion
	}
}
