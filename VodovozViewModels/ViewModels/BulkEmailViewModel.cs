using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using NHibernate;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report.Domain;
using QS.Report.Repository;
using QS.Services;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.ViewModels.ViewModels
{
	public class BulkEmailViewModel : DialogViewModelBase, IDisposable
	{
		private DelegateCommand _savePrintSettingsCommand;
		private DelegateCommand<PrinterNode> _openPrinterSettingsCommand;
		private readonly IUnitOfWork _uow;
		private string _subjectText;
		//private readonly IList<DebtorJournalNode> _debtorJournalNodeList;
		

		public BulkEmailViewModel(INavigationManager navigation, IUnitOfWorkFactory unitOfWorkFactory,
			Func<IUnitOfWork, IQueryOver<Order>> itemsSourceQueryFunction) : base(navigation)
		{
			_uow = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory))).CreateWithoutRoot();
			var itemsSourceQuery = itemsSourceQueryFunction.Invoke(_uow);
			//_debtorJournalNodeList = itemsSourceQuery.List<DebtorJournalNode>();
			var counterparties = itemsSourceQuery.List<DebtorJournalNode>();

			var emails = _uow.GetById<Domain.Client.Counterparty>(counterparties.Select(c=>c.ClientId))
				.SelectMany(x => x.Emails)
				.Where(x=>x.EmailType.EmailPurpose == EmailPurpose.Default);

			foreach(var email in emails)
			{
				;
			}
		}


		[PropertyChangedAlso(nameof(SubjectInfo))]
		public string SubjectText
		{
			get => _subjectText;
			set
			{
				SetField(ref _subjectText, value);
			}
		}

		public string SubjectInfo => $"{ SubjectText?.Length ?? 0 }/255 символов";

		#region Commands

		public DelegateCommand SavePrintSettingsCommand =>
			_savePrintSettingsCommand ?? (_savePrintSettingsCommand = new DelegateCommand(() =>
			{
				//var selectedPrinters = AllPrintersWithSelected.Where(x => x.IsChecked).Select(x => x.Printer).ToArray();

				//var printersToDelete = _savedUserPrinterList.Except(selectedPrinters).ToArray();
				//foreach (var printer in printersToDelete)
				//{
				//	var delPrinter = AllPrintersWithSelected.Single(x => x.Printer.Name == printer.Name);
				//	delPrinter.Printer = new UserSelectedPrinter
				//	{
				//		Name = printer.Name,
				//		User = _user
				//	};
				//	_savedUserPrinterList.Remove(printer);
				//	_uow.Delete(printer);
				//}

				//var printersToAdd = selectedPrinters.Except(_savedUserPrinterList).ToArray();
				//foreach (var printer in printersToAdd)
				//{
				//	_savedUserPrinterList.Add(printer);
				//	_uow.Save(printer);
				//}

				//_uow.Save(UserPrintSettings);
				//_uow.Commit();
			},
				() => true
			));

		public DelegateCommand<PrinterNode> OpenPrinterSettingsCommand =>
			_openPrinterSettingsCommand ?? (_openPrinterSettingsCommand = new DelegateCommand<PrinterNode>((selectedItem) =>
			{
				if(selectedItem != null)
				{
					string printerName = selectedItem.Printer.Name;
					System.Diagnostics.Process process = new System.Diagnostics.Process();
					System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
					{
						WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
						FileName = "cmd.exe",
						Arguments = $"/C rundll32.exe printui.dll,PrintUIEntry /e /n \"{ printerName }\""
					};
					process.StartInfo = startInfo;
					process.Start();
				}
			},
				(selectedItem) => IsWindowsOs
			));

		public DelegateCommand<PrinterNode> SubjectInfoChangeCommand =>
			_openPrinterSettingsCommand ?? (_openPrinterSettingsCommand = new DelegateCommand<PrinterNode>((selectedItem) =>
				{
					if(selectedItem != null)
					{
						string printerName = selectedItem.Printer.Name;
						System.Diagnostics.Process process = new System.Diagnostics.Process();
						System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
						{
							WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
							FileName = "cmd.exe",
							Arguments = $"/C rundll32.exe printui.dll,PrintUIEntry /e /n \"{ printerName }\""
						};
						process.StartInfo = startInfo;
						process.Start();
					}
				},
				(selectedItem) => IsWindowsOs
			));

		#endregion

		public IEnumerable<PrinterNode> AllPrintersWithSelected { get; }
		public UserPrintSettings UserPrintSettings { get; }
		public bool IsWindowsOs => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		public class PrinterNode
		{
			public bool IsChecked { get; set; }
			public UserSelectedPrinter Printer { get; set; }
		}

		public void Dispose()
		{
			_uow?.Dispose();
		}
	}

}
