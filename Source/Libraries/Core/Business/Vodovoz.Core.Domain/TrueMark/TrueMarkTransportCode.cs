using QS.DomainModel.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark
{
	/// <summary>
	/// Транспортный код честного знака
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Genitive = "транспортного кода честного знака",
		GenitivePlural = "транспортных кодов честного знака",
		Nominative = "транспортный код честного знака",
		NominativePlural = "транспортные коды честного знака",
		Accusative = "транспортный код честного знака",
		AccusativePlural = "транспортные коды честного знака",
		Prepositional = "транспортном коде честного знака",
		PrepositionalPlural = "транспортных кодах честного знака")]
	public class TrueMarkTransportCode : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private string _rawCode;
		private bool _isInvalid;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Необработанный код
		/// </summary>
		[Display(Name = "Необработанный код")]
		public virtual string RawCode
		{
			get => _rawCode;
			set => SetField(ref _rawCode, value);
		}

		/// <summary>
		/// Код не валидный
		/// </summary>
		[Display(Name = "Код не валидный")]
		public virtual bool IsInvalid
		{
			get => _isInvalid;
			set => SetField(ref _isInvalid, value);
		}

		public override bool Equals(object obj)
		{
			if(obj is TrueMarkWaterIdentificationCode code)
			{
				return RawCode == code.RawCode;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return -1155050507 + EqualityComparer<string>.Default.GetHashCode(RawCode);
		}
	}
}
