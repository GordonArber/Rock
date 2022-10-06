using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Rest;

namespace Rock.Tests.UnitTests.Rock
{
    [TestClass]
    public class ODataFilterParserTests
    {
        [TestMethod]
        [DataRow( 10, 0 )]
        [DataRow( 100, 0 )]
        [DataRow( 1000, 0 )]

        [DataRow( 10, 1 )]
        [DataRow( 100, 1 )]
        [DataRow( 1000, 1 )]
        [DataRow( 10000, 1 )]

        [DataRow( 10, 5 )]
        [DataRow( 100, 5 )]
        [DataRow( 1000, 5 )]
        [DataRow( 10000, 5 )]

        [DataRow( 10, 11 )]
        [DataRow( 100, 11 )]
        [DataRow( 1000, 11 )]
        
        public void PerformanceTest( int urlCount, int filterCount )
        {
            string baseUrl = $"https://localhost:44329/api/People?$filter=";
            StringBuilder filterExpression = new StringBuilder();
            if ( filterCount > 0 )
            {
                filterExpression.Append( $"ModifiedDateTime eq datetime'{RockDateTime.Now.ToISO8601DateString()}'" );
                for ( int i = 0; i < filterCount; i++ )
                {
                    filterExpression.Append( " and " );
                    filterExpression.Append( $"Guid eq guid'{Guid.NewGuid():D}'" );
                    filterExpression.Append( " and " );
                    filterExpression.Append( $"ModifiedDateTime eq datetime'{RockDateTime.Now.ToISO8601DateString()}'" );
                }
            }

            var filterExpressionFilter = filterExpression.ToString();
            var originalUrl = baseUrl + System.Net.WebUtility.UrlEncode( filterExpressionFilter );

            var warmup = RockEnableQueryAttribute.ParseUrl( originalUrl, filterExpressionFilter );

            List<double> elaspedMS = new List<double>();
            Stopwatch stopwatch = Stopwatch.StartNew();
            for ( int i = 0; i < urlCount; i++ )
            {
                stopwatch.Restart();
                var updatedUrl = RockEnableQueryAttribute.ParseUrl( originalUrl, filterExpressionFilter );
                stopwatch.Stop();
                elaspedMS.Add( stopwatch.Elapsed.TotalMilliseconds );
            }

            Debug.WriteLine($"SB: [{elaspedMS.Average()} ms], urlCount={urlCount}, filterCount={filterCount}" );
        }

        [TestMethod]
        [DataRow(
            "Guid eq guid'722dfa12-b47d-49c3-8b23-1b7d08a1cf53'",
            "Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53" )]
        [DataRow(
            "Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53",
            "Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53" )]
        [DataRow(
            "ModifiedDateTime eq datetime'2022-10-04T10:56:50.747'",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747" )]
        [DataRow(
            "ModifiedDateTime eq 2022-10-04T10:56:50.747 and Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747 and Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53" )]
        [DataRow(
            "ModifiedDateTime eq 2022-10-04T10:56:50.747",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747" )]
        [DataRow(
            "ModifiedDateTime eq datetime'2022-10-04T10:56:50.747' and Guid eq guid'722dfa12-b47d-49c3-8b23-1b7d08a1cf53'",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747 and Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53" )]
        [DataRow(
            "ModifiedDateTime eq datetime'2022-10-04T10:56:50.747' and Guid eq guid'722dfa12-b47d-49c3-8b23-1b7d08a1cf53'",
            "ModifiedDateTime eq 2022-10-04T10:56:50.747 and Guid eq 722dfa12-b47d-49c3-8b23-1b7d08a1cf53" )]
        public void DidParseCorrectlyTest(string originalFilter, string expectedResult)
        {
            string originalUrl = System.Net.WebUtility.UrlEncode( $"https://localhost:44329/api/People?$filter={originalFilter}" );
            var actualResult = RockEnableQueryAttribute.ParseUrl( originalUrl, originalFilter );
            string expectedUrl = System.Net.WebUtility.UrlEncode( $"https://localhost:44329/api/People?$filter={expectedResult}" );
            Assert.AreEqual( actualResult, expectedUrl );
        }
    }
}
