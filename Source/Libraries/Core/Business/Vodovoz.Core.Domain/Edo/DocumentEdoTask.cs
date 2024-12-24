using QS.Extensions.Observable.Collections.List;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class DocumentEdoTask : CustomerEdoTask
	{
		private int _fromOrganization;
		private int _toClient;
		private EdoDocumentType _documentType;
		private DocumentEdoTaskStage _stage;
		private ObservableList<TransferEdoRequest> _transferRequests;

		[Display(Name = "Код организации")]
		public virtual int FromOrganization
		{
			get => _fromOrganization;
			set => SetField(ref _fromOrganization, value);
		}

		[Display(Name = "Код контрагента")]
		public virtual int ToClient
		{
			get => _toClient;
			set => SetField(ref _toClient, value);
		}

		[Display(Name = "Тип документа")]
		public virtual EdoDocumentType DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}

		[Display(Name = "Стадия")]
		public virtual DocumentEdoTaskStage Stage
		{
			get => _stage;
			set => SetField(ref _stage, value);
		}

		[Display(Name = "Заявки на перенос")]
		public virtual ObservableList<TransferEdoRequest> TransferEdoRequests
		{
			get => _transferRequests;
			set => SetField(ref _transferRequests, value);
		}
	}
}
