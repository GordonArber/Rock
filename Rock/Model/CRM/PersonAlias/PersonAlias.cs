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
    /// Represents the merge history for people in Rock. When a person is found to have a duplicate <see cref="Rock.Model.Person"/> entity in the database
    /// the duplicate entities need to be merged together into one record to avoid confusion and to ensure that we have accurate contact, involvement, and 
    /// contribution data. It also helps to avoid situations where an individual is counted or contacted multiple times.
    /// 
    /// The PersonAlias entity is a log containing the merge history (previous Person identifiers) and a pointer to the Person's current Id.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "PersonAlias" )]
    [NotAudited]
    [CodeGenerateRest( ~Enums.CodeGenerateRestEndpoint.DeleteItem, DisableEntitySecurity = true )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.PERSON_ALIAS )]
    public partial class PersonAlias : Entity<PersonAlias>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the alias
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        [Index( IsUnique = true )]
        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the person Id of the <see cref="Rock.Model.Person"/>. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing a person Id of the <see cref="Rock.Model.Person"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int PersonId { get; set; }

        /// <summary>
        /// If <see cref="AliasPersonId"/> equals <see cref="PersonId"/>, this is the PrimaryAlias. Otherwise, the AliasPersonId is the previous person id that was merged into a new person record.
        /// If this is NULL it is special case where the PersonAlias is or was anonymous.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the current/previous Id of the <see cref="Rock.Model.Person"/>.
        /// </value>
        /// <remarks>
        /// This is a Unique Constraint, but we make it a filtered unique constraint
        /// where it only applies to non-null values.
        /// </remarks>
        [Index( IsUnique = true )]
        public int? AliasPersonId { get; set; }

        /// <summary>
        /// If <see cref="AliasPersonId"/> equals <see cref="PersonId"/>, this is the Guid of the Person. Otherwise, the AliasPersonGuid is the previous person's guid that was merged into a new person record.
        /// If this is NULL it is special case where the PersonAlias is or was anonymous.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the current/previous Guid of the <see cref="Rock.Model.Person"/>.
        /// </value>
        public Guid? AliasPersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the aliased date time.
        /// </summary>
        /// <value>
        /// The aliased date time.
        /// </value>
        [DataMember]
        public DateTime? AliasedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last visit time.
        /// </summary>
        /// <value>
        /// The last visit time.
        /// </value>
        [DataMember]
        public DateTime? LastVisitDateTime { get; set; }

        /// <summary>
        /// Gets or sets the internal message. This is used by core Rock to
        /// track internal state messages of the record.
        /// </summary>
        /// <value>The internal message.</value>
        [MaxLength( 250 )]
        [DataMember]
        public string InternalMessage { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [DataMember]
        public virtual Person Person { get; set; }

        /// <summary>
        /// Sets the alias person
        /// NOTE: This is a special case where we want to allow the AliasPersonId to keep the value even if the associated Person is deleted
        /// </summary>
        /// <value>
        /// The alias person.
        /// </value>
        public virtual Person AliasPerson { internal get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Person Alias Configuration class.
    /// </summary>
    public partial class PersonAliasConfiguration : EntityTypeConfiguration<PersonAlias>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAliasConfiguration"/> class.
        /// </summary>
        public PersonAliasConfiguration()
        {
            HasRequired( a => a.Person ).WithMany( p => p.Aliases ).HasForeignKey( a => a.PersonId ).WillCascadeOnDelete( false );

            // NOTE: The foreign key is a fake foreign key (not a physical foreign key in the database) 
            // since we want to keep the AliasPersonId even if the associated Person is deleted
            HasOptional( a => a.AliasPerson ).WithMany().HasForeignKey( a => a.AliasPersonId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
