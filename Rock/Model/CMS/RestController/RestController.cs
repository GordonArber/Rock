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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// </summary>
    [RockDomain( "CMS" )]
    [NotAudited]
    [Table( "RestController" )]
    [DataContract]
    [CodeGenerateRest( Enums.CodeGenerateRestEndpoint.ReadOnly, DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "65CDFD5B-A9AA-48FA-8D22-669612D5EA7D")]
    public partial class RestController : Model<RestController>, ICacheable, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the class name.
        /// </summary>
        /// <value>
        /// The class name.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string ClassName { get; set; }

        /// <inheritdoc/>
        [RockInternal( "17.0" )]
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        [DataMember]
        public virtual ICollection<RestAction> Actions { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RestController"/> class.
        /// </summary>
        public RestController()
        {
            Actions = new Collection<RestAction>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this RestController.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this RestController.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Defined Type Configuration class.
    /// </summary>
    public partial class RestControllerConfiguration : EntityTypeConfiguration<RestController>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestControllerConfiguration"/> class.
        /// </summary>
        public RestControllerConfiguration()
        {
        }
    }

    #endregion
}
