using System;
using QS.Dialog;
using QS.Dialog.GtkUI;

namespace Vodovoz.Infrastructure.Services
{
	public class GtkInteractiveService : IInteractiveService
	{
		private IInteractiveMessage interactiveMessage;
		public IInteractiveMessage InteractiveMessage {
			get {
				if(interactiveMessage == null) {
					interactiveMessage = new GtkMessageDialogsInteractive();
				}
				return interactiveMessage;
			}
		}

		private IInteractiveQuestion interactiveQuestion;
		public IInteractiveQuestion InteractiveQuestion {
			get {
				if(interactiveQuestion == null) {
					interactiveQuestion = new GtkQuestionDialogsInteractive();
				}
				return interactiveQuestion;
			}
		}
	}
}
