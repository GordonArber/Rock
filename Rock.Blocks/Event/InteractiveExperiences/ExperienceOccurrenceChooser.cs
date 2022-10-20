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
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Event.InteractiveExperiences.ExperienceOccurrenceChooser;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event.InteractiveExperiences
{
    /// <summary>
    /// Displays a list of interactive experience occurrences for the user to pick from.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianDetailBlockType" />

    [DisplayName( "Experience Occurrence Chooser" )]
    [Category( "Event > Interactive Experiences" )]
    [Description( "Displays a list of interactive experience occurrences for the user to pick from." )]
    [IconCssClass( "fa fa-question" )]

    #region Block Attributes

    [LinkedPage( "Experience Manager Page",
        IsRequired = true,
        Key = AttributeKey.ExperienceManagerPage,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "08c31c15-7328-4759-b530-49c9d342cdb7" )]
    [Rock.SystemGuid.BlockTypeGuid( "b8be65ec-04cc-4423-944e-b6b30f6eb38c" )]
    public class ExperienceOccurrenceChooser : RockObsidianBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ExperienceManagerPage = "ExperienceManagerPage";
        }

        private static class PageParameterKey
        {
            public const string InteractiveExperienceId = "InteractiveExperienceId";

            public const string InteractiveExperienceOccurrenceId = "InteractiveExperienceOccurrenceId";
        }

        private static class NavigationUrlKey
        {
            public const string ExperienceManagerPage = "ExperienceManagerPage";
        }

        #endregion

        public override string BlockFileUrl => $"{base.BlockFileUrl}.vue";

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new OccurrenceChooserInitializationBox();
                var experience = GetInteractiveExperience( rockContext, PageParameterKey.InteractiveExperienceId );

                if ( experience == null )
                {
                    box.ErrorMessage = "Interactive Experience was not found.";
                    return box;
                }

                if ( !experience.IsActive )
                {
                    box.ErrorMessage = "This Interactive Experience is not currently active.";
                    return box;
                }

                var occurrences = GetOccurrenceItemBags( experience, rockContext );

                if ( !occurrences.Any() )
                {
                    box.ErrorMessage = "This Interactive Experience does not have any active occurrences.";
                    return box;
                }

                box.ExperienceName = experience.Name;
                box.SecurityGrantToken = GetSecurityGrantToken();
                box.NavigationUrls = GetBoxNavigationUrls();
                box.Occurrences = occurrences;

                return box;
            }
        }

        /// <summary>
        /// Gets the interactive experience entity from page parameters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="InteractiveExperience"/> to be viewed or edited on the page.</returns>
        private InteractiveExperience GetInteractiveExperience( RockContext rockContext, string entityIdKey )
        {
            var entityId = RequestContext.GetPageParameter( entityIdKey );
            var experienceService = new InteractiveExperienceService( rockContext );

            return experienceService.GetNoTracking( entityId, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the occurrence item bags for all the active occurrences.
        /// </summary>
        /// <param name="experience">The experience to use when enumerating occurrences.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A collection of <see cref="ListItemBag"/> objects that represent the occurrences.</returns>
        private static List<ListItemBag> GetOccurrenceItemBags( InteractiveExperience experience, RockContext rockContext )
        {
            var occurrenceIds = InteractiveExperienceService.GetOrCreateAllCurrentOccurrenceIds( experience.Id );

            return new InteractiveExperienceOccurrenceService( rockContext )
                .Queryable()
                .Where( ieo => occurrenceIds.Contains( ieo.Id ) )
                .Select( ieo => new
                {
                    ieo.Id,
                    ieo.CampusId,
                    ieo.OccurrenceDateTime
                } )
                .ToList()
                .Select( ieo => new ListItemBag
                {
                    Value = IdHasher.Instance.GetHash( ieo.Id ),
                    Text = GetOccurrenceTitle( ieo.OccurrenceDateTime, ieo.CampusId )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the occurrence title to use for the specified date, time and campus.
        /// </summary>
        /// <param name="occurrenceDateTime">The occurrence date time.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns>The string that should be used when displaying the occurrence.</returns>
        private static string GetOccurrenceTitle( DateTime occurrenceDateTime, int? campusId )
        {
            string campusName;

            if ( campusId.HasValue )
            {
                campusName = CampusCache.Get( campusId.Value )?.Name ?? "Unknown Campus";
            }
            else
            {
                campusName = "All Campuses";
            }

            return $"{occurrenceDateTime.ToShortDateTimeString()} at {campusName}";
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ExperienceManagerPage] = this.GetLinkedPageUrl( AttributeKey.ExperienceManagerPage, new Dictionary<string, string>
                {
                    [PageParameterKey.InteractiveExperienceOccurrenceId] = "((Id))"
                } )
            };
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
