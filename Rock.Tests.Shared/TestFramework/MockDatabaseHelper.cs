﻿using System;
using System.Collections.Generic;
using System.Data.Entity;

using Moq;
using Moq.Protected;

using Rock.Attribute;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Helper methods for working with mock databases.
    /// </summary>
    public static class MockDatabaseHelper
    {
        /// <summary>
        /// Gets a mocked <see cref="RockContext"/> that can be used to setup
        /// additional mocked values and then used for database access.
        /// </summary>
        /// <returns>An mocking instance for <see cref="RockContext"/>.</returns>
        public static Mock<RockContext> GetRockContextMock()
        {
            var rockContextMock = new Mock<RockContext>( MockBehavior.Strict, "invalidConnectionString" );

            // Ignore any call to dispose.
            rockContextMock.Protected().Setup( "Dispose", ItExpr.IsAny<bool>() );

            return rockContextMock;
        }

        /// <summary>
        /// Creates a mock <typeparamref name="TEntity"/> object.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="guid">The entity unique identifier.</param>
        /// <returns>A mocking instance for <typeparamref name="TEntity"/>.</returns>
        public static Mock<TEntity> CreateEntityMock<TEntity>( int id, Guid guid )
            where TEntity : class, IEntity, new()
        {
            var entityMock = new Mock<TEntity>( MockBehavior.Loose )
            {
                CallBase = true
            };

            entityMock.Setup( m => m.TypeId ).Returns( 0 );

            entityMock.Object.Id = id;
            entityMock.Object.Guid = guid;

            if ( entityMock.Object is IHasAttributes attributeMock )
            {
                attributeMock.Attributes = new Dictionary<string, AttributeCache>();
                attributeMock.AttributeValues = new Dictionary<string, AttributeValueCache>();
            }

            return entityMock;
        }
    }
}
