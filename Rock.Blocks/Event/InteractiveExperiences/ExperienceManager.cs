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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.ExperienceManager;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event.InteractiveExperiences
{
    /// <summary>
    /// Displays a list of interactive experience occurrences for the user to pick from.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianDetailBlockType" />

    [DisplayName( "Experience Manager" )]
    [Category( "Event > Interactive Experiences" )]
    [Description( "Manages an active interactive experience." )]
    [IconCssClass( "fa fa-question" )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "5d2594d9-2695-41be-880c-966ff25bcf11" )]
    [Rock.SystemGuid.BlockTypeGuid( "7af57181-dd9a-446a-b321-abad900df9bc" )]
    public class ExperienceManager : RockObsidianBlockType
    {
        #region Keys

        private static class AttributeKey
        {
        }

        private static class PageParameterKey
        {
            public const string InteractiveExperienceOccurrenceId = "InteractiveExperienceOccurrenceId";
        }

        private static class NavigationUrlKey
        {
        }

        #endregion

        public override string BlockFileUrl => $"{base.BlockFileUrl}.vue";

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new ExperienceManagerInitializationBox();
                var occurrence = GetInteractiveExperienceOccurrence( rockContext, PageParameterKey.InteractiveExperienceOccurrenceId );

                if ( occurrence == null )
                {
                    box.ErrorMessage = "Interactive Experience Occurrence was not found.";
                    return box;
                }

                if ( !occurrence.InteractiveExperienceSchedule.InteractiveExperience.IsActive )
                {
                    box.ErrorMessage = "This Interactive Experience is not currently active.";
                    return box;
                }

                box.ExperienceName = occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name;
                box.SecurityGrantToken = GetSecurityGrantToken();
                box.NavigationUrls = GetBoxNavigationUrls();

                return box;
            }
        }

        /// <summary>
        /// Gets the interactive experience entity from page parameters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="InteractiveExperience"/> to be viewed or edited on the page.</returns>
        private InteractiveExperienceOccurrence GetInteractiveExperienceOccurrence( RockContext rockContext, string entityIdKey )
        {
            var entityId = RequestContext.GetPageParameter( entityIdKey );
            var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );

            return occurrenceService.GetQueryableByKey( entityId, !PageCache.Layout.Site.DisablePredictableIds )
                .AsNoTracking()
                .Include( o => o.InteractiveExperienceSchedule.InteractiveExperience )
                .SingleOrDefault();
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            return securityGrant.ToToken();
        }

        #endregion
    }
}
