using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Cash
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "приложения к оплате по Б/Н",
		Nominative = "приложение к оплате по Б/Н"
	)]
	[HistoryTrace]
	public class CashlessRequestFile : PropertyChangedBase, IDomainObject
	{
		private string _fileStorageId;
		private byte[] _byteFile;
		private CashlessRequest _cashlessRequest;

		public virtual int Id { get; set; }

		[Display(Name = "Идентификатор файла в системе хранения")]
		public virtual string FileStorageId
		{
			get => _fileStorageId;
			set => SetField(ref _fileStorageId, value);
		}

		[Display(Name = "Файл")]
		public virtual byte[] ByteFile
		{
			get => _byteFile;
			set => SetField(ref _byteFile, value);
		}

		[Display(Name = "Запрос на оплату по Б/Н")]
		public virtual CashlessRequest CashlessRequest
		{
			get => _cashlessRequest;
			set => SetField(ref _cashlessRequest, value);
		}

		public virtual string Title => $"Файл \"{FileStorageId}\"";
	}
}
