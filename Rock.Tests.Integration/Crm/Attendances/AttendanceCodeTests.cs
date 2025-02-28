﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Crm.Attendance
{
    /// <summary>
    /// Used for testing anything regarding AttendanceCode.
    /// NOTE on IDisposable: We'd like to be able to use IDisposble to perform automatic Cleanup() after
    /// each test method but we can't do this until we can safely run tests that have a db (ie, we don't
    /// want to break other teams CI environments that are running these tests w/o a db.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    [TestClass]
    public class AttendanceCodeTests : DatabaseTestsBase
    {
        [TestCleanup]
        public void TestCleanup()
        {
            AttendanceCodeService.FlushTodaysCodes();
        }

        #region Tests that don't require a database/context

        /// <summary>
        /// Avoids the triple six.  Note: Does not use the database.
        /// </summary>
        [TestMethod]
        public void AvoidTripleSix()
        {
            int alphaNumericLength = 0;
            int alphaLength = 0;
            int numericLength = 4;
            bool isRandomized = false;
            string lastCode = "0665";

            string code = AttendanceCodeService.GetNextNumericCodeAsString( alphaNumericLength, alphaLength, numericLength, isRandomized, lastCode );
            Assert.That.Equal( "0667", code );
        }

        #endregion

        #region Alpha-numeric codes

        [TestMethod]
        public void AlphaNumericCodesShouldSkipBadCodes()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            for ( int i = 0; i < 6000; i++ )
            {
                var code = attendanceCodeService.CreateNewCode( 3, 0, 0, false );
                codeList.Add( code.Code );
            }

            bool hasMatchIsBad = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) ).Any();

            Assert.That.False( hasMatchIsBad );
        }

        #endregion

        #region Numeric only codes

        [TestMethod]
        public void CheckThreeCharNumericNonRandom002Code()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            AttendanceCode code = null;
            for ( int i = 0; i < 2; i++ )
            {
                code = attendanceCodeService.CreateNewCode( 0, 0, 3, false );
            }

            Assert.That.Equal( "002", code.Code );
        }

        [TestMethod]
        public void NumericCodesShouldSkip911And666()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            for ( int i = 0; i < 2000; i++ )
            {
                var code = attendanceCodeService.CreateNewCode( 0, 0, 4, false );
                codeList.Add( code.Code );
            }

            Assert.That.DoesNotContain( codeList, "911" );
            Assert.That.DoesNotContain( codeList, "666" );
        }

        /// <summary>
        /// Numeric only code with length of 2 should not go beyond 99.
        /// Attempting to create one should not be allowed so throwing a
        /// timeout exception is acceptable to let the admin know there is a
        /// configuration problem.
        /// </summary>
        [TestMethod]
        public void NumericCodeWithLengthOf2ShouldNotGoBeyond99()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            int maxTwoDigitCodes = 99; // 01-99
            try
            {
                for ( int i = 1; i <= maxTwoDigitCodes; i++ )
                {
                    var code = attendanceCodeService.CreateNewCode( 0, 0, 2, false );
                    codeList.Add( code.Code );
                }

                // should not be longer than 2 characters
                // This is a known bug in v7.4 and earlier, and possibly fixed via PR #3071
                Assert.That.True( codeList.OrderBy( x => x.Length ).Last().Length == 2 );
            }
            catch ( TimeoutException )
            {
                // An exception in this case is considered better than hanging (since there is 
                // no actual solution).
                Assert.That.True( true );
            }
        }

        /// <summary>
        /// Numerics codes should not repeat.  This is/was a known bug in v7.4 and earlier
        /// </summary>
        [TestMethod]
        public void NumericCodesShouldNotRepeat()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            int maxThreeDigitCodes = 997;
            for ( int i = 1; i < maxThreeDigitCodes; i++ )
            {
                var code = attendanceCodeService.CreateNewCode( 0, 0, 3, false );
                codeList.Add( code.Code );
            }

            var duplicates = codeList.GroupBy( x => x )
                                    .Where( group => group.Count() > 1 )
                                    .Select( group => group.Key );

            Assert.That.True( duplicates.Count() == 0 );
        }

        /// <summary>
        /// Numerics codes should not repeat.  This is/was a known bug in v7.4 and earlier
        /// </summary>
        [TestMethod]
        public void RandomNumericCodesShouldNotRepeat()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            int maxThreeDigitCodes = 997;
            for ( int i = 1; i < maxThreeDigitCodes; i++ )
            {
                var code = attendanceCodeService.CreateNewCode( 0, 0, 3, true );
                codeList.Add( code.Code );
            }

            var duplicates = codeList.GroupBy( x => x )
                                    .Where( group => group.Count() > 1 )
                                    .Select( group => group.Key );

            Assert.That.True( duplicates.Count() == 0 );
        }

        /// <summary>
        /// Requesting more codes than are possible should throw exception...
        /// because there's really nothing else we could do in that situation, right.
        /// </summary>
        [TestMethod]
        public void RequestingMoreCodesThanPossibleShouldThrowException()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            // Generate 100 codes (the maximum number of valid codes).
            // 100 because "00" is a valid code.
            for ( int i = 0; i < 100; i++ )
            {
                var code = attendanceCodeService.CreateNewCode( 0, 0, 2, true );
                codeList.Add( code.Code );
            }

            Assert.That.ThrowsException<TimeoutException>( () =>
            {
                var code = attendanceCodeService.CreateNewCode( 0, 0, 2, true );
                codeList.Add( code.Code );
            } );

            Assert.That.AreEqual( 100, codeList.Count );
        }

        [TestMethod]
        public void Increment100SequentialNumericCodes()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            AttendanceCode code = null;
            for ( int i = 0; i < 100; i++ )
            {
                code = attendanceCodeService.CreateNewCode( 0, 0, 3, false );
            }

            Assert.That.Equal( "100", code.Code );
        }

        #endregion

        #region Alpha only codes

        /// <summary>
        /// The number of unique three characters strings you can make from the 17 allowed
        /// characters in the Check-in system's 17 <see cref="AttendanceCodeService._alphaCharacters"/>
        /// characters is about 4,913.  But there are about 80 in the <see cref="AttendanceCodeService.NoGood"/>
        /// list so we should be able to run this loop to about 4000-4800 without running out of options.
        /// WARNING: The closer you get to the using the entire set, the slower this will take to complete.
        /// </summary>
        [TestMethod]
        public void ThreeCharAlphaOnlyCodesShouldSkipBadCodes()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            for ( int i = 0; i < 4000; i++ )
            {
                var code = attendanceCodeService.CreateNewCode( 0, 3, 0, true );
                codeList.Add( code.Code );
            }

            bool hasMatchIsBad = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) ).Any();

            Assert.That.False( hasMatchIsBad );
        }

        /// <summary>
        /// Alpha codes should not repeat.
        /// </summary>
        [TestMethod]
        public void ThreeCharAlphaOnlyCodesShouldNotRepeat()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            // 4800 (17*17*17 minus ~80 badcodes) possible combinations of 17 letters
            for ( int i = 0; i < 4800; i++ )
            {
                //System.Diagnostics.Debug.WriteIf( i > 4700, "code number " + i + " took... " );
                var code = attendanceCodeService.CreateNewCode( 0, 3, 0, false );
                codeList.Add( code.Code );
                //System.Diagnostics.Debug.WriteLineIf( i > 4700, "" );
            }

            var duplicates = codeList.GroupBy( x => x )
                                    .Where( group => group.Count() > 1 )
                                    .Select( group => group.Key );

            Assert.That.True( duplicates.Count() == 0 );
        }

        #endregion

        #region Alpha-numeric + numeric only codes

        /// <summary>
        /// A two character alpha numeric code (AttendanceCodeService.codeCharacters) has possible
        /// 24*24 (576) combinations plus 1 character numeric code has a possible 9 additional suffixes
        /// for a total set of 5184 combinations.  Removing the noGood (~80) codes leaves us with
        /// a valid set of about 5100 codes.
        /// 
        /// Even when run with 2 alpha numeric and 1 numeric, this test should verify that codes
        /// such as 666, 991 do not occur.
        /// 
        /// There should be no bad codes in the generated codeList -- even though
        /// individually each part has no bad codes.  For example, "66" + "6" should
        /// not appear since combined it would be "666".
        /// </summary>
        [TestMethod]
        public void AlphaNumericWith1NumericCodeShouldGenerateAtLeast5100Codes()
        {
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            int attemptCombination = 0;
            var stopWatch = new System.Diagnostics.Stopwatch();
            var stopWatchSingle = new System.Diagnostics.Stopwatch();

            stopWatch.Start();
            stopWatchSingle.Start();
            var outputDebug = false;
            try
            {
                var codeList = new List<string>();
                for ( int i = 1; i < 5100; i++ )
                {
                    attemptCombination = i;
                    if ( i > 4000 && i % 100 == 0 )
                    {
                        outputDebug = true;
                        stopWatchSingle.Restart();
                        System.Diagnostics.Debug.Write( "code number " + i + " took... " );
                    }
                    var code = attendanceCodeService.CreateNewCode( 2, 0, 1, true );
                    if ( outputDebug )
                    {
                        System.Diagnostics.Debug.WriteLine( stopWatchSingle.ElapsedMilliseconds + " ms" );
                        outputDebug = false;
                    }
                    codeList.Add( code.Code );
                }

                var matches = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) );
                bool hasMatchIsBad = matches.Any();

                Assert.That.IsFalse( hasMatchIsBad, "bad codes were: " + string.Join( ", ", matches ) );

                var duplicates = codeList.GroupBy( x => x )
                        .Where( group => group.Count() > 1 )
                        .Select( group => group.Key );

                Assert.That.True( duplicates.Count() == 0 );
            }
            catch ( TimeoutException )
            {
                // If an infinite loop was detected, but we tried at least 5100 codes then
                // we'll consider this a pass.
                Assert.That.IsTrue( attemptCombination >= 5100 );
            }
            finally
            {
                stopWatch.Stop();
                System.Diagnostics.Trace.Listeners.Add( new System.Diagnostics.TextWriterTraceListener( Console.Out ) );
                System.Diagnostics.Trace.WriteLine( string.Format( "Test AlphaNumericWith1NumericCodeShouldGenerateAtLeast5100Codes took {0} ms.", stopWatch.ElapsedMilliseconds ) );
            }
        }

        /// <summary>
        /// Two character alpha numeric codes (AttendanceCodeService.codeCharacters) has possible
        /// 24*24 (576) combinations plus two character numeric codes has a possible 10*10 (100)
        /// for a total set of 676 combinations.  Removing the noGood (~60) codes leaves us with
        /// a valid set of about 616 codes.
        /// 
        /// NOTE: This appears to be a possible bug in v8.0 and earlier. The AttendanceCodeService
        /// service will only generate 100 codes when trying to combine the numeric parameter of "2" with
        /// the other parameters.
        ///
        /// Even when run with 2 alpha numeric and 3 numeric, this test should verify that codes
        /// such as X6662, 99119, 66600 do not occur.
        /// 
        /// There should be no bad codes in the generated codeList -- even though
        /// individually each part has no bad codes.  For example, "A6" + "66" should
        /// not appear since combined it would be "A666".
        /// </summary>
        [TestMethod]
        public void AlphaNumericWithNumericCodesShouldSkipBadCodes()
        {
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            int attemptCombination = 0;
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            try
            {
                var codeList = new List<string>();
                for ( int i = 0; i < 596; i++ )
                {
                    attemptCombination = i;
                    var code = attendanceCodeService.CreateNewCode( 2, 0, 3, true );
                    codeList.Add( code.Code );
                }

                var matches = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) );
                bool hasMatchIsBad = matches.Any();

                Assert.That.IsFalse( hasMatchIsBad, "bad codes were: " + string.Join( ", ", matches ) );
            }
            catch ( TimeoutException )
            {
                // If an infinite loop was detected, but we tried at least 596 codes then
                // we'll consider this a pass.
                Assert.That.IsTrue( attemptCombination >= 596 );
            }
            finally
            {
                stopWatch.Stop();
                System.Diagnostics.Trace.Listeners.Add( new System.Diagnostics.TextWriterTraceListener( Console.Out ) );
                System.Diagnostics.Trace.WriteLine( string.Format( "Test AlphaOnlyWithNumericOnlyCodesShouldSkipBadCodes took {0} ms.", stopWatch.ElapsedMilliseconds ) );
            }
        }

        #endregion

        #region Alpha only + numeric only codes

        [TestMethod]
        public void ThreeCharAlphaWithFourCharNumericCodesShouldSkipBadCodes()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            for ( int i = 0; i < 6000; i++ )
            {
                var code = attendanceCodeService.CreateNewCode( 0, 3, 4, true );
                codeList.Add( code.Code );
            }

            bool hasMatchIsBad = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) ).Any();

            Assert.That.False( hasMatchIsBad );
        }

        /// <summary>
        /// This is the configuration that churches like Central Christian Church use for their
        /// Children's check-in.
        /// </summary>
        [TestMethod]
        public void TwoAlphaWithFourRandomNumericCodesShouldSkipBadCodes()
        {
            var codeList = new List<string>();
            var rockContextMock = CreateRockContextMock();
            var attendanceCodeService = new AttendanceCodeService( rockContextMock.Object );

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            int attemptCombination = 0;

            for ( int i = 0; i < 2500; i++ )
            {
                attemptCombination = i;
                var code = attendanceCodeService.CreateNewCode( 0, 2, 4, true );
                codeList.Add( code.Code );
            }

            var matches = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) );
            bool hasMatchIsBad = matches.Any();
            Assert.That.IsFalse( hasMatchIsBad, "bad codes were: " + string.Join( ", ", matches ) );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add( new System.Diagnostics.TextWriterTraceListener( Console.Out ) );
            System.Diagnostics.Trace.WriteLine( string.Format( "Test AlphaOnlyWithNumericOnlyCodesShouldSkipBadCodes took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }

        #endregion

        /// <summary>
        /// Creates a mock <see cref="RockContext"/> that is configured for
        /// use with attendance codes. The context is created with no
        /// attendance codes in the database to represent a clean day.
        /// </summary>
        /// <returns>A mocking object for <see cref="RockContext"/>.</returns>
        private Mock<RockContext> CreateRockContextMock()
        {
            var rockContextMock = MockDatabaseHelper.GetRockContextMock();

            rockContextMock.SetupDbSet( new List<AttendanceCode>() );
            rockContextMock.Setup( m => m.SaveChanges() ).Returns( 0 );

            return rockContextMock;
        }
    }
}
