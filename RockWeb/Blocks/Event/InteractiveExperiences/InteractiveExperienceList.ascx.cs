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

using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event.InteractiveExperiences
{
    [DisplayName( "Interactive Experience List" )]
    [Category( "Event" )]
    [Description( "List Interactive Experiences" )]
    [BlockTypeGuid( "1BC8542B-D2C8-4496-BB5C-EFF4C0E6AE9D" )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    public partial class InteractiveExperienceList : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string InteractiveExperienceId = "InteractiveExperienceId";
        }

        #endregion Page Parameter Keys

        #region UserPreferenceKeys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKey
        {
            /// <summary>
            /// The campus filter.
            /// </summary>
            public const string Campus = "Campus";

            /// <summary>
            /// The include inactive filter.
            /// </summary>
            public const string IncludeInactive = "Include Inactive";
        }

        #endregion UserPreferanceKeys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gExperienceList.DataKeyNames = new string[] { "Id" };
            gExperienceList.Actions.ShowAdd = canAddEditDelete;
            gExperienceList.IsDeleteEnabled = canAddEditDelete;
            gExperienceList.Actions.AddClick += gExperienceList_AddClick;
            gExperienceList.GridRebind += gExperienceList_GridRebind;
            gExperienceList.EntityTypeId = EntityTypeCache.Get<InteractiveExperience>().Id;

            gfExperiences.ApplyFilterClick += gfExperiences_ApplyFilterClick;
            gfExperiences.DisplayFilterValue += gfExperiences_DisplayFilterValue;
            gfExperiences.ClearFilterClick += gfExperiences_ClearFilterClick;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion Base Control Methods

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfExperiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfExperiences_ApplyFilterClick( object sender, EventArgs e )
        {
            gfExperiences.SaveUserPreference( UserPreferenceKey.Campus, cpCampus.SelectedValue );
            gfExperiences.SaveUserPreference( UserPreferenceKey.IncludeInactive, cbShowInactive.Checked ? cbShowInactive.Checked.ToString() : string.Empty );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfExperiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfExperiences_ClearFilterClick( object sender, EventArgs e )
        {
            gfExperiences.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Updates the filter display value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfExperiences_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case UserPreferenceKey.Campus:
                    var campus = CampusCache.Get( cpCampus.SelectedValue.AsGuid() );
                    if ( campus != null )
                    {
                        e.Value = campus.Name;
                    }
                    break;

                case UserPreferenceKey.IncludeInactive:
                    var includeFilterValue = e.Value.AsBooleanOrNull();
                    if ( includeFilterValue.HasValue && includeFilterValue.Value )
                    {
                        e.Value = includeFilterValue.Value.ToYesNo();
                    }
                    break;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gExperienceList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gExperienceList_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gExperienceList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gExperienceList_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.InteractiveExperienceId, 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gExperienceList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gExperienceList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.InteractiveExperienceId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gExperienceList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gExperienceList_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var interactiveExperienceService = new InteractiveExperienceService( rockContext );
                var interactiveExperience = interactiveExperienceService.Get( e.RowKeyId );

                if ( interactiveExperience != null )
                {
                    if ( !interactiveExperienceService.CanDelete( interactiveExperience, out var errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    interactiveExperienceService.Delete( interactiveExperience );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            cpCampus.SetValue( gfExperiences.GetUserPreference( UserPreferenceKey.Campus ) );
            cbShowInactive.Checked = gfExperiences.GetUserPreference( UserPreferenceKey.IncludeInactive ).AsBoolean();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var interactiveExperienceService = new InteractiveExperienceService( rockContext );

            // Use AsNoTracking() since these records won't be modified, and
            // therefore don't need to be tracked by the EF change tracker.
            var qry = interactiveExperienceService.Queryable().AsNoTracking();

            Guid? campusGuid = gfExperiences.GetUserPreference( UserPreferenceKey.Campus ).AsGuidOrNull();
            if ( campusGuid.HasValue )
            {
                qry = qry.Where( ie => ie.InteractiveExperienceSchedules.Any( ies => ies.InteractiveExperienceScheduleCampuses.Any( iesc => iesc.Campus != null && iesc.Campus.Guid == campusGuid.Value ) ) );
            }

            bool includeInactive = gfExperiences.GetUserPreference( UserPreferenceKey.IncludeInactive ).AsBoolean();

            if ( !includeInactive )
            {
                qry = qry.Where( s => s.IsActive == true );
            }

            var experiences = qry.ToList()
                .Select( ie => new
                {
                    ie.Id,
                    ie.Name,
                    NextStartDateTime = GetNextStartDateTime( ie ),
                    ActionCount = ie.InteractiveExperienceActions.Count(),
                    Campus = GetCampusText( ie ),
                    ie.IsActive
                } )
                .AsQueryable();

            var sortProperty = gExperienceList.SortProperty;
            if ( gExperienceList.AllowSorting && sortProperty != null )
            {
                experiences = experiences.Sort( sortProperty );
            }
            else
            {
                experiences = experiences.OrderBy( a => a.Name );
            }

            gExperienceList.EntityTypeId = EntityTypeCache.GetId<InteractiveExperience>();
            gExperienceList.DataSource = experiences.ToList();
            gExperienceList.DataBind();
        }

        private static DateTime? GetNextStartDateTime( InteractiveExperience ie )
        {
            return ie.InteractiveExperienceSchedules
                .Select( ies => ies.Schedule.GetNextStartDateTime( RockDateTime.Now ) )
                .Where( d => d.HasValue )
                .OrderBy( d => d )
                .FirstOrDefault();
        }

        private static string GetCampusText( InteractiveExperience ie )
        {
            var campusIds = ie.InteractiveExperienceSchedules
                .SelectMany( ies => ies.InteractiveExperienceScheduleCampuses )
                .Select( iesc => iesc.CampusId )
                .ToList();

            if ( !campusIds.Any() )
            {
                return "All";
            }

            return campusIds.Select( id => CampusCache.Get( id ) )
                .Where( c => c != null )
                .OrderBy( c => c.Name )
                .Select( c => c.Name )
                .JoinStrings( ", " );
        }

        #endregion
    }
}
