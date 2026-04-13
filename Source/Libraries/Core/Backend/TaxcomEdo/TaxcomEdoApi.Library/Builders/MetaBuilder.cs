using System;
using DateTimeHelpers;
using TaxcomEdo.Contracts.Xml.Container;

namespace TaxcomEdoApi.Library.Builders
{
	public class MetaBuilder
	{
		private ContainerDescription _meta = new ContainerDescription();

		public MetaBuilder RequestDateTime(DateTime dateTime)
		{
			_meta.RequestDateTime = dateTime.ToEdoMetaFileDateTimeString();
			return this;
		}
		
		public MetaBuilder IsLast(bool? isLast)
		{
			if(isLast.HasValue)
			{
				_meta.IsLast = isLast.Value;
			}
			return this;
		}
		
		public MetaBuilder LastRecordDateTime(DateTime? dateTime)
		{
			if(dateTime.HasValue)
			{
				_meta.LastRecordDateTime = dateTime.Value.ToEdoMetaFileDateTimeString();
			}
			
			return this;
		}
		
		public MetaBuilder Docflows(ContainerDescriptionDocFlow[] docFlows)
		{
			_meta.DocFlow = docFlows;
			return this;
		}

		public ContainerDescription Build()
		{
			var meta = _meta;
			_meta = new ContainerDescription();
			
			return meta;
		}
		
		public static MetaBuilder Create() => new MetaBuilder();
	}
}
