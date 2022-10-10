// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail;

namespace Rock.Blocks.Event.InteractiveExperiences
{
    /// <summary>
    /// Displays the details of a particular interactive experience.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianDetailBlockType" />

    [DisplayName( "Interactive Experience Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a particular interactive experience." )]
    [IconCssClass( "fa fa-question" )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "e9e76f40-3e00-40e1-bd9d-3156e9208557" )]
    [Rock.SystemGuid.BlockTypeGuid( "dc997692-3bb4-470c-a2ee-83cb87d246b1" )]
    public class InteractiveExperienceDetail : RockObsidianDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string InteractiveExperienceId = "InteractiveExperienceId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        public override string BlockFileUrl => $"{base.BlockFileUrl}.vue";

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = GetAttributeQualifiedColumns<InteractiveExperience>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private InteractiveExperienceDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new InteractiveExperienceDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the InteractiveExperience for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="interactiveExperience">The InteractiveExperience to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the InteractiveExperience is valid, <c>false</c> otherwise.</returns>
        private bool ValidateInteractiveExperience( InteractiveExperience interactiveExperience, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {InteractiveExperience.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( InteractiveExperience.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( InteractiveExperience.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="InteractiveExperienceBag"/> that represents the entity.</returns>
        private InteractiveExperienceBag GetCommonEntityBag( InteractiveExperience entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new InteractiveExperienceBag
            {
                IdKey = entity.IdKey,
                ActionBackgroundColor = entity.ActionBackgroundColor,
                ActionBackgroundImageBinaryFile = entity.ActionBackgroundImageBinaryFile.ToListItemBag(),
                ActionCustomCss = entity.ActionCustomCss,
                ActionPrimaryButtonColor = entity.ActionPrimaryButtonColor,
                ActionPrimaryButtonTextColor = entity.ActionPrimaryButtonTextColor,
                ActionSecondaryButtonColor = entity.ActionSecondaryButtonColor,
                ActionSecondaryButtonTextColor = entity.ActionSecondaryButtonTextColor,
                ActionTextColor = entity.ActionTextColor,
                AudienceAccentColor = entity.AudienceAccentColor,
                AudienceBackgroundColor = entity.AudienceBackgroundColor,
                AudienceBackgroundImageBinaryFile = entity.AudienceBackgroundImageBinaryFile.ToListItemBag(),
                AudienceCustomCss = entity.AudienceCustomCss,
                AudiencePrimaryColor = entity.AudiencePrimaryColor,
                AudienceSecondaryColor = entity.AudienceSecondaryColor,
                AudienceTextColor = entity.AudienceTextColor,
                Description = entity.Description,
                //InteractiveExperienceActions = entity.InteractiveExperienceActions.ToListItemBagList(),
                IsActive = entity.IsActive,
                Name = entity.Name,
                NoActionHeaderImageBinaryFile = entity.NoActionHeaderImageBinaryFile.ToListItemBag(),
                NoActionMessage = entity.NoActionMessage,
                NoActionTitle = entity.NoActionTitle,
                PhotoBinaryFile = entity.PhotoBinaryFile.ToListItemBag(),
                PublicLabel = entity.PublicLabel,
                PushNotificationType = entity.PushNotificationType,
                WelcomeHeaderImageBinaryFile = entity.WelcomeHeaderImageBinaryFile.ToListItemBag(),
                WelcomeMessage = entity.WelcomeMessage,
                WelcomeTitle = entity.WelcomeTitle
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="InteractiveExperienceBag"/> that represents the entity.</returns>
        private InteractiveExperienceBag GetEntityBagForView( InteractiveExperience entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="InteractiveExperienceBag"/> that represents the entity.</returns>
        private InteractiveExperienceBag GetEntityBagForEdit( InteractiveExperience entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( InteractiveExperience entity, DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.ActionBackgroundColor ),
                () => entity.ActionBackgroundColor = box.Entity.ActionBackgroundColor );

            box.IfValidProperty( nameof( box.Entity.ActionBackgroundImageBinaryFile ),
                () => entity.ActionBackgroundImageBinaryFileId = box.Entity.ActionBackgroundImageBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.ActionCustomCss ),
                () => entity.ActionCustomCss = box.Entity.ActionCustomCss );

            box.IfValidProperty( nameof( box.Entity.ActionPrimaryButtonColor ),
                () => entity.ActionPrimaryButtonColor = box.Entity.ActionPrimaryButtonColor );

            box.IfValidProperty( nameof( box.Entity.ActionPrimaryButtonTextColor ),
                () => entity.ActionPrimaryButtonTextColor = box.Entity.ActionPrimaryButtonTextColor );

            box.IfValidProperty( nameof( box.Entity.ActionSecondaryButtonColor ),
                () => entity.ActionSecondaryButtonColor = box.Entity.ActionSecondaryButtonColor );

            box.IfValidProperty( nameof( box.Entity.ActionSecondaryButtonTextColor ),
                () => entity.ActionSecondaryButtonTextColor = box.Entity.ActionSecondaryButtonTextColor );

            box.IfValidProperty( nameof( box.Entity.ActionTextColor ),
                () => entity.ActionTextColor = box.Entity.ActionTextColor );

            box.IfValidProperty( nameof( box.Entity.AudienceAccentColor ),
                () => entity.AudienceAccentColor = box.Entity.AudienceAccentColor );

            box.IfValidProperty( nameof( box.Entity.AudienceBackgroundColor ),
                () => entity.AudienceBackgroundColor = box.Entity.AudienceBackgroundColor );

            box.IfValidProperty( nameof( box.Entity.AudienceBackgroundImageBinaryFile ),
                () => entity.AudienceBackgroundImageBinaryFileId = box.Entity.AudienceBackgroundImageBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.AudienceCustomCss ),
                () => entity.AudienceCustomCss = box.Entity.AudienceCustomCss );

            box.IfValidProperty( nameof( box.Entity.AudiencePrimaryColor ),
                () => entity.AudiencePrimaryColor = box.Entity.AudiencePrimaryColor );

            box.IfValidProperty( nameof( box.Entity.AudienceSecondaryColor ),
                () => entity.AudienceSecondaryColor = box.Entity.AudienceSecondaryColor );

            box.IfValidProperty( nameof( box.Entity.AudienceTextColor ),
                () => entity.AudienceTextColor = box.Entity.AudienceTextColor );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            //box.IfValidProperty( nameof( box.Entity.InteractiveExperienceActions ),
            //    () => entity.InteractiveExperienceActions = box.Entity./* TODO: Unknown property type 'ICollection<InteractiveExperienceAction>' for conversion to bag. */ );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.NoActionHeaderImageBinaryFile ),
                () => entity.NoActionHeaderImageBinaryFileId = box.Entity.NoActionHeaderImageBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.NoActionMessage ),
                () => entity.NoActionMessage = box.Entity.NoActionMessage );

            box.IfValidProperty( nameof( box.Entity.NoActionTitle ),
                () => entity.NoActionTitle = box.Entity.NoActionTitle );

            box.IfValidProperty( nameof( box.Entity.PhotoBinaryFile ),
                () => entity.PhotoBinaryFileId = box.Entity.PhotoBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.PublicLabel ),
                () => entity.PublicLabel = box.Entity.PublicLabel );

            box.IfValidProperty( nameof( box.Entity.PushNotificationType ),
                () => entity.PushNotificationType = box.Entity.PushNotificationType );

            box.IfValidProperty( nameof( box.Entity.WelcomeHeaderImageBinaryFile ),
                () => entity.WelcomeHeaderImageBinaryFileId = box.Entity.WelcomeHeaderImageBinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.WelcomeMessage ),
                () => entity.WelcomeMessage = box.Entity.WelcomeMessage );

            box.IfValidProperty( nameof( box.Entity.WelcomeTitle ),
                () => entity.WelcomeTitle = box.Entity.WelcomeTitle );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="InteractiveExperience"/> to be viewed or edited on the page.</returns>
        private InteractiveExperience GetInitialEntity( RockContext rockContext )
        {
            var entity = GetInitialEntity<InteractiveExperience, InteractiveExperienceService>( rockContext, PageParameterKey.InteractiveExperienceId );

            if ( entity.Id == 0 )
            {
                entity.IsActive = true;
            }

            return entity;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( InteractiveExperience entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out InteractiveExperience entity, out BlockActionResult error )
        {
            var entityService = new InteractiveExperienceService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new InteractiveExperience
                {
                    IsActive = true
                };
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{InteractiveExperience.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${InteractiveExperience.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new InteractiveExperienceService( rockContext );
                var binaryFileService = new BinaryFileService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Record all the old binary file identifiers in case any
                // get deleted by the update operation.
                var oldBinaryFileIds = new List<int>()
                {
                    entity.ActionBackgroundImageBinaryFileId ?? 0,
                    entity.AudienceBackgroundImageBinaryFileId ?? 0,
                    entity.NoActionHeaderImageBinaryFileId ?? 0,
                    entity.PhotoBinaryFileId ?? 0,
                    entity.WelcomeHeaderImageBinaryFileId ?? 0
                }.Where( fileId => fileId != 0 ).ToList();


                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateInteractiveExperience( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;
                var binaryFileIds = new List<int>
                    {
                        entity.ActionBackgroundImageBinaryFileId ?? 0,
                        entity.AudienceBackgroundImageBinaryFileId ?? 0,
                        entity.NoActionHeaderImageBinaryFileId ?? 0,
                        entity.PhotoBinaryFileId ?? 0,
                        entity.WelcomeHeaderImageBinaryFileId ?? 0
                    }.Where( fileId => fileId != 0 ).ToList();
                var removedBinaryFileIds = oldBinaryFileIds.Except( binaryFileIds ).ToList();

                // Ensure all the current binary files are marked as permanent.
                binaryFileService.Queryable()
                    .Where( bf => binaryFileIds.Contains( bf.Id ) && bf.IsTemporary )
                    .ToList()
                    .ForEach( bf => bf.IsTemporary = false );

                // Ensure all the removed binary files are marked as temporary.
                binaryFileService.Queryable()
                    .Where( bf => removedBinaryFileIds.Contains( bf.Id ) && !bf.IsTemporary )
                    .ToList()
                    .ForEach( bf => bf.IsTemporary = true );

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.InteractiveExperienceId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity ) );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new InteractiveExperienceService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<InteractiveExperienceBag, InteractiveExperienceDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}
