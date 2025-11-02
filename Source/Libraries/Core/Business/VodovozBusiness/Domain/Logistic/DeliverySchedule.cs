using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Project.Services;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.Domain.Logistic
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "графики доставки",
		Nominative = "график доставки")]
	[EntityPermission]
	[HistoryTrace]
	public class DeliverySchedule: DeliveryScheduleEntity, IValidatableObject, IRoboatsEntity
	{
		public const string FastDelivery = "Доставка за час";
		private string _name;
		private TimeSpan _from;
		private TimeSpan _to;
		private Guid? _fileId;
		private string _roboatsAudiofile;
		private bool _isArchive;

		public DeliverySchedule()
		{
			Name = String.Empty;
		}

		[Required (ErrorMessage = "Не заполнено название.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get => _name;
			set => SetField (ref _name, value, () => Name);
		}

		[Display (Name = "От часа")]
		public virtual TimeSpan From {
			get => _from;
			set => SetField (ref _from, value, () => From);
		}

		[Display (Name = "До часа")]
		public virtual TimeSpan To {
			get => _to;
			set => SetField (ref _to, value, () => To);
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

		public virtual string DeliveryTime => $"с {_from:hh\\:mm} до {_to:hh\\:mm}";

		[Display(Name = "Архивный")]
		public virtual bool IsArchive {
			get => _isArchive;
			set => SetField(ref _isArchive, value);
		}
		public virtual int? RoboatsId => Id;

		public virtual RoboatsEntityType RoboatsEntityType => RoboatsEntityType.DeliverySchedules;

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_delivery_schedule") && Id > 0)
				yield return new ValidationResult("Вы не можете изменять график доставки");

			if(From > To)
				yield return new ValidationResult("Окончание периода доставки не может быть раньше его начала.",
					new[] { nameof(From), nameof(To) });

			if(From.TotalMinutes < 1)
				yield return new ValidationResult("Время начало периода не может быть 0.",
					new[] { nameof(From) });

			if(To.TotalMinutes < 1)
				yield return new ValidationResult("Время окончания периода не может быть 0.",
					new[] { nameof(To) });

			if(From.TotalDays > 1)
				yield return new ValidationResult("Время начало периода не может быть больше 24 часов.",
					new[] { nameof(From) });

			if(To.TotalDays > 1)
				yield return new ValidationResult("Время окончания периода не может быть больше 24 часов.",
					new[] { nameof(To) });
		}

		#endregion

		public static Expression<Func<DeliverySchedule, bool>> GetNameCompareExpression(string searchText)
		{
			return entity => (entity.Name ?? string.Empty).IndexOf(searchText) >= 0;
		}

	}
}

