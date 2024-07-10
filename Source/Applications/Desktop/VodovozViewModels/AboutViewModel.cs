using QS.Commands;
using QS.Navigation;
using QS.Project.Versioning;
using QS.Project.Versioning.Product;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vodovoz.Settings.Common;

namespace Vodovoz.ViewModels
{
	public class AboutViewModel : WindowDialogViewModelBase
	{
		private readonly IApplicationInfo _applicationInfo;

		public AboutViewModel(
			INavigationManager navigationManager,
			IApplicationInfo applicationInfo,
			IWikiSettings wikiSettings,
			IProductService productService = null)
			: base(navigationManager)
		{
			if(wikiSettings is null)
			{
				throw new ArgumentNullException(nameof(wikiSettings));
			}

			WindowPosition = QS.Dialog.WindowGravity.None;
			Resizable = false;

			Title = "О программе";

			_applicationInfo = applicationInfo
				?? throw new ArgumentNullException(nameof(applicationInfo));

			ProgramName = $"{_applicationInfo.ProductTitle} " +
			(!_applicationInfo.ModificationIsHidden && !string.IsNullOrEmpty(_applicationInfo.ModificationTitle)
			? _applicationInfo.ModificationTitle
			: string.Empty);

			Version = _applicationInfo.Version.VersionToShortString() +
			(!_applicationInfo.ModificationIsHidden && string.IsNullOrEmpty(_applicationInfo.ModificationTitle) && !string.IsNullOrWhiteSpace(_applicationInfo.Modification)
				? $"-{_applicationInfo.Modification}"
				: string.Empty);

			BuildTimeFormattedDate = GetDateTimeFGromVersion(_applicationInfo.Version)
				.ToString("dd.MM.yyyy HH:mm");

			var description = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
			var support = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblySupportAttribute>()?.SupportInfo;

			var text = new List<string>();

			if(productService?.CurrentEditionName != null)
			{
				text.Add(productService?.CurrentEditionName);
			}

			if(_applicationInfo.IsBeta)
			{
				text.Add($"Бета от {BuildTimeFormattedDate}");
			}

			if(string.IsNullOrWhiteSpace(description))
			{
				text.Add(description);
			}

			if(string.IsNullOrWhiteSpace(support))
			{
				text.Add(support);
			}

			Description = string.Join("\n", text);

			Copyright = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

			var authorAttributes = Assembly.GetEntryAssembly().GetCustomAttributes<AssemblyAuthorAttribute>();

			Authors = authorAttributes
				.Select(x => string.Join(" ", x.Name, x.Email, x.YearsOfActivity))
				.Reverse()
				.ToArray();

			WikiWebsite = wikiSettings.Url;

			Website = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyAppWebsiteAttribute>()?.Link;

			OpenAuthorsCommand = new DelegateCommand(OpenAuthors);
			CloseCommand = new DelegateCommand(Close);
		}

		public DelegateCommand OpenAuthorsCommand { get; }
		public DelegateCommand CloseCommand { get; }

		public string ProgramName { get; }
		public string ProgramNameFormatted => $"<span size=\"xx-large\" weight=\"bold\">{ProgramName}</span>";
		public string Version { get; }
		public string Description { get; }
		public string Copyright { get; }
		public string[] Authors { get; }

		public string WikiWebsite { get; }
		public string WikiWebsiteFormatted => $"База знаний по работе с программой: <a href=\"{WikiWebsite}\">{WikiWebsite}</a>";

		public string Website { get; }
		public string WebsiteFormatted => $"<a href=\"{Website}\">{Website}</a>";

		public string BuildTimeFormattedDate { get; }

		private void OpenAuthors()
		{
			NavigationManager.OpenViewModel<AboutAuthorsViewModel>(
				this,
				OpenPageOptions.AsSlave,
				viewModel => viewModel.Authors = string.Join("\n", Authors));
		}

		private void Close()
		{
			Close(false, CloseSource.Cancel);
		}

		private DateTime GetDateTimeFGromVersion(Version version) =>
			new DateTime(2000, 1, 1)
				.AddDays(version.Build)
				.AddSeconds(version.Revision * 2);
	}
}
