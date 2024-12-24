using NetTopologySuite.Index.HPRtree;
using QS.Extensions.Observable.Collections.List;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public abstract class CustomerEdoTask : EdoTask
	{
		private CustomerEdoRequest _customerEdoRequest;
		private ObservableList<EdoTaskItem> _items;

		[Display(Name = "Заявка ЭДО отправки клиенту")]
		public virtual CustomerEdoRequest CustomerEdoRequest
		{
			get => _customerEdoRequest;
			set => SetField(ref _customerEdoRequest, value);
		}

		[Display(Name = "Строки с кодами")]
		public virtual ObservableList<EdoTaskItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}
	}
}
