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
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// A collection of group and groupTypeRole values used to sync a group's roles to a dataview.
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupSync" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( "1C011499-1122-4429-9AFA-6578798E18A9")]
    public partial class GroupSync : Model<GroupSync>
    {
        #region Entity Properties
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_GroupIdGroupTypeRoleId", 0, IsUnique = true )]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupTypeRole"/> identifier.
        /// </summary>
        /// <value>
        /// The group type role identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_GroupIdGroupTypeRoleId", 1, IsUnique = true )]
        public int GroupTypeRoleId { get; set; }

        /// <summary>
        /// Gets or sets the synchronize <see cref="Rock.Model.DataView"/> identifier.
        /// </summary>
        /// <value>
        /// The synchronize data view identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int SyncDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the welcome system email identifier.
        /// </summary>
        /// <value>
        /// The welcome system email identifier.
        /// </value>
        [DataMember]
        public int? WelcomeSystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the welcome system email identifier.
        /// </summary>
        /// <value>
        /// The welcome system email identifier.
        /// </value>
        [DataMember]
        [Obsolete( "Use WelcomeSystemCommunicationId instead.", true )]
        [RockObsolete( "1.10" )]
        public int? WelcomeSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets the exit <see cref="Rock.Model.SystemCommunication"/> identifier.
        /// </summary>
        /// <value>
        /// The exit system communication identifier.
        /// </value>
        [DataMember]
        public int? ExitSystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the exit system email identifier.
        /// </summary>
        /// <value>
        /// The exit system email identifier.
        /// </value>
        [DataMember]
        [Obsolete( "Use ExitSystemCommunicationId instead.", true )]
        [RockObsolete( "1.10" )]
        public int? ExitSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [add user accounts during synchronize].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [add user accounts during synchronize]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AddUserAccountsDuringSync { get; set; }

        /// <summary>
        /// Gets or sets the schedule interval minutes.
        /// </summary>
        /// <value>
        /// The schedule interval minutes.
        /// </value>
        [DataMember]
        public int? ScheduleIntervalMinutes { get; set; }

        /// <summary>
        /// Gets or sets the last refresh date time.
        /// </summary>
        /// <value>
        /// The last refresh date time.
        /// </value>
        [DataMember]
        public DateTime? LastRefreshDateTime { get; set; }

        #endregion
        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupTypeRole"/>.
        /// </summary>
        /// <value>
        /// The group type role.
        /// </value>
        [DataMember]
        public virtual GroupTypeRole GroupTypeRole { get; set; }

        /// <summary>
        /// Gets or sets the syncDataview.
        /// </summary>
        /// <value>
        /// The synchronize data view.
        /// </value>
        [DataMember]
        public virtual Rock.Model.DataView SyncDataView { get; set; }

        /// <summary>
        /// Gets or sets the welcome system email.
        /// </summary>
        /// <value>
        /// The welcome system email.
        /// </value>
        [DataMember]
        [Obsolete( "Use WelcomeSystemCommunication instead.", true )]
        [RockObsolete( "1.10" )]
        public virtual Rock.Model.SystemEmail WelcomeSystemEmail { get; set; }

        /// <summary>
        /// Gets or sets the exit system email.
        /// </summary>
        /// <value>
        /// The exit system email.
        /// </value>
        [DataMember]
        [Obsolete( "Use ExitSystemCommunication instead.", true )]
        [RockObsolete( "1.10" )]
        public virtual SystemEmail ExitSystemEmail { get; set; }

        /// <summary>
        /// Gets or sets the welcome <see cref="Rock.Model.SystemCommunication"/>.
        /// </summary>
        /// <value>
        /// The welcome system communication.
        /// </value>
        [DataMember]
        public virtual SystemCommunication WelcomeSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the exit <see cref="Rock.Model.SystemCommunication"/>.
        /// </summary>
        /// <value>
        /// The exit system communication.
        /// </value>
        [DataMember]
        public virtual SystemCommunication ExitSystemCommunication { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// GroupSync configuration class
    /// </summary>
    public partial class GroupSyncConfiguration : EntityTypeConfiguration<GroupSync>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupSyncConfiguration"/> class.
        /// </summary>
        public GroupSyncConfiguration()
        {
            HasRequired( g => g.Group ).WithMany( g => g.GroupSyncs ).HasForeignKey( g => g.GroupId ).WillCascadeOnDelete( true );
            HasRequired( g => g.GroupTypeRole ).WithMany().HasForeignKey( g => g.GroupTypeRoleId ).WillCascadeOnDelete( false );
            HasRequired( g => g.SyncDataView ).WithMany().HasForeignKey( g => g.SyncDataViewId ).WillCascadeOnDelete( false );
            HasOptional( g => g.WelcomeSystemCommunication ).WithMany().HasForeignKey( g => g.WelcomeSystemCommunicationId ).WillCascadeOnDelete( false );
            HasOptional( g => g.ExitSystemCommunication ).WithMany().HasForeignKey( g => g.ExitSystemCommunicationId ).WillCascadeOnDelete( false );
        }
    }
    #endregion
}
