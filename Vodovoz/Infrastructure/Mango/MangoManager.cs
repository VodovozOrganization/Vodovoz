using System;
using System.Threading;
using ClientMangoService;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Utilities;
using Vodovoz.Infrastructure.Services;

namespace Vodovoz.Infrastructure.Mango
{
	public class MangoManager
	{
		private readonly Gtk.Action toolbarIcon;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IEmployeeService employeeService;
		private readonly IUserService userService;
		private ConnectionState connectionState;
		private uint extension;
		private MangoNotificationClient NotificationClient;
		private CancellationTokenSource NotifacationCancellation = new CancellationTokenSource();

		public ConnectionState ConnectionState {
			get => connectionState; private set {
				connectionState = value;
				var iconName = $"phone-{value.ToString().ToLower()}";
				toolbarIcon.StockId = iconName;
				if(ConnectionState == ConnectionState.Connected || ConnectionState == ConnectionState.Disconnected)
					toolbarIcon.ShortLabel = extension.ToString();
				else
					toolbarIcon.ShortLabel = "Mango";
				GtkHelper.WaitRedraw();
			}
		}

		public MangoManager(Gtk.Action toolbarIcon, IUnitOfWorkFactory unitOfWorkFactory, IEmployeeService employeeService, IUserService userService)
		{
			this.toolbarIcon = toolbarIcon;
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		public void Connect()
		{
			using(var uow = unitOfWorkFactory.CreateWithoutRoot("MangoManager Connect")) {
				var employee = employeeService.GetEmployeeForUser(uow, userService.CurrentUserId);
				if(employee?.InnerPhone == null) {
					ConnectionState = ConnectionState.Disable;
					return;
				}

				extension = employee.InnerPhone.Value;
				ConnectionState = ConnectionState.Disconnected;
				NotificationClient = new MangoNotificationClient(extension, NotifacationCancellation.Token);
			}
		}
	}

	public enum ConnectionState
	{
		Connected,
		Disable,
		Disconnected,
		Ring,
		Talk
	}
}
