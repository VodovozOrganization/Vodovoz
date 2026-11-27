using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Domain.Edo
{
	public class EdoUpdInventPositionCode : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _quantity;
		private TrueMarkIdentificationCode _individualCode;
		private TrueMarkGroupCode _groupCode;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Количество")]
		public virtual int Quantity
		{
			get => _quantity;
			set => SetField(ref _quantity, value);
		}

		[Display(Name = "Индивидуальный код")]
		public virtual TrueMarkIdentificationCode IndividualCode
		{
			get => _individualCode;
			set => SetField(ref _individualCode, value);
		}

		[Display(Name = "Групповой код")]
		public virtual TrueMarkGroupCode GroupCode
		{
			get => _groupCode;
			set => SetField(ref _groupCode, value);
		}
	}
}
