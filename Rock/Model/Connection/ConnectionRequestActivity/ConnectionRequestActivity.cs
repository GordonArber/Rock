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
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection request activity
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "ConnectionRequestActivity" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.CONNECTION_REQUEST_ACTIVITY )]
    public partial class ConnectionRequestActivity : Model<ConnectionRequestActivity>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionRequest"/> identifier.
        /// </summary>
        /// <value>
        /// The connection request identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [IgnoreCanDelete]
        public int ConnectionRequestId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionActivityType"/> identifier.
        /// </summary>
        /// <value>
        /// The connection activity type identifier.
        /// </value>
        [Required]
        [DataMember]
        public int ConnectionActivityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the connector <see cref="Rock.Model.PersonAlias"/> identifier.
        /// </summary>
        /// <value>
        /// The connector person alias identifier.
        /// </value>
        [DataMember]
        public int? ConnectorPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionOpportunity"/> identifier.
        /// </summary>
        /// <value>
        /// The connection opportunity identifier.
        /// </value>
        [Required]
        [DataMember]
        public int? ConnectionOpportunityId { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        public String Note { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionRequest"/>.
        /// </summary>
        /// <value>
        /// The connection request.
        /// </value>
        [DataMember]
        public virtual ConnectionRequest ConnectionRequest { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionActivityType">type</see> of the connection activity.
        /// </summary>
        /// <value>
        /// The type of the connection activity.
        /// </value>
        [DataMember]
        public virtual ConnectionActivityType ConnectionActivityType { get; set; }

        /// <summary>
        /// Gets or sets the connector <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The connector person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ConnectorPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.ConnectionOpportunity"/>.
        /// </summary>
        /// <value>
        /// The connection opportunity.
        /// </value>
        [DataMember]
        public virtual ConnectionOpportunity ConnectionOpportunity { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionRequestActivity Configuration class.
    /// </summary>
    public partial class ConnectionRequestActivityConfiguration : EntityTypeConfiguration<ConnectionRequestActivity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionRequestActivityConfiguration" /> class.
        /// </summary>
        public ConnectionRequestActivityConfiguration()
        {
            this.HasOptional( p => p.ConnectorPersonAlias ).WithMany().HasForeignKey( p => p.ConnectorPersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ConnectionRequest ).WithMany( p => p.ConnectionRequestActivities ).HasForeignKey( p => p.ConnectionRequestId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ConnectionActivityType ).WithMany().HasForeignKey( p => p.ConnectionActivityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ConnectionOpportunity ).WithMany().HasForeignKey( p => p.ConnectionOpportunityId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
