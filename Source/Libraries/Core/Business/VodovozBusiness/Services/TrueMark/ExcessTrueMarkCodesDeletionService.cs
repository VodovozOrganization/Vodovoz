using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Services;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.EntityRepositories.TrueMark;

namespace VodovozBusiness.Services.TrueMark
{
	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	public class ExcessTrueMarkCodesDeletionService : IExcessTrueMarkCodesDeletionService
	{
		private readonly ICurrentPermissionService _permissions;
		private readonly TrueMarkCodesPoolFactory _poolFactory;
		private readonly ITrueMarkRepository _repository;

		/// <summary>Конструктор.</summary>
		/// <param name="permissions">Права текущего пользователя.</param>
		/// <param name="poolFactory">Фабрика пула в транзакции операции.</param>
		/// <param name="repository">Репозиторий кодов маркировки.</param>
		public ExcessTrueMarkCodesDeletionService(ICurrentPermissionService permissions,
			TrueMarkCodesPoolFactory poolFactory, ITrueMarkRepository repository)
		{
			_permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
			_poolFactory = poolFactory ?? throw new ArgumentNullException(nameof(poolFactory));
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public bool CanDelete(OrderEdoTask task)
		{
			return _permissions.ValidatePresetPermission(EdoPermissions.CanDeleteExcessTrueMarkCodes)
				&& task?.Status == EdoTaskStatus.Problem
				&& (task is DocumentEdoTask document && document.DocumentType == EdoDocumentType.UPD
					&& document.Stage == DocumentEdoTaskStage.New && !document.UpdInventPositions.Any()
					|| task is ReceiptEdoTask receipt && receipt.ReceiptStatus == EdoReceiptStatus.New
					&& !receipt.FiscalDocuments.Any())
				&& GetExcessCount(task) > 0;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public Result Delete(IUnitOfWork uow, int taskId, IEnumerable<int> productCodeIds)
		{
			var task = uow.GetById<OrderEdoTask>(taskId);
			if(!CanDelete(task))
			{
				return Failure("Unavailable", "Удаление доступно при превышении кодов в проблемной задаче до распределения, при наличии права.");
			}

			var ids = new HashSet<int>(productCodeIds);
			var items = task.Items.Where(x => ids.Contains(x.ProductCode.Id)).ToList();
			if(ids.Count == 0 || items.Select(x => x.ProductCode.Id).Distinct().Count() != ids.Count)
			{
				return Failure("SelectionChanged", "Выбранные коды отсутствуют в задаче. Обновите список и повторите выбор.");
			}
			if(items.Count > GetExcessCount(task))
			{
				return Failure("TooManyCodes", "Выбрано больше кодов, чем превышение необходимого количества.");
			}

			var codes = items.SelectMany(x => new[] { x.ProductCode.SourceCode, x.ProductCode.ResultCode })
				.Where(x => x != null).GroupBy(x => x.Id).Select(x => x.First()).ToList();
			if(items.Any(x => x.ProductCode.SourceCode == null && x.ProductCode.ResultCode == null))
			{
				return Failure("EmptyCode", "У выбранного элемента отсутствует код для возврата в пул.");
			}
			var codeIds = codes.Select(x => x.Id).ToArray();
			if(_repository.AreCodesUsedByOtherEdoTasks(uow, taskId, codeIds)
				|| task.Items.Where(x => !ids.Contains(x.ProductCode.Id)).Any(x =>
					codeIds.Contains(x.ProductCode.SourceCode?.Id ?? 0)
					|| codeIds.Contains(x.ProductCode.ResultCode?.Id ?? 0)))
			{
				return Failure("CodeInUse", "Код связан с другим элементом или документом. Возврат в пул невозможен.");
			}

			var pool = _poolFactory.Create(uow);
			foreach(var code in codes)
			{
				pool.PutCode(code.Id);
			}
			foreach(var item in items)
			{
				item.ProductCode.ResultCode = null;
				item.ProductCode.SourceCodeStatus = SourceProductCodeStatus.SavedToPool;
				uow.Save(item.ProductCode);
				task.Items.Remove(item);
				uow.Delete(item);
			}
			uow.Save(task);
			return Result.Success();
		}

		private static decimal GetExcessCount(OrderEdoTask task) =>
			task.Items.Count(x => x.ProductCode.SourceCode != null || x.ProductCode.ResultCode != null)
			- task.FormalEdoRequest.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark).Sum(x => x.CurrentCount);

		private static Result Failure(string code, string message) => Result.Failure(new Error("ExcessCodes." + code, message));
	}
}
