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
using System.Data.Entity;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    public partial class InteractiveExperienceOccurrenceService
    {
        /// <summary>
        /// Gets the current occurrence identifier for the schedule in relation
        /// to the given campus. If no occurrence exists but one should exist based
        /// on the schedule then a new occurrence is created.
        /// </summary>
        /// <param name="interactiveExperienceScheduleId">The interactive experience schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns>An integer that represents the matching occurrence identifier or <c>null</c> if an occurrence should not be active right now.</returns>
        internal static int? GetOrCreateCurrentOccurrenceId( int interactiveExperienceScheduleId, int? campusId )
        {
            using ( var rockContext = new RockContext() )
            {
                var interactiveExperienceScheduleService = new InteractiveExperienceScheduleService( rockContext );

                // Load the experience schedule and the calendar schedule.
                var experienceSchedule = interactiveExperienceScheduleService.Queryable()
                    .Include( ies => ies.Schedule )
                    .Where( ies => ies.Id == interactiveExperienceScheduleId && ies.InteractiveExperience.IsActive )
                    .SingleOrDefault();

                if ( experienceSchedule == null )
                {
                    return null;
                }

                var occurrenceDateTime = GetCurrentOccurrenceDateTime( experienceSchedule.Schedule );

                if ( !occurrenceDateTime.HasValue )
                {
                    return null;
                }

                // We have a valid occurrence date and time. Get or create
                // the occurrence for that date and time.
                return GetOrCreateCurrentOccurrenceId( interactiveExperienceScheduleId, campusId, occurrenceDateTime.Value, rockContext );
            }
        }

        /// <summary>
        /// Gets the current occurrence identifier for the schedule, campus and
        /// date-time. If no occurrence exists then a new occurrence is created.
        /// </summary>
        /// <param name="interactiveExperienceScheduleId">The interactive experience schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="occurrenceDateTime">The occurrence date time.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>An integer that represents the matching occurrence identifier.</returns>
        private static int GetOrCreateCurrentOccurrenceId( int interactiveExperienceScheduleId, int? campusId, DateTime occurrenceDateTime, RockContext rockContext )
        {
            var occurrenceService = new InteractiveExperienceOccurrenceService( rockContext );

            var occurrenceId = occurrenceService.GetExistingOccurrenceId( interactiveExperienceScheduleId, campusId, occurrenceDateTime );

            if ( occurrenceId.HasValue )
            {
                return occurrenceId.Value;
            }

            // An existing occurrence wasn't found, create a new one.
            var occurrence = new InteractiveExperienceOccurrence
            {

                InteractiveExperienceScheduleId = interactiveExperienceScheduleId,
                CampusId = campusId,
                OccurrenceDateTime = occurrenceDateTime,
            };

            occurrenceService.Add( occurrence );

            try
            {
                rockContext.SaveChanges();

                return occurrence.Id;
            }
            catch ( Exception ex )
            {
                // If an exception happens, assume it was a unique constraint
                // violation. Meaning, another thread or server created the
                // occurrence after we checked for it. Try to load again.
                occurrenceId = occurrenceService.GetExistingOccurrenceId( interactiveExperienceScheduleId, campusId, occurrenceDateTime );

                if ( occurrenceId.HasValue )
                {
                    return occurrenceId.Value;
                }

                throw new Exception( "Failed to load or create InteractiveExperienceOccurrence", ex );
            }
        }

        /// <summary>
        /// Gets the existing occurrence identifier for the specified schedule,
        /// campus and date-time.
        /// </summary>
        /// <param name="interactiveExperienceScheduleId">The interactive experience schedule identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="occurrenceDateTime">The occurrence date time.</param>
        /// <returns>An integer that represents the matching occurrence identifier or <c>null</c> if an occurrence was not found.</returns>
        private int? GetExistingOccurrenceId( int interactiveExperienceScheduleId, int? campusId, DateTime occurrenceDateTime )
        {
            return Queryable()
                .Where( ieo => ieo.InteractiveExperienceScheduleId == interactiveExperienceScheduleId
                    && ( ( !ieo.CampusId.HasValue && !campusId.HasValue ) || ieo.CampusId == campusId )
                    && ieo.OccurrenceDateTime == occurrenceDateTime )
                .OrderBy( ieo => ieo.Id )
                .Select( ieo => ( int? ) ieo.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the current occurrence date time for the schedule. This looks
        /// for a schedule that started in the past and whose duration puts the
        /// end date and time past the current date and time.
        /// </summary>
        /// <param name="schedule">The schedule to use when calculating the occurrence date and time.</param>
        /// <returns>A <see cref="DateTime"/> that represents the occurrence date and time or <c>null</c> if the schedule does not have an active occurrence date and time.</returns>
        private static DateTime? GetCurrentOccurrenceDateTime( Schedule schedule )
        {
            var now = RockDateTime.Now;

            // Look for a schedule that started sometime between midnight today
            // and the current time. The cast to DateTime? ensures we get a null
            // value back, otherwise we get a 1/1/0001 date if nothing found.
            return schedule.GetScheduledStartTimes( now.Date, now )
                .Where( dt => dt >= now && dt.AddMinutes( schedule.DurationInMinutes ) < now )
                .Select( dt => ( DateTime? ) dt )
                .FirstOrDefault();
        }
    }
}