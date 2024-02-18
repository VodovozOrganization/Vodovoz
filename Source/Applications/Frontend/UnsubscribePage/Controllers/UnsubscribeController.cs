using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using UnsubscribePage.Models;
using Vodovoz.EntityRepositories;
using Vodovoz.Settings.Common;

namespace UnsubscribePage.Controllers
{
	public class UnsubscribeController : Controller
	{
		private readonly IUnsubscribeViewModelFactory _unsubscribeViewModelFactory;
		private readonly IEmailRepository _emailRepository;
		private readonly IEmailSettings _emailParametersProvider;

		public UnsubscribeController(IUnsubscribeViewModelFactory unsubscribeViewModelFactory, IEmailRepository emailRepository, IEmailSettings emailParametersProvider)
		{
			_unsubscribeViewModelFactory = unsubscribeViewModelFactory ?? throw new ArgumentNullException(nameof(unsubscribeViewModelFactory));
			_emailRepository = emailRepository ?? throw new ArgumentNullException(nameof(emailRepository));
			_emailParametersProvider = emailParametersProvider ?? throw new ArgumentNullException(nameof(emailParametersProvider));
		}

		[HttpGet]
		[Route("/{emailGuid:guid}")]
		public IActionResult Index(Guid emailGuid)
		{
			var viewModel = _unsubscribeViewModelFactory.CreateNewUnsubscribeViewModel(emailGuid, _emailRepository, _emailParametersProvider);

			if(viewModel.CounterpartyId != 0)
			{
				viewModel.SaveUnsubscribe();
			}

			return View(viewModel);
		}

		[HttpPost]
		[Route("/{emailGuid:guid}")]
		public IActionResult Index(UnsubscribeViewModel viewModel)
		{
			if(!ModelState.IsValid)
			{
				return View(viewModel);
			}

			var selectedReason = viewModel.ReasonsList?.Single(x => x.Id == viewModel.SelectedReasonId);
			
			viewModel.SaveUnsubscribe(selectedReason);

			return RedirectToAction(nameof(Finish));
		}

		public IActionResult Finish()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
