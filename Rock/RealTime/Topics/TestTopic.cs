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
using System.Threading;
using System.Threading.Tasks;

namespace Rock.RealTime.Topics
{
    public class TestTopic : Topic<ITestTopicClient>
    {
        public async Task<int> Ping( string text, int value )
        {
            try
            {
                var caller = Clients.Current;

                var result = caller.Pong( value, CancellationToken.None );

                await result;

                await Clients.Channel( "123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789" ).Pong( value + 1, CancellationToken.None );
                await Clients.Channel( "123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-" ).Pong( value + 2, CancellationToken.None );

                return value;
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( ex.Message );
                throw ex;
            }
        }
    }
}
