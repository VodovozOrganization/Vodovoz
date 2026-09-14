using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Запрос переотправки документа ЭДО после отмены конкретного документа вывода кодов из оборота в ЧЗ.
	/// </summary>
	public class EdoResendAfterTrueMarkCancellationRequest : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _version;
		private OrderEntity _order;
		private OrderEdoTask _originalEdoTask;
		private ManualEdoRequest _resendEdoRequest;
		private TrueMarkDocument _withdrawalDocument;
		private TrueMarkDocument _cancellationDocument;
		private EdoResendAfterTrueMarkCancellationStatus _status;
		private int _cancellationAttemptsCount;
		private string _errorMessage;
		private DateTime _creationTime = DateTime.Now;
		private DateTime _lastUpdateTime = DateTime.Now;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Версия")]
		public virtual int Version
		{
			get => _version;
			set => SetField(ref _version, value);
		}

		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		[Display(Name = "Исходная задача ЭДО")]
		public virtual OrderEdoTask OriginalEdoTask
		{
			get => _originalEdoTask;
			set => SetField(ref _originalEdoTask, value);
		}

		[Display(Name = "Заявка ЭДО для переотправки")]
		public virtual ManualEdoRequest ResendEdoRequest
		{
			get => _resendEdoRequest;
			set => SetField(ref _resendEdoRequest, value);
		}

		[Display(Name = "Документ вывода из оборота")]
		public virtual TrueMarkDocument WithdrawalDocument
		{
			get => _withdrawalDocument;
			set => SetField(ref _withdrawalDocument, value);
		}

		[Display(Name = "Документ отмены вывода из оборота")]
		public virtual TrueMarkDocument CancellationDocument
		{
			get => _cancellationDocument;
			set => SetField(ref _cancellationDocument, value);
		}

		[Display(Name = "Состояние")]
		public virtual EdoResendAfterTrueMarkCancellationStatus Status
		{
			get => _status;
			set => SetField(ref _status, value);
		}

		[Display(Name = "Количество попыток отмены")]
		public virtual int CancellationAttemptsCount
		{
			get => _cancellationAttemptsCount;
			set => SetField(ref _cancellationAttemptsCount, value);
		}

		[Display(Name = "Ошибка")]
		public virtual string ErrorMessage
		{
			get => _errorMessage;
			set => SetField(ref _errorMessage, value);
		}

		[Display(Name = "Время создания")]
		public virtual DateTime CreationTime
		{
			get => _creationTime;
			set => SetField(ref _creationTime, value);
		}

		[Display(Name = "Время последнего изменения")]
		public virtual DateTime LastUpdateTime
		{
			get => _lastUpdateTime;
			set => SetField(ref _lastUpdateTime, value);
		}

		/// <summary>
		/// Регистрирует новую попытку отправки отмены в ЧЗ.
		/// </summary>
		public virtual void RegisterCancellationAttempt()
		{
			EnsureStatus(EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation);
			CancellationAttemptsCount++;
			LastUpdateTime = DateTime.Now;
		}

		/// <summary>
		/// Фиксирует отправленный документ отмены.
		/// </summary>
		public virtual void MarkCancellationSent(TrueMarkDocument cancellationDocument)
		{
			EnsureStatus(EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation);
			CancellationDocument = cancellationDocument ?? throw new ArgumentNullException(nameof(cancellationDocument));
			Status = EdoResendAfterTrueMarkCancellationStatus.CancellationSent;
			ErrorMessage = null;
			LastUpdateTime = DateTime.Now;
		}

		/// <summary>
		/// Фиксирует ошибку отправки или обработки отмены в ЧЗ.
		/// </summary>
		public virtual void MarkCancellationFailed(string errorMessage)
		{
			if(Status != EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation
				&& Status != EdoResendAfterTrueMarkCancellationStatus.CancellationSent)
			{
				throw new InvalidOperationException($"Нельзя зафиксировать ошибку отмены из состояния {Status}");
			}

			Status = EdoResendAfterTrueMarkCancellationStatus.CancellationFailed;
			ErrorMessage = errorMessage;
			LastUpdateTime = DateTime.Now;
		}

		/// <summary>
		/// Возвращает ошибочный запрос в очередь отправки отмены.
		/// </summary>
		public virtual void RetryCancellation()
		{
			if(Status != EdoResendAfterTrueMarkCancellationStatus.CancellationFailed)
			{
				throw new InvalidOperationException("Повторная отмена доступна только для запроса с ошибкой");
			}

			Status = EdoResendAfterTrueMarkCancellationStatus.WaitingForCancellation;
			CancellationDocument = null;
			ErrorMessage = null;
			LastUpdateTime = DateTime.Now;
		}

		/// <summary>
		/// Разрешает публикацию сохранённой заявки ЭДО после успешной отмены в ЧЗ.
		/// </summary>
		public virtual void MarkReadyToResend()
		{
			EnsureStatus(EdoResendAfterTrueMarkCancellationStatus.CancellationSent);
			Status = EdoResendAfterTrueMarkCancellationStatus.ReadyToResend;
			ErrorMessage = null;
			LastUpdateTime = DateTime.Now;
		}

		/// <summary>
		/// Фиксирует запуск переотправки документа ЭДО.
		/// </summary>
		public virtual void MarkCompleted()
		{
			EnsureStatus(EdoResendAfterTrueMarkCancellationStatus.ReadyToResend);
			Status = EdoResendAfterTrueMarkCancellationStatus.Completed;
			LastUpdateTime = DateTime.Now;
		}

		private void EnsureStatus(EdoResendAfterTrueMarkCancellationStatus expectedStatus)
		{
			if(Status != expectedStatus)
			{
				throw new InvalidOperationException($"Ожидалось состояние {expectedStatus}, текущее состояние {Status}");
			}
		}
	}
}
