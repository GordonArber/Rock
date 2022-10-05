// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;

namespace Rock.Rest
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action to enable
    /// querying using the OData query syntax using the precompiled Rock EdmModel
    /// </summary>
    public class RockEnableQueryAttribute : EnableQueryAttribute
    {
        /// <summary>
        /// Gets the EDM model for the given type and request. Override this method to customize the EDM model used for querying.
        /// </summary>
        /// <param name="elementClrType">The CLR type to retrieve a model for.</param>
        /// <param name="request">The request message to retrieve a model for.</param>
        /// <param name="actionDescriptor">The action descriptor for the action being queried on.</param>
        /// <returns>The EDM model for the given type and request.</returns>
        public override Microsoft.OData.Edm.IEdmModel GetModel( Type elementClrType, System.Net.Http.HttpRequestMessage request, System.Web.Http.Controllers.HttpActionDescriptor actionDescriptor )
        {
            // use the EdmModel that we already created in WebApiConfig (so that we don't have problems with OData4 Open Types)
            var baseModel = base.GetModel( elementClrType, request, actionDescriptor );
            var ourModel = WebApiConfig.EdmModel;
            return ourModel;
        }

        /// <summary>
        /// Applies the query to the given entity based on incoming query from uri and query settings.
        /// </summary>
        /// <param name="entity">The original entity from the response message.</param>
        /// <param name="queryOptions">The <see cref="T:Microsoft.AspNet.OData.Query.ODataQueryOptions" /> instance constructed based on the incoming request.</param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        public override object ApplyQuery( object entity, ODataQueryOptions queryOptions )
        {
            return base.ApplyQuery( entity, EnsureV4ODataQueryOptions( queryOptions ) );
        }

        /// <summary>
        /// Applies the query to the given IQueryable based on incoming query from uri and query settings. By default,
        /// the implementation supports $top, $skip, $orderby and $filter. Override this method to perform additional
        /// query composition of the query.
        /// </summary>
        /// <param name="queryable">The original queryable instance from the response message.</param>
        /// <param name="queryOptions">The <see cref="T:Microsoft.AspNet.OData.Query.ODataQueryOptions" /> instance constructed based on the incoming request.</param>
        /// <returns>IQueryable.</returns>
        public override IQueryable ApplyQuery( IQueryable queryable, ODataQueryOptions queryOptions )
        {
            return base.ApplyQuery( queryable, EnsureV4ODataQueryOptions( queryOptions ) );
        }

        /// <summary>
        /// Validates the OData query in the incoming request. By default, the implementation throws an exception if
        /// the query contains unsupported query parameters. Override this method to perform additional validation of
        /// the query.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="queryOptions">The <see cref="T:Microsoft.AspNet.OData.Query.ODataQueryOptions" /> instance constructed based on the incoming request.</param>
        public override void ValidateQuery( HttpRequestMessage request, ODataQueryOptions queryOptions )
        {
            base.ValidateQuery( request, EnsureV4ODataQueryOptions( queryOptions ) );
        }

        /// <summary>
        /// Ensures the v4 o data query options.
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        /// <returns>ODataQueryOptions.</returns>
        private ODataQueryOptions EnsureV4ODataQueryOptions( ODataQueryOptions queryOptions )
        {
            if ( InferIfRequestUriHasV3Filters( queryOptions ) )
            {
                return ConvertOData3FiltersToODataV4( queryOptions );
            }
            else
            {
                return queryOptions;
            }
        }

        // Sometimes clients will not specify dataserviceversion or maxdataserviceversion
        // While this is required in the spec, we can quietly check a couple giveaways that the request URI is v3
        /// <summary>
        /// Infers if request URI has v3 filters.
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool InferIfRequestUriHasV3Filters( ODataQueryOptions queryOptions )
        {
            var rawFilter = queryOptions?.Filter?.RawValue;
            if ( rawFilter.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var isV3DateTimeFilter = _dateTimeFilterCapture.IsMatch( rawFilter );
            var isV3GuidFilter = _guidFilterCapture.IsMatch( rawFilter );
            return isV3DateTimeFilter || isV3GuidFilter;
        }

        /// <summary>
        /// The date time filter capture
        /// </summary>
        private readonly Regex _dateTimeFilterCapture = new Regex( @"datetime\'(\S*)\'", RegexOptions.Compiled );
        /// <summary>
        /// The unique identifier filter capture
        /// </summary>
        private readonly Regex _guidFilterCapture = new Regex( @"guid\'([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\'", RegexOptions.Compiled );

        /// <summary>
        /// Converts the o data3 filters to o data v4.
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        /// <returns>ODataQueryOptions.</returns>
        private ODataQueryOptions ConvertOData3FiltersToODataV4( ODataQueryOptions queryOptions )
        {
            var originalUrl = queryOptions.Request.RequestUri.OriginalString;
            var rawFilter = queryOptions?.Filter?.RawValue;
            var dateTimeMatches = _dateTimeFilterCapture.Matches( rawFilter );
            var updatedUrl = originalUrl;

            foreach ( Match match in dateTimeMatches )
            {
                if ( match.Groups.Count == 2 )
                {
                    var v3Filter = match.Groups[0].Value;
                    var capture = match.Groups[1].Value;

                    var replace = System.Net.WebUtility.UrlEncode( v3Filter );
                    var replaceWith = System.Net.WebUtility.UrlEncode( capture );
                    updatedUrl = updatedUrl.Replace( replace, replaceWith );
                }
            }

            var guidMatches = _guidFilterCapture.Matches( rawFilter );
            foreach ( Match match in guidMatches )
            {
                if ( match.Groups.Count == 2 )
                {
                    var v3Filter = match.Groups[0].Value;
                    var capture = match.Groups[1].Value;

                    var replace = System.Net.WebUtility.UrlEncode( v3Filter );
                    var replaceWith = System.Net.WebUtility.UrlEncode( capture );
                    updatedUrl = updatedUrl.Replace( replace, replaceWith );
                }
            }
            
            var convertedRequest = new HttpRequestMessage( queryOptions.Request.Method, updatedUrl );
            foreach( var origProperty in queryOptions.Request.Properties )
            {
                convertedRequest.Properties.Add( origProperty.Key, origProperty.Value );
            }

            foreach ( var origProperty in queryOptions.Request.Headers )
            {
                convertedRequest.Headers.Add( origProperty.Key, origProperty.Value );
            }

            var convertedODataQueryOptions = new ODataQueryOptions( queryOptions.Context, convertedRequest );

            return convertedODataQueryOptions;
        }
    }
}
