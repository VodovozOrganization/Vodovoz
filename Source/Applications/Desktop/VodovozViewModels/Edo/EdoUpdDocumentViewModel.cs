using System;
using System.Collections.Generic;
using System.Linq;
using Comparer;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.ViewModels.Services.Edo;

namespace Vodovoz.ViewModels.Edo
{
	/// <summary>
	/// Просмотр значений сохранённого документа УПД.
	/// </summary>
	public class EdoUpdDocumentViewModel : DialogViewModelBase, IDisposable
	{
		private readonly IUnitOfWork _uow;

		/// <summary>Открывает сохранённый УПД для просмотра.</summary>
		/// <param name="documentId">Код сохранённого УПД.</param>
		/// <param name="unitOfWorkFactory">Фабрика контекстов данных.</param>
		/// <param name="navigationManager">Навигация.</param>
		/// <param name="documentService">Сервис получения сохранённых УПД.</param>
		public EdoUpdDocumentViewModel(int documentId, IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager, IEdoUpdDocumentService documentService) : base(navigationManager)
		{
			if(documentService == null)
			{
				throw new ArgumentNullException(nameof(documentService));
			}
			_uow = unitOfWorkFactory.CreateWithoutRoot();
			var document = documentService.GetDocument(_uow, documentId);
			if(document == null)
			{
				_uow.Dispose();
				throw new AbortCreatingPageException($"УПД №{documentId} не найден", "Документ не найден");
			}

			Title = $"УПД №{document.Number} от {document.Date:dd.MM.yyyy}";
			Fields = CreateFields(document);
		}

		/// <summary>Поля документа и вложенных позиций.</summary>
		public IList<UpdCompareViewModel> Fields { get; }

		private static IList<UpdCompareViewModel> CreateFields(EdoUpdDocument document)
		{
			var individualCode = new ObjectPropertyComparer<TrueMarkWaterIdentificationCode>()
				.DefaultExclude().Include(x => x.Id).Include(x => x.IdentificationCode).Build();
			var groupCode = new ObjectPropertyComparer<TrueMarkWaterGroupCode>()
				.DefaultExclude().Include(x => x.Id).Include(x => x.IdentificationCode).Build();
			var code = new ObjectPropertyComparer<UpdProductCode>().DefaultInclude()
				.CompareObject(x => x.IndividualCode, individualCode)
				.CompareObject(x => x.GroupCode, groupCode).Build();
			var position = new ObjectPropertyComparer<UpdInventPosition>().DefaultInclude()
				.CompareObject(x => x.AssignedOrderItem, new ObjectPropertyComparer<OrderItemEntity>()
					.DefaultExclude().Include(x => x.Id).Build())
				.CompareObject(x => x.Codes, code).Build();
			var organization = new ObjectPropertyComparer<UpdOrganization>().DefaultInclude().Build();
			var payment = new ObjectPropertyComparer<UpdPayment>().DefaultInclude().Build();
			var comparer = new ObjectPropertyComparer<EdoUpdDocument>().DefaultInclude()
				.CompareObject(x => x.Seller, organization)
				.CompareObject(x => x.Shipper, organization)
				.CompareObject(x => x.Customer, organization)
				.CompareObject(x => x.Consignee, organization)
				.CompareObject(x => x.InventPositions, position)
				.CompareObject(x => x.Payments, payment).Build();
			return comparer.Compare(document, new EdoUpdDocument())
				.Select(field => new UpdCompareViewModel(field, false)).ToList();
		}

		/// <summary>Освобождает контекст просмотра документа.</summary>
		public void Dispose() => _uow.Dispose();
	}
}
