using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{

	public class TransferEdoRequest : PropertyChangedBase
	{
		private int _id;
		private TransferEdoRequestIteration _iteration;
		private int _fromOrganizationId;
		private int _toOrganizationId;
		private IObservableList<EdoTaskItem> _transferedItems = new ObservableList<EdoTaskItem>();
		private TransferEdoTask _transferEdoTask;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Итерация")]
		public virtual TransferEdoRequestIteration Iteration
		{
			get => _iteration;
			set => SetField(ref _iteration, value);
		}

		[Display(Name = "Код организации отправителя")]
		public virtual int FromOrganizationId
		{
			get => _fromOrganizationId;
			set => SetField(ref _fromOrganizationId, value);
		}

		[Display(Name = "Код организации получателя")]
		public virtual int ToOrganizationId
		{
			get => _toOrganizationId;
			set => SetField(ref _toOrganizationId, value);
		}

		[Display(Name = "Перемещаемые коды")]
		public virtual IObservableList<EdoTaskItem> TransferedItems
		{
			get => _transferedItems;
			set => SetField(ref _transferedItems, value);
		}

		[Display(Name = "Задача перемещения")]
		public virtual TransferEdoTask TransferEdoTask
		{
			get => _transferEdoTask;
			set => SetField(ref _transferEdoTask, value);
		}
	}
}
