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

namespace Rock.RealTime
{
    /// <summary>
    /// Default implementation of <see cref="IContext"/>.
    /// </summary>
    internal class Context : IContext
    {
        #region Properties

        /// <inheritdoc/>
        public string ConnectionId { get; set; }

        /// <inheritdoc/>
        public string UserName { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="userName">Name of the user that is logged in.</param>
        internal Context( string connectionId, string userName )
        {
            ConnectionId = connectionId;
            UserName = userName;
        }

        #endregion
    }
}
