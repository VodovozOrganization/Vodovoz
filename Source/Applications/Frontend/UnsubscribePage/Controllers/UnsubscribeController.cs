using Microsoft.AspNetCore.Mvc;
using QS.DomainModel.UoW;
using System;
using System.Diagnostics;
using System.Linq;
using UnsubscribePage.Models;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;

namespace UnsubscribePage.Controllers
{
	/// <summary>
	/// Контроллер страницы отписки от email-рассылки.
	/// </summary>
	public class UnsubscribeController : Controller
	{
		private readonly IUnsubscribeViewModelFactory _unsubscribeViewModelFactory;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmailSettings _emailSettings;
		private readonly IUnitOfWorkFactory _uowFactory;

		/// <summary>
		/// Создаёт экземпляр контроллера.
		/// </summary>
		/// <param name="unsubscribeViewModelFactory">Фабрика ViewModel.</param>
		/// <param name="emailRepository">Репозиторий email-данных.</param>
		/// <param name="emailSettings">Настройки email.</param>
		/// <param name="uowFactory">Фабрика UnitOfWork.</param>
		public UnsubscribeController(
			IUnsubscribeViewModelFactory unsubscribeViewModelFactory,
			IEmailRepository emailRepository,
			IEmailSettings emailSettings,
			IUnitOfWorkFactory uowFactory)
		{
			_unsubscribeViewModelFactory = unsubscribeViewModelFactory ?? throw new ArgumentNullException(nameof(unsubscribeViewModelFactory));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		/// <summary>
		/// Открывает страницу отписки и сразу сохраняет факт отписки.
		/// </summary>
		/// <param name="emailGuid">Guid email-ссылки.</param>
		/// <returns>Страница отписки.</returns>
		[HttpGet]
		[Route("/{emailGuid:guid}")]
		public IActionResult Index(Guid emailGuid)
		{
			var viewModel = _unsubscribeViewModelFactory.CreateNewUnsubscribeViewModel(emailGuid, _emailRepository, _emailSettings);

			if(viewModel.CounterpartyBulkSubscribeNode is CounterpartyBulkSubscribeNode node)
			{
				var unsubscribingEvent = new UnsubscribingBulkEmailEvent
				{
					CounterpartyEmailType = viewModel.CounterpartyBulkSubscribeNode.CounterpartyEmailType,

					Counterparty = new Counterparty
					{
						Id = node.CounterpartyId
					}
				};

				using var unitOfWork = _uowFactory.CreateWithoutRoot("Отписка от массовой рассылки");

				var counterparty = unitOfWork.GetById<Counterparty>(node.CounterpartyId);
				switch(node.CounterpartyEmailType)
				{
					case CounterpartyEmailType.ClosingDeliveries:
						counterparty.DisableClosingDeliveriesMailing = true;
						break;
					case CounterpartyEmailType.LetterOfClaim:
						counterparty.DisableClaimMailing = true;
						break;
					case CounterpartyEmailType.InformationLetter:
						counterparty.DisableDebtMailing = true;
						break;
				}

				unitOfWork.Save(counterparty);
 
				unitOfWork.Save(unsubscribingEvent);

				unitOfWork.Commit();

				viewModel.EmailEventId = unsubscribingEvent.Id;
			}

			return View(viewModel);
		}

		/// <summary>
		/// Сохраняет причину отписки.
		/// </summary>
		/// <param name="viewModel">Данные формы.</param>
		/// <returns>Redirect на финальную страницу.</returns>
		[HttpPost]
		[Route("/{emailGuid:guid}")]
		public IActionResult Index(UnsubscribeViewModel viewModel)
		{
			if(!ModelState.IsValid)
			{
				return View(viewModel);
			}

			var selectedReason = viewModel.ReasonsList?.Single(x => x.Id == viewModel.SelectedReasonId);

			using var unitOfWork = _uowFactory.CreateWithoutRoot("Сохранение причины отписки");

			var unsubscribingEvent = unitOfWork.GetById<UnsubscribingBulkEmailEvent>(viewModel.EmailEventId);

			if(unsubscribingEvent == null)
			{
				return NotFound();
			}

			unsubscribingEvent.Reason = selectedReason;

			unsubscribingEvent.ReasonDetail = selectedReason?.Id == viewModel.OtherReasonId
				? viewModel.OtherReason
				: null;

			unitOfWork.Save(unsubscribingEvent);
			unitOfWork.Commit();

			return RedirectToAction(nameof(Finish));
		}

		/// <summary>
		/// Финальная страница после отправки причины отписки.
		/// </summary>
		/// <returns>Финальная страница.</returns>
		public IActionResult Finish()
		{
			return View();
		}

		/// <summary>
		/// Страница отображения ошибки.
		/// </summary>
		/// <returns>Страница ошибки.</returns>
		[ResponseCache(
			Duration = 0,
			Location = ResponseCacheLocation.None,
			NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel
			{
				RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
			});
		}
	}
}
