using QS.Dialog.GtkUI;
using QS.Project.DB;
using QS.Project.Dialogs.GtkUI.ServiceDlg;
using QS.Project.Repositories;
using QS.Project.Services.GtkUI;
using Vodovoz.Additions;
using Vodovoz.Parameters;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Infrastructure.Services;

namespace Vodovoz.Factories
{
	public class AuthorizationServiceFactory : IAuthorizationServiceFactory
	{
		public IAuthorizationService CreateNewAuthorizationService() =>
			new AuthorizationService(
				new PasswordGenerator(),
				new MySQLUserRepository(
					new MySQLProvider(new GtkRunOperationService(), new GtkQuestionDialogsInteractive()), new GtkInteractiveService()),
				new EntityRepositories.UserRepository(),
				new EmailParametersProvider(new ParametersProvider()));
	}
}