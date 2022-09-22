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
using System.Reflection;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;

using Owin;

using Rock;
using Rock.Model;
using Rock.Utility;

[assembly: OwinStartup( typeof( RockWeb.Startup ) )]
namespace RockWeb
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configurations the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configuration( IAppBuilder app )
        {
            ConfigureSignalR( app );

            try
            {
                // This is for OIDC Connect
                Rock.Oidc.Startup.OnStartup( app );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }

            try
            {
                // Find any plugins that implement IRockOwinStartup
                var startups = new Dictionary<int, List<IRockOwinStartup>>();
                foreach ( var startupType in Rock.Reflection.FindTypes( typeof( IRockOwinStartup ) ).Select( a => a.Value ).ToList() )
                {
                    var startup = Activator.CreateInstance( startupType ) as IRockOwinStartup;
                    startups.AddOrIgnore( startup.StartupOrder, new List<IRockOwinStartup>() );
                    startups[startup.StartupOrder].Add( startup );
                }

                foreach ( var startupList in startups.OrderBy( s => s.Key ).Select( s => s.Value ) )
                {
                    foreach ( var startup in startupList )
                    {
                        startup.OnStartup( app );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        /// <summary>
        /// Configure SignalR for use by the application.
        /// </summary>
        /// <param name="app">The application builder to be configured.</param>
        private void ConfigureSignalR( IAppBuilder app )
        {
            var legacyHubConfiguration = new HubConfiguration
            {
                Resolver = new DefaultDependencyResolver()
            };

            app.MapSignalR( legacyHubConfiguration );

            /* 02/18/2022 MDP
             By default, Signal R will use reflection to find classes that inherit from Microsoft.AspNet.SignalR.
             It looks in *all* DLLs in RockWeb/bin. It does this on the first page that includes <script src="/SignalR/hubs"></script>.
             This initial hit can take 30-60 seconds, so we'll register our own assembly locator to only look in Rock and Rock Plugins.
             RockWeb.RockMessageHub will be the only Hub. So it doesn't make sense to look in all DLL for any more.

             09/22/2022 DSH
             This must be done _after_ the call to MapSignalR otherwise our
             out custom locator will be replaced.
            */
            legacyHubConfiguration.Resolver.Register( typeof( IAssemblyLocator ), () => new RockHubAssemblyLocator() );

            bool useAzureSignalR = false;

            if ( !useAzureSignalR )
            {
                var rtHubConfiguration = new HubConfiguration
                {
                    Resolver = new DefaultDependencyResolver()
                };
                rtHubConfiguration.Resolver.Register( typeof( IHubDescriptorProvider ), () => new RealTimeHubDescriptorProvider() );
                app.MapSignalR( "/signalr-rt", rtHubConfiguration );

                Rock.WebStartup.RockApplicationStartupHelper.InitializeRockRealTime( rtHubConfiguration );
            }
        }

        /// <summary>
        /// Class RockHubAssemblyLocator.
        /// Implements the <see cref="Microsoft.AspNet.SignalR.Hubs.IAssemblyLocator" />
        /// </summary>
        /// <seealso cref="Microsoft.AspNet.SignalR.Hubs.IAssemblyLocator" />
        private class RockHubAssemblyLocator : IAssemblyLocator
        {
            /// <summary>
            /// Gets the assemblies.
            /// </summary>
            /// <returns>IList&lt;Assembly&gt;.</returns>
            public IList<Assembly> GetAssemblies()
            {
                return Rock.Reflection.GetRockAndPluginAssemblies();
            }
        }

        /// <summary>
        /// This is a custom implementation for the "signalr-rt" endpoint to
        /// ensure that the only hub that shows up is the RealTime hub.
        /// </summary>
        private class RealTimeHubDescriptorProvider : IHubDescriptorProvider
        {
            /// <summary>
            /// Lazy loaded dictionary of hubs to provide to the resolver.
            /// </summary>
            private readonly Lazy<IDictionary<string, HubDescriptor>> _hubs = new Lazy<IDictionary<string, HubDescriptor>>( BuildHubsCache );

            /// <inheritdoc/>
            public IList<HubDescriptor> GetHubs()
            {
                return _hubs.Value.Select( kv => kv.Value ).Distinct().ToList();
            }

            /// <inheritdoc/>
            public bool TryGetHub( string hubName, out HubDescriptor descriptor )
            {
                return _hubs.Value.TryGetValue( hubName, out descriptor );
            }

            /// <summary>
            /// Build the collection of known hubs, in this case just our RealTime hub.
            /// </summary>
            /// <returns>A dictionary of hub names for keys and hub descriptors for values.</returns>
            protected static IDictionary<string, HubDescriptor> BuildHubsCache()
            {
                var type = typeof( Rock.RealTime.AspNet.RealTimeHub );

                var descriptor = new HubDescriptor
                {
                    NameSpecified = true,
                    Name = "realTime",
                    HubType = type
                };

                return new Dictionary<string, HubDescriptor>( StringComparer.OrdinalIgnoreCase )
                {
                    [descriptor.Name] = descriptor
                };
            }
        }
    }
}
