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
using Newtonsoft.Json;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationResponse" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "DB449144-6045-4B11-AA55-ECF286B117A9" )]
    public partial class CommunicationResponse : Model<CommunicationResponse>
    {
        #region Entity Properties

        /// <summary>
        /// This is the address of the sender communication medium. e.g. A phone number or email address.
        /// It is used when an incoming message cannot be identified with a person, this can be used to
        /// link it up later.
        /// </summary>
        /// <value>
        /// The message key.
        /// </value>
        [Required]
        [MaxLength( 1000 )]
        [DataMember( IsRequired = true )]
        public string MessageKey { get; set; }

        /// <summary>
        /// Gets or sets from person alias identifier.
        /// </summary>
        /// <value>
        /// From person alias identifier.
        /// </value>
        [DataMember]
        public int? FromPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets to person alias identifier.
        /// </summary>
        /// <value>
        /// To person alias identifier.
        /// </value>
        [DataMember]
        public int? ToPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is read; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets the related SMS from defined value identifier.
        /// </summary>
        /// <value>
        /// The related SMS from defined value identifier.
        /// </value>
        [DataMember]
        [Obsolete( "Use RelatedSmsFromSystemPhoneNumberId instead." )]
        [RockObsolete( "1.15" )]
        public int? RelatedSmsFromDefinedValueId { get; set; }

        /// <summary>
        /// Gets or sets the related SMS system phone number identifier this
        /// response was received on.
        /// </summary>
        /// <value>
        /// The related SMS system phone number identifier this response was recieved on.
        /// </value>
        [DataMember]
        public int? RelatedSmsFromSystemPhoneNumberId { get; set; }

        /// <summary>
        /// Gets or sets the related communication identifier.
        /// </summary>
        /// <value>
        /// The related communication identifier.
        /// </value>
        [DataMember]
        public int? RelatedCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the related transport entity type identifier.
        /// </summary>
        /// <value>
        /// The related transport entity type identifier.
        /// </value>
        [DataMember]
        public int RelatedTransportEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the related medium entity type identifier.
        /// </summary>
        /// <value>
        /// The related medium entity type identifier.
        /// </value>
        [DataMember]
        public int RelatedMediumEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        [DataMember]
        public string Response { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets from person alias.
        /// </summary>
        /// <value>
        /// From person alias.
        /// </value>
        public virtual PersonAlias FromPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets to person alias.
        /// </summary>
        /// <value>
        /// To person alias.
        /// </value>
        public virtual PersonAlias ToPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the related SMS system phone number this response was
        /// received on.
        /// </summary>
        /// <value>
        /// The related SMS system phone number this response was recieved on.
        /// </value>
        public virtual SystemPhoneNumber RelatedSmsFromSystemPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the related communication.
        /// </summary>
        /// <value>
        /// The related communication.
        /// </value>
        public virtual Communication RelatedCommunication { get; set; }

        /// <summary>
        /// Gets or sets the related medium.
        /// </summary>
        /// <value>
        /// The related medium.
        /// </value>
        public virtual EntityType RelatedMedium { get; set; }

        /// <summary>
        /// Gets or sets the related transport.
        /// </summary>
        /// <value>
        /// The related transport.
        /// </value>
        public virtual EntityType RelatedTransport { get; set; }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationResponseAttachment> Attachments
        {
            get
            {
                return _attachments ?? ( _attachments = new Collection<CommunicationResponseAttachment>() );
            }

            set
            {
                _attachments = value;
            }
        }

        private ICollection<CommunicationResponseAttachment> _attachments;

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class CommunicationResponseConfiguration : EntityTypeConfiguration<CommunicationResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationResponseConfiguration"/> class.
        /// </summary>
        public CommunicationResponseConfiguration()
        {
            this.HasOptional( r => r.FromPersonAlias ).WithMany().HasForeignKey( r => r.FromPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.ToPersonAlias ).WithMany().HasForeignKey( r => r.ToPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.RelatedSmsFromSystemPhoneNumber ).WithMany().HasForeignKey( r => r.RelatedSmsFromSystemPhoneNumberId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.RelatedCommunication ).WithMany().HasForeignKey( c => c.RelatedCommunicationId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.RelatedMedium ).WithMany().HasForeignKey( c => c.RelatedMediumEntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( c => c.RelatedTransport ).WithMany().HasForeignKey( c => c.RelatedTransportEntityTypeId ).WillCascadeOnDelete( false );
        }
    }
    #endregion Entity Configuration
}
