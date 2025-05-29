using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.PrintableDocuments;

namespace Vodovoz.Core.Domain.Users.Settings
{
	/// <summary>
	/// Настройки принтера документа
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Feminine,
		Accusative = "настройку принтера документа",
		AccusativePlural = "настройки принтеров документов",
		Genitive = "настройки принтера документа",
		GenitivePlural = "настроек принтеров документов",
		Nominative = "настройка принтера документа",
		NominativePlural = "настройки принтеров документов",
		Prepositional = "настройке принтера документа",
		PrepositionalPlural = "настройках принтеров документов")]
	public class DocumentPrinterSetting : PropertyChangedBase, IDomainObject
	{
		private UserSettings _userSettings;
		private CustomPrintDocumentType _documentType;
		private string _printerName;
		private int _numberOfCopies;
		private int _id;

		public DocumentPrinterSetting()
		{
			NumberOfCopies = 1;
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Настройки пользователя
		/// </summary>
		[Display(Name = "Настройки пользователя")]
		public virtual UserSettings UserSettings
		{
			get => _userSettings;
			set => SetField(ref _userSettings, value);
		}

		/// <summary>
		/// Тип документа
		/// </summary>
		[Display(Name = "Тип документа")]
		public virtual CustomPrintDocumentType DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}

		/// <summary>
		/// Название принтера
		/// </summary>
		[Display(Name = "Название принтера")]
		public virtual string PrinterName
		{
			get => _printerName;
			set => SetField(ref _printerName, value);
		}

		/// <summary>
		/// Число копий для печати
		/// </summary>
		[Display(Name = "Число копий")]
		public virtual int NumberOfCopies
		{
			get => _numberOfCopies;
			set => SetField(ref _numberOfCopies, value);
		}
	}
}
