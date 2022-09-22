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
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Rock.RealTime
{
    /// <summary>
    /// Provides implementation configuration and features for the Rock
    /// RealTime system.
    /// </summary>
    internal abstract class Engine
    {
        #region Fields

        /// <summary>
        /// The lazy-initialized list of registered topics.
        /// </summary>
        private readonly Lazy<List<TopicConfiguration>> _registeredTopics;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the topics that were found through reflection and registered
        /// in the system for use in real-time communication.
        /// </summary>
        protected List<TopicConfiguration> RegisteredTopics => _registeredTopics.Value;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="Engine"/> and sets
        /// all values to defaults.
        /// </summary>
        public Engine()
        {
            _registeredTopics = new Lazy<List<TopicConfiguration>>( BuildTopics );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the registered topics. This is called by the lazy initializer
        /// so that we don't waste CPU cycles while Rock is starting.
        /// </summary>
        /// <returns>A list of <see cref="TopicConfiguration"/> objects that describe all the topics that were found.</returns>
        private List<TopicConfiguration> BuildTopics()
        {
            var types = Rock.Reflection.FindTypes( typeof( Topic<> ) ).Values;
            var topics = new List<TopicConfiguration>();

            foreach ( var type in types )
            {
                try
                {
                    topics.Add( GetTopicConfiguration( type ) );
                }
                catch ( Exception ex )
                {
                    var loggedException = new Exception( $"Error while trying to register real-time topic {type.FullName}.", ex );
                    Rock.Model.ExceptionLogService.LogException( loggedException );
                }
            }

            return topics;
        }

        /// <summary>
        /// Get the context to send messages to connections on a topic.
        /// </summary>
        /// <typeparam name="TTopicClient">The type that identifies the interface associated with the topic.</typeparam>
        /// <returns>An instance of <see cref="ITopicContext{T}"/> scoped to the interface <typeparamref name="TTopicClient"/>.</returns>
        public ITopicContext<TTopicClient> GetTopicContext<TTopicClient>()
            where TTopicClient : class
        {
            var clientInterfaceType = typeof( TTopicClient );

            var topicConfiguration = RegisteredTopics
                .FirstOrDefault( tc => tc.ClientInterfaceType == clientInterfaceType );

            if ( topicConfiguration == null )
            {
                throw new Exception( $"Topic for interface '{clientInterfaceType.FullName}' was not found." );
            }

            return ( ITopicContext<TTopicClient> ) topicConfiguration.TopicContext;
        }

        /// <summary>
        /// Get an instance of the hub specified by its identifier. Each time
        /// an incoming message from a connection is handled a new instance
        /// is created to respond to the message. This is the same way as the
        /// base SignalR works.
        /// </summary>
        /// <param name="realTimeHub">The hub object that is currently processing the real request.</param>
        /// <param name="topicIdentifier">The identifier of the topic that should be created.</param>
        /// <returns>A new instance of the topic class that will handle the request.</returns>
        public object GetHubInstance( object realTimeHub, string topicIdentifier )
        {
            var topicConfiguration = RegisteredTopics
                .FirstOrDefault( tc => tc.TopicIdentifier == topicIdentifier );

            if ( topicConfiguration == null )
            {
                throw new Exception( $"Topic '{topicIdentifier}' was not found." );
            }

            var hubInstance = Activator.CreateInstance( topicConfiguration.TopicType );

            ConfigureTopicInstance( realTimeHub, topicConfiguration, hubInstance );

            return hubInstance;
        }

        /// <summary>
        /// Gets the topic configuration for the given topic type.
        /// </summary>
        /// <param name="topicType">The <see cref="Type"/> that describes the topic to be configured.</param>
        /// <returns>A new instance of <see cref="TopicConfiguration"/> that describes the topic.</returns>
        protected abstract TopicConfiguration GetTopicConfiguration( Type topicType );

        /// <summary>
        /// Performs additional configuration of a topic instance. This is used
        /// to provide custom property values for the different implementations.
        /// </summary>
        /// <param name="realTimeHub">The real hub used to communicate with clients.</param>
        /// <param name="topicConfiguration">The configuration of the topic being setup.</param>
        /// <param name="topicInstance">The instance object that will handle incoming topic messages.</param>
        protected abstract void ConfigureTopicInstance( object realTimeHub, TopicConfiguration topicConfiguration, object topicInstance );

        /// <summary>
        /// Sends a message to the specified clients scoped to the topic.
        /// </summary>
        /// <param name="proxy">The proxy object that will send the actual message.</param>
        /// <param name="topicIdentifier">The identifier of the topic that the message will be scoped to.</param>
        /// <param name="messageName">The name that identifies the message to be sent.</param>
        /// <param name="parameters">The parameters to be passed in the message.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
        public abstract Task SendMessageAsync( object proxy, string topicIdentifier, string messageName, object[] parameters, CancellationToken cancellationToken );

        #endregion
    }
}
