using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Edo
{
	public class EdoTaskItem : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrderEdoTask _customerEdoTask;
		private TrueMarkProductCode _productCode;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Код ЭДО задачи")]
		public virtual OrderEdoTask CustomerEdoTask
		{
			get => _customerEdoTask;
			set => SetField(ref _customerEdoTask, value);
		}

		[Display(Name = "Код ЧЗ")]
		public virtual TrueMarkProductCode ProductCode
		{
			get => _productCode;
			set => SetField(ref _productCode, value);
		}
	}
}
