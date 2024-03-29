﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "причина отписки от массовой рассылки",
		NominativePlural = "причины отписки от массовой рассылкив"
		)]
	[EntityPermission]
	[HistoryTrace]
	public class BulkEmailEventReason : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private string _name;
		private bool _isArchive;
		private bool _hideForUnsubscribePage;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Название ")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Архивный?")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}

		[Display(Name = "Скрывать на странице отписки?")]
		public virtual bool HideForUnsubscribePage
		{
			get => _hideForUnsubscribePage;
			set => SetField(ref _hideForUnsubscribePage, value);
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Имя должно быть заполнено.",
					new[] { nameof(Name) });
			}

			if(Name?.Length > 255)
			{
				yield return new ValidationResult($"Превышена максимально допустимая длина имени ({Name.Length}/255).",
					new[] { nameof(Name) });
			}
		}

		#endregion
	}
}
