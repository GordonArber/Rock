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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// A Group Member Workflow Trigger defined a workflow that should be triggered to start when certain group member changes are made..
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupMemberWorkflowTrigger" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "3CE3406A-1FFE-4CCA-A8D5-916EEF800D76")]
    public partial class GroupMemberWorkflowTrigger : Entity<GroupMemberWorkflowTrigger>, IOrdered, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if the WorkflowTrigger is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the WorkflowTrigger is active; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember]
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

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
        /// Gets or sets the WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that is executed by this WorkflowTrigger. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing WorkflowTypeId of the <see cref="Rock.Model.WorkflowType"/> that is executed by the WorkflowTrigger.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupMemberWorkflowTriggerType">type</see> of the trigger.
        /// </summary>
        /// <value>
        /// The type of the trigger.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public GroupMemberWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the type qualifier.
        /// </summary>
        /// <value>
        /// The type qualifier.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string TypeQualifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow trigger.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the workflow trigger.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string WorkflowName { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType">type</see> of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [LavaVisible]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        /// <remarks>
        /// NOTE: [DataMember] attribute is intentionally omitted to prevent having to serialize all group members (times out on large groups)
        /// </remarks>
        [LavaVisible]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowType"/> that is executed by this WorkflowTrigger.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowType"/> that is executed by this WorkflowTrigger.
        /// </value>
        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EntityTypeWorkflowTrigger Configuration class.
    /// </summary>
    public partial class GroupMemberWorkflowTriggerConfiguration : EntityTypeConfiguration<GroupMemberWorkflowTrigger>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberWorkflowTriggerConfiguration"/> class.
        /// </summary>
        public GroupMemberWorkflowTriggerConfiguration()
        {
            this.HasOptional( t => t.GroupType ).WithMany( g => g.GroupMemberWorkflowTriggers ).HasForeignKey( t => t.GroupTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( t => t.Group ).WithMany( g => g.GroupMemberWorkflowTriggers ).HasForeignKey( t => t.GroupId ).WillCascadeOnDelete( true );
            this.HasRequired( t => t.WorkflowType ).WithMany().HasForeignKey( t => t.WorkflowTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
