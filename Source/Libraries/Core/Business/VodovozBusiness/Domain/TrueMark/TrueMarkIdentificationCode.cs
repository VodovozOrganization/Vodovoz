using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Domain.TrueMark
{
	[
		Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "код честного знака",
			Nominative = "код честного знака"
		)
	]
	public class TrueMarkWaterIdentificationCode : PropertyChangedBase, IDomainObject
	{
		private string _rawCode;
		private bool _isInvalid;
		private string _gtin;
		private string _serialNumber;
		private string _checkCode;

		public virtual int Id { get; set; }

		[Display(Name = "Необработанный код")]
		public virtual string RawCode
		{
			get => _rawCode;
			set => SetField(ref _rawCode, value);
		}

		[Display(Name = "Код не валидный")]
		public virtual bool IsInvalid
		{
			get => _isInvalid;
			set => SetField(ref _isInvalid, value);
		}

		[Display(Name = "Код GTIN")]
		public virtual string GTIN
		{
			get => _gtin;
			set => SetField(ref _gtin, value);
		}

		[Display(Name = "Серийный номер экземпляра")]
		public virtual string SerialNumber
		{
			get => _serialNumber;
			set => SetField(ref _serialNumber, value);
		}

		[Display(Name = "Код проверки")]
		public virtual string CheckCode
		{
			get => _checkCode;
			set => SetField(ref _checkCode, value);
		}
	}
}
