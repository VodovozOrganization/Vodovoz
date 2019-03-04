using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.StoredResources
{
	public class StoredResource : BusinessObjectBase<StoredResource>, IDomainObject , IValidatableObject
	{
		public virtual int Id { get; set; }

		private byte[] binaryFile;
		[Display(Name = "Бинарный файл")]
		public virtual byte[] BinaryFile {
			get { return binaryFile; }
			set { SetField(ref binaryFile, value, () => BinaryFile); }
		}

		[Display(Name = "Тип")]
		public virtual ResoureceType Type { get; set; } = ResoureceType.Binary;

		private string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrEmpty(Name))
				yield return new ValidationResult("Файл должен иметь название", new[] { "Name" });

			if(BinaryFile == null)
				yield return new ValidationResult("Должен быть указан файл", new[] { "BinaryFile" });
		}
	}

	public enum ResoureceType
	{
		Image,
		Pdf,
		Binary
	}

	public class ResoureceFileStringType : NHibernate.Type.EnumStringType
	{
		public ResoureceFileStringType() : base(typeof(ResoureceType))
		{
		}
	}
}
