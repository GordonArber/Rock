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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents Session for <see cref="Rock.Model.Interaction">Interaction</see>
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "InteractionSession" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "338025DE-C16F-47BB-BA31-6DE0C59E59AA")]
    public partial class InteractionSession : Model<InteractionSession>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the interaction mode.
        /// </summary>
        /// <value>
        /// The interaction mode.
        /// </value>
        [DataMember]
        [MaxLength(25)]
        public string InteractionMode { get; set; }

        /// <summary>
        /// Gets or sets the interaction session data.
        /// </summary>
        /// <value>
        /// The interaction session data.
        /// </value>
        [DataMember]
        public string SessionData { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionDeviceType"/> device type that that is associated with this Session.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractionDeviceType"/> device type that this session is associated with.
        /// </value>
        [DataMember]
        public int? DeviceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the request.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> of the IP address of the request.
        /// </value>
        [DataMember]
        [MaxLength( 45 )]
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the session start date key which is the form YYYYMMDD.
        /// </summary>
        /// <value>
        /// The session start date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? SessionStartDateKey { get; set; }

        /// <summary>
        /// Gets or set the duration in seconds.
        /// </summary>
        /// <value>
        /// A integer representing the duration in seconds.
        /// </value>
        [DataMember]
        public int? DurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the last date and time when this was calculated.
        /// </summary>
        /// <value>
        /// The last calculated date time.
        /// </value>
        public DateTime? DurationLastCalculatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionSessionLocation"/> Session Location that that is associated with this Interaction Session.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractionSessionLocation"/> session location that this Interaction Session is associated with.
        /// </value>
        [DataMember]
        public int? InteractionSessionLocationId { get; set; }

        /// <summary>
        /// Gets or sets the interaction channel identifier.
        /// </summary>
        /// <value>
        /// The interaction channel identifier.
        /// </value>
        [DataMember]
        public int? InteractionChannelId { get; set; }

        /// <summary>
        /// Gets or sets the Interaction Count.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing Interaction count.
        /// </value>
        [DataMember]
        public int? InteractionCount { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the device type.
        /// </summary>
        /// <value>
        /// The Device type.
        /// </value>
        [LavaVisible]
        public virtual InteractionDeviceType DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the interaction session location.
        /// </summary>
        /// <value>
        /// The interaction session location.
        /// </value>
        [LavaVisible]
        public virtual InteractionSessionLocation InteractionSessionLocation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Interaction">Interactions</see>  for this session.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.Interaction" /> interactions for this session.
        /// </value>
        [DataMember]
        public virtual ICollection<Interaction> Interactions
        {
            get { return _interactions ?? ( _interactions = new Collection<Interaction>() ); }
            set { _interactions = value; }
        }
        private ICollection<Interaction> _interactions;

        /// <summary>
        /// Gets or sets the interaction channel.
        /// </summary>
        /// <value>
        /// The interaction channel.
        /// </value>
        public virtual InteractionChannel InteractionChannel { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class InteractionSessionConfiguration : EntityTypeConfiguration<InteractionSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionSessionConfiguration"/> class.
        /// </summary>
        public InteractionSessionConfiguration()
        {
            this.HasOptional( r => r.DeviceType ).WithMany().HasForeignKey( r => r.DeviceTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.InteractionSessionLocation ).WithMany( r => r.InteractionSessions ).HasForeignKey( r => r.InteractionSessionLocationId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.InteractionChannel ).WithMany( r => r.InteractionSessions ).HasForeignKey( r => r.InteractionChannelId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
