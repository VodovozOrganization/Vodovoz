using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Common
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах",
		Nominative = "информация о прикрепленном файле")]
	public abstract class FileInformation : PropertyChangedBase, IDomainObject
	{
		private string _fileName;

		public virtual int Id { get; set; }

		[Display(Name = "Имя файла")]
		public virtual string FileName
		{
			get => _fileName;
			set => SetField(ref _fileName, value);
		}
	}
}
