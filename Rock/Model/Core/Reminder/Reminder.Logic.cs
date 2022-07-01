﻿// <copyright>
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net;
using Ical.Net.DataTypes;
using Rock.Lava;
using Rock.Data;
using Rock.Web.Cache;
using Ical.Net.CalendarComponents;

namespace Rock.Model
{
    public partial class Reminder
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this is a renewing reminder.
        /// </summary>
        public bool IsRenewing
        {
            get
            {
                return ( this.RenewMaxCount != null && this.RenewPeriodDays != null );
            }
        }

        /// <summary>
        /// Gets a value indicating whether this reminder has future occurrences.
        /// </summary>
        public bool HasFutureOccurrences
        {
            get
            {
                if ( RenewMaxCount == 0 )
                {
                    return true;
                }

                return ( RenewMaxCount > RenewCurrentCount );
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Marks this reminder as complete and adjusts the reminder date and recurrence count.
        /// </summary>
        public void CompleteReminder()
        {
            var currentDate = RockDateTime.Now;
            this.IsComplete = true;
            if ( this.IsRenewing )
            {
                if ( this.HasFutureOccurrences )
                {
                    this.ReminderDate = this.GetNextDate( currentDate ).Value;
                    this.IsComplete = false;
                }

                this.RenewCurrentCount++;
            }
        }

        /// <summary>
        /// Gets the next reminder date.
        /// Note:  The next date is determined from the time a reminder is completed, not the prior reminder date.
        /// </summary>
        /// <param name="completedDateTime">The current date time.</param>
        /// <returns></returns>
        public DateTime? GetNextDate( DateTime completedDateTime )
        {
            if ( !this.IsRenewing )
            {
                return null;
            }

            if ( !this.HasFutureOccurrences )
            {
                return null;
            }

            return completedDateTime.AddDays( this.RenewPeriodDays.Value );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Reminder: " + this.ReminderType.Name;
        }

        #endregion Public Methods
    }
}
