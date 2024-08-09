using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;
using Vodovoz.PrintableDocuments;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "настройки принтеров документов",
		Nominative = "настройка принтера документа")]
	public class DocumentPrinterSetting : PropertyChangedBase, IDomainObject
	{
		private UserSettings _userSettings;
		private CustomPrintDocumentType _documentType;
		private string _printerName;
		private int _numberOfCopies;

		public DocumentPrinterSetting()
		{
			NumberOfCopies = 1;
		}

		public virtual int Id { get; set; }

		[Display(Name = "Настройки пользователя")]
		public virtual UserSettings UserSettings
		{
			get => _userSettings;
			set => SetField(ref _userSettings, value);
		}

		[Display(Name = "Тип документа")]
		public virtual CustomPrintDocumentType DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}

		[Display(Name = "Название принтера")]
		public virtual string PrinterName
		{
			get => _printerName;
			set => SetField(ref _printerName, value);
		}

		[Display(Name = "Число копий")]
		public virtual int NumberOfCopies
		{
			get => _numberOfCopies;
			set => SetField(ref _numberOfCopies, value);
		}
	}
}
