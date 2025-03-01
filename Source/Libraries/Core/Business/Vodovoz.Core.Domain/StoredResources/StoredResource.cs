using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.StoredResources
{
	/// <summary>
	/// Хранимые ресурсы, файлы
	/// </summary>
	public class StoredResource : BusinessObjectBase<StoredResource>, IDomainObject, IValidatableObject
	{
		private int _id;
		private ResourceType _type = ResourceType.Binary;
		private string _name;
		private byte[] _binaryFile;
		private ImageType _imageType;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Тип
		/// </summary>
		[Display(Name = "Тип")]
		public virtual ResourceType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		/// <summary>
		/// Название
		/// </summary>
		[Display(Name = "Название")]
		public virtual string Name
		{
			get { return _name; }
			set { SetField(ref _name, value); }
		}

		/// <summary>
		/// Бинарный файл
		/// </summary>
		[Display(Name = "Бинарный файл")]
		public virtual byte[] BinaryFile
		{
			get { return _binaryFile; }
			set { SetField(ref _binaryFile, value); }
		}

		/// <summary>
		/// Тип изображения
		/// </summary>
		[Display(Name = "Тип изображения")]
		public virtual ImageType ImageType
		{
			get { return _imageType; }
			set { SetField(ref _imageType, value); }
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(Name))
			{
				yield return new ValidationResult("Файл должен иметь название", new[] { nameof(Name) });
			}

			if(BinaryFile == null)
			{
				yield return new ValidationResult("Должен быть указан файл", new[] { nameof(BinaryFile) });
			}
		}
	}
}
