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

namespace Rock.RealTime
{
    /// <summary>
    /// Provides details about a topic that was found and will be exposed
    /// with the RealTime system.
    /// </summary>
    internal abstract class TopicConfiguration
    {
        #region Properties

        /// <summary>
        /// Gets a string that uniquely identifies this topic. It will be used
        /// when sending and receiving messages over SignalR.
        /// </summary>
        public string TopicIdentifier { get; protected set; }

        /// <summary>
        /// Gets the <see cref="Type"/> that represents the original topic
        /// implementation class.
        /// </summary>
        public Type TopicType { get; protected set; }

        /// <summary>
        /// Gets the <see cref="Type"/> that represents the interface used
        /// by the topic to send messages to the clients.
        /// </summary>
        public Type ClientInterfaceType { get; protected set; }

        /// <summary>
        /// Gets the default implementation of the <see cref="RockHubContext{T}"/>
        /// for the <see cref="ClientInterfaceType"/>.
        /// </summary>
        public object TopicContext { get; protected set; }

        /// <summary>
        /// Gets the <see cref="Type"/> that implements the <see cref="ITopicCallerClients{T}"/>
        /// interface for the <see cref="ClientInterfaceType"/>.
        /// </summary>
        public Type CallerClientsType { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="TopicConfiguration"/> and sets
        /// the initial values. This object will hold information about the topic
        /// that describes how it should function at runtime.
        /// </summary>
        /// <param name="topicType">The <see cref="Type"/> that describes the topic to be configured.</param>
        public TopicConfiguration( Type topicType )
        {
            if ( !topicType.BaseType.IsGenericType || topicType.BaseType.GetGenericTypeDefinition() != typeof( Topic<> ) )
            {
                throw new Exception( "Invalid topic type." );
            }

            var clientInterfaceType = topicType.BaseType.GetGenericArguments()[0];

            if ( !clientInterfaceType.IsInterface )
            {
                throw new Exception( $"Invalid topic type '{topicType.FullName}'. Must provide interface when subclassing {typeof( Topic<> ).FullName}." );
            }

            TopicIdentifier = topicType.FullName;
            TopicType = topicType;
            ClientInterfaceType = clientInterfaceType;

            var hubContextType = typeof( RockHubContext<> ).MakeGenericType( ClientInterfaceType );
            TopicContext = Activator.CreateInstance( hubContextType );
        }

        #endregion
    }
}
