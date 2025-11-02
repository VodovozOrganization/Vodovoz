using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class DocumentEdoTask : OrderEdoTask, IUpdEnventPositionsTask
	{
		private int _fromOrganization;
		private int _toCustomer;
		private EdoDocumentType _documentType;
		private DocumentEdoTaskStage _stage;
		private IList<EdoUpdInventPosition> _updInventPositions = new List<EdoUpdInventPosition>();

		[Display(Name = "Код организации")]
		public virtual int FromOrganization
		{
			get => _fromOrganization;
			set => SetField(ref _fromOrganization, value);
		}

		[Display(Name = "Код контрагента")]
		public virtual int ToCustomer
		{
			get => _toCustomer;
			set => SetField(ref _toCustomer, value);
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

		[Display(Name = "Строка УПД документа")]
		public virtual IList<EdoUpdInventPosition> UpdInventPositions
		{
			get => _updInventPositions;
			set => SetField(ref _updInventPositions, value);
		}

		public override EdoTaskType TaskType => EdoTaskType.Document;
	}
}
