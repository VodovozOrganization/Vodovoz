using System;
using System.Linq;
using QS.Commands;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.Organization
{
	public class SubdivisionViewModel : EntityTabViewModelBase<Subdivision>
	{
		public SubdivisionViewModel(IEntityUoWBuilder ctorParam, ICommonServices commonServices) : base(ctorParam, commonServices)
		{
			ConfigureEntityChangingRelations();
			CreateCommands();
		}

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(e => e.GeographicGroup,
				() => GeographicGroup
			);

			SetPropertyChangeRelation(e => e.ParentSubdivision,
				() => GeographicGroupVisible
			);

			Entity.ObservableChildSubdivisions.ElementAdded += (aList, aIdx) => OnPropertyChanged(() => GeographicGroupVisible);
			Entity.ObservableChildSubdivisions.ElementRemoved += (aList, aIdx, aObject) => OnPropertyChanged(() => GeographicGroupVisible);
		}

		public bool CanEdit => PermissionResult.CanUpdate;

		public bool GeographicGroupVisible => Entity.ParentSubdivision != null && Entity.ChildSubdivisions.Any();

		public virtual GeographicGroup GeographicGroup {
			get => Entity.GeographicGroup;
			set {
				if(Entity.GeographicGroup == value) {
					return;
				}
				Entity.GeographicGroup = value;
				Entity.SetChildsGeographicGroup(Entity.GeographicGroup);
			}
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAddDocumentTypeCommand();
			CreateDeleteDocumentCommand();
		}

		#region AddDocumentTypeCommand

		public DelegateCommand<TypeOfEntity> AddDocumentTypeCommand { get; private set; }

		private void CreateAddDocumentTypeCommand()
		{
			AddDocumentTypeCommand = new DelegateCommand<TypeOfEntity>(
				(docType) => Entity.AddDocumentType(docType),
				(docType) => docType != null && CanEdit
			);
		}

		#endregion AddDocumentTypeCommand


		#region DeleteDocumentCommand

		public DelegateCommand<TypeOfEntity> DeleteDocumentCommand { get; private set; }

		private void CreateDeleteDocumentCommand()
		{
			DeleteDocumentCommand = new DelegateCommand<TypeOfEntity>(
				(docType) => Entity.DeleteDocumentType(docType),
				(docType) => docType != null && CanEdit
			);
		}

		#endregion DeleteDocumentCommand

		#endregion Commands
	}
}
