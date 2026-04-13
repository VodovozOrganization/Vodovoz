using System;
using System.Collections.Generic;
using System.Linq;
using TaxcomEdo.Contracts.Xml.Container.Entities.Warrant;
using TaxcomEdo.Contracts.Xml.Container.Entities.Warrants;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Containers
{
	public class NewContainer
	{
		private readonly IList<IDocument> _documents = new List<IDocument>();
		
		public SignMode SignMode { get; private set; }
		public NewContainerWarrant ContainerWarrant { get; private set; }
		
		public IEnumerable<IDocument> Documents => _documents;
		
		public void AddDocument(IDocument document)
		{
			_documents.Add(document);
		}
		
		public void SetWarrantParameters(
			string warrantRegNum,
			string issuerInn,
			string representativeInn,
			DateTime? dateStart = null,
			DateTime? dateEnd = null)
		{
			if(_documents == null || !_documents.Any())
			{
				throw new InvalidOperationException("documents in container are not set!");
			}

			if(SignMode == SignMode.NotSign)
			{
				throw new InvalidOperationException("wrong DocumentSignMode!");
			}

			var warrantParameters = new List<WarrantCardAdditionalParameter>();
			
			if(dateStart != null)
			{
				warrantParameters.Add(WarrantCardAdditionalParameter.Create(WarrantConstants.ValidFrom, dateStart.ToString()));
			}

			if(dateEnd != null)
			{
				warrantParameters.Add(WarrantCardAdditionalParameter.Create(WarrantConstants.ValidTo, dateEnd.ToString()));
			}
			
			var warrantCard = new WarrantCard
			{
				Description = new WarrantCardDescription
				{
					Item = new WarrantCardDescriptionMeta
					{
						Id =  warrantRegNum,
						Issuer = issuerInn,
						Representative = representativeInn,
						Link = WarrantConstants.DefaultLink
					}
				},
				AdditionalData = warrantParameters.ToArray()
			};
			
			ContainerWarrant ??= NewContainerWarrant.Create(Warrant.Create(new []{ warrantCard }));
		}
		
		public static NewContainer Create(SignMode signMode) => new NewContainer
		{
			SignMode = signMode
		};
	}
}
