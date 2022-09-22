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

using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using Rock.Attribute;

namespace Rock.RealTime.AspNet
{
    /// <summary>
    /// The SignalR hub implementation for SignalR v2 on ASP.Net.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [HubName( "realTime" )]
    [RockInternal]
    public sealed class RealTimeHub : Hub<IRockHubClientProxy>
    {
        public async Task<object> PostMessage( string hubIdentifier, string messageName, object[] parameters )
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var hubInstance = RealTimeHelper.GetHubInstance( this, hubIdentifier );
            sw.Stop();

            var mi = hubInstance.GetType().GetMethod( messageName );
            var methodParameters = mi.GetParameters();
            var parms = new object[methodParameters.Length];

            for ( int i = 0; i < methodParameters.Length; i++ )
            {
                if ( methodParameters[i].ParameterType == typeof( int ) )
                {
                    parms[i] = ( int ) ( long ) parameters[i];
                }
                else if ( methodParameters[i].ParameterType == typeof( string ) )
                {
                    parms[i] = ( string ) parameters[i];
                }
                else if ( methodParameters[i].ParameterType == typeof( CancellationToken ) )
                {
                    parms[i] = CancellationToken.None;
                }
            }

            var result = mi.Invoke( hubInstance, parms );

            if ( result is Task resultTask )
            {
                await resultTask;

                // Task<T> is not covariant, so we can't just cast to Task<object>.
                if ( resultTask.GetType().GetProperty( "Result" ) != null )
                {
                    result = ( ( dynamic ) resultTask ).Result;
                }
                else
                {
                    result = null;
                }
            }

            return result;
        }

        public override Task OnConnected()
        {
            Groups.Add( Context.ConnectionId, "123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-123456789-" );
            System.Console.WriteLine( $"New connection on server {/*Startup.Url*/ "unknown"}" );
            return Task.CompletedTask;
        }
    }
}
