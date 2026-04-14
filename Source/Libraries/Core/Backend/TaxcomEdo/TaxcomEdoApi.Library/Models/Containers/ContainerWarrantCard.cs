using System;
using System.Collections.Generic;
using TaxcomEdo.Contracts.Xml.Container.Entities.Warrant;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Containers
{
	public class ContainerWarrantCard : IContainerWarrantCard
	{
		public const string ADAP_ValidFromName = "МЧДДействительнаС";
		public const string ADAP_ValidToName = "МЧДДействительнаПо";
		public const string ADAP_ChildWarrantMeta = "МЧДОснованиеКМета";
		public const string ADAP_ChildWarrantFile = "МЧДОснованиеКФайлу";
		public const string ADAP_RepresentativeInn = "Representative";
		
		private IFileData _warrantImage;
		private IList<IFileData> _warrantSignatures;
		private IList<IFileData> _docSigns = new List<IFileData>();

		public WarrantCard RawWarrantCard { get; set; }
		public DateTime? ValidTo { get; set; }
		public DateTime? ValidFrom { get; set; }
		public string ChildFileWarrant { get; set; }
		public string ChildMetaWarrant { get; set; }

		/// <summary>
		/// Файл доверенности(МЧД)
		/// </summary>
		public IFileData WarrantImage
		{
			get => this._warrantImage;
			set => this._warrantImage = value;
		}

		/// <summary>
		/// Подписи, к доверенности
		/// </summary>
		public IList<IFileData> WarrantSignatures
		{
			get => this._warrantSignatures;
			set => this._warrantSignatures = value;
		}

		/// <summary>
		/// Подписи доверенности, если она находится у оператора(прикреплена не в виде файла)
		/// </summary>
		public IEnumerable<IFileData> DocSigns => _docSigns;

		/// <summary>
		/// Мета информация доверенности, если она прикреплена не в виде файла
		/// </summary>
		public IWarrantCardMeta DescriptionMeta { get; set; }

		public ContainerWarrantCard()
		{
		}

		public void AddDocSign(IFileData fileData)
		{
			_docSigns.Add(fileData);
		}

		public interface IWarrantCardMeta
		{
			string Id { get; }

			string Issuer { get; }

			string Representative { get; }

			string Link { get; }
		}

		public class WarrantCardMeta : IWarrantCardMeta
		{
			public string Id { get; set; }

			public string Issuer { get; set; }

			public string Representative { get; set; }

			public string Link { get; set; }
		}
	}
}
