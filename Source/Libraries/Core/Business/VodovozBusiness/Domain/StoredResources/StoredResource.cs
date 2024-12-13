using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.StoredResources
{
	public class StoredResource : BusinessObjectBase<StoredResource>, IDomainObject , IValidatableObject
	{
		public virtual int Id { get; set; }

		private byte[] _binaryFile;
		[Display(Name = "Бинарный файл")]
		public virtual byte[] BinaryFile {
			get { return _binaryFile; }
			set { SetField(ref _binaryFile, value); }
		}

		[Display(Name = "Тип")]
		public virtual ResoureceType Type { get; set; } = ResoureceType.Binary;

		private string _name;
		[Display(Name = "Название")]
		public virtual string Name {
			get { return _name; }
			set { SetField(ref _name, value); }
		}

		private ImageType _imageType;
		[Display(Name = "Тип изображения")]
		public virtual ImageType ImageType
		{
			get { return _imageType; }
			set { SetField(ref _imageType, value); }
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

	public enum ImageType
	{
		[Display(Name = "Подпись")]
		Signature,
		[Display(Name = "Прочее")]
		Other
	}
}
