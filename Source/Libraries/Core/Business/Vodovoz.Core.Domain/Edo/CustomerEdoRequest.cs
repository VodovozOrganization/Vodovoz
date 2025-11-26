using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Заявка на отправку документов ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на отправку документов ЭДО",
		NominativePlural = "заявки на отправку документов ЭДО"
	)]
	/// Переименовать в FormalEdoRequest
	public abstract class CustomerEdoRequest : EdoRequest
	{
		private IObservableList<TrueMarkProductCode> _productCodes = new ObservableList<TrueMarkProductCode>();

		/// <summary>
		/// Тип заявки
		/// </summary>
		public abstract EdoRequestType DocumentRequestType { get; }

		/// <summary>
		/// Коды маркировки
		/// </summary>
		[Display(Name = "Коды маркировки")]
		public virtual IObservableList<TrueMarkProductCode> ProductCodes
		{
			get => _productCodes;
			set => SetField(ref _productCodes, value);
		}
	}
}
