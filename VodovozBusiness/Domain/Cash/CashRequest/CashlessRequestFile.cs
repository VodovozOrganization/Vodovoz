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
		private string fileStorageId;
		private byte[] byteFile;
		private CashlessRequest _cashlessRequest;

		public virtual int Id { get; set; }

		[Display(Name = "Идентификатор файла в системе хранения")]
		public virtual string FileStorageId
		{
			get => fileStorageId;
			set => SetField(ref fileStorageId, value, () => FileStorageId);
		}

		[Display(Name = "Файл")]
		public virtual byte[] ByteFile
		{
			get => byteFile;
			set => SetField(ref byteFile, value, () => ByteFile);
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
