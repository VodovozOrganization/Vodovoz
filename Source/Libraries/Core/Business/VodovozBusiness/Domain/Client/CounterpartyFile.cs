using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
    [Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "документы контрагента",
		Nominative = "документ контрагента"
    )]
    [HistoryTrace]
    public class CounterpartyFile : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private string fileStorageId;
		[Display(Name = "Идентификатор файла в системе хранения")]
		public virtual string FileStorageId
		{
			get => fileStorageId;
			set => SetField(ref fileStorageId, value);
		}

		private Counterparty counterparty;
		[Display(Name = "Рекламация")]
		public virtual Counterparty Counterparty
		{
			get => counterparty;
			set => SetField(ref counterparty, value);
		}

		private byte[] byteFile;
		[Display(Name = "Файл")]
		public virtual byte[] ByteFile
		{
			get => byteFile;
			set => SetField(ref byteFile, value);
		}

		public virtual string Title => $"Файл \"{FileStorageId}\"";
	}
}
