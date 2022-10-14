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
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Event.InteractiveExperiences
{
    /// <summary>
    /// MEF Container class for Interactive Experience Action Type Components.
    /// </summary>
    internal class ActionTypeContainer : Container<ActionTypeComponent, IComponentData>
    {
        #region Fields

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<ActionTypeContainer> _instance =
            new Lazy<ActionTypeContainer>( () => new ActionTypeContainer() );


        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ActionTypeContainer Instance => _instance.Value;

        /// <summary>
        /// Gets all action type components that have been found in the system.
        /// </summary>
        /// <value>All action type components that have been found in the system.</value>
        public IEnumerable<ActionTypeComponent> AllComponents => Instance.Components.Values.Select( v => v.Value );

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( ActionTypeComponent ) )]
        protected override IEnumerable<Lazy<ActionTypeComponent, IComponentData>> MEFComponents { get; set; }

        #endregion

        /// <summary>
        /// Forces a reloading of all the components
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            // Create any attributes that need to be created
            var actionEntityTypeId = EntityTypeCache.Get<InteractiveExperienceAction>().Id;

            using ( var rockContext = new RockContext() )
            {
                foreach ( var actionComponent in this.Components )
                {
                    var actionComponentType = actionComponent.Value.Value.GetType();
                    var actionComponentEntityTypeId = EntityTypeCache.Get( actionComponentType ).Id;
                    Rock.Attribute.Helper.UpdateAttributes( actionComponentType, actionEntityTypeId, "ActionEntityTypeId", actionComponentEntityTypeId.ToString(), rockContext );
                }
            }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>An instance of <see cref="ActionTypeComponent"/> or <c>null</c> if it was not found.</returns>
        public static ActionTypeComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the component matching the specified type.
        /// </summary>
        /// <returns>An instance of <see cref="ActionTypeComponent"/> or <c>null</c> if it was not found.</returns>
        public static ActionTypeComponent GetComponent<T>()
        {
            return GetComponent( typeof( T ).FullName );
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>A <see cref="string"/> that represents the name of the component or an empty string if not found.</returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the name of the component or an empty string if not found.</returns>
        public static string GetComponentName<T>()
        {
            return GetComponentName( typeof( T ).FullName );
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <param name="component">The component whose name is to be determined.</param>
        /// <returns>A <see cref="string"/> that represents the name of the component or an empty string if not found.</returns>
        public static string GetComponentName( ActionTypeComponent component )
        {
            if ( component == null )
            {
                throw new ArgumentNullException( nameof( component ) );
            }

            return GetComponentName( component.GetType().FullName );
        }
    }
}
