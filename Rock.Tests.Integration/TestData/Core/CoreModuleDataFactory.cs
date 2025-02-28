﻿// <copyright>
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
using System.Diagnostics;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Integration.TestData.Core
{
    /// <summary>
    /// Functions to assist with core module testing.
    /// </summary>
    public class CoreModuleDataFactory
    {
        private string _RecordTag = null;

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="recordTag">A tag that is added to the ForeignKey property of each record created by this helper instance.</param>
        public CoreModuleDataFactory( string recordTag )
        {
            _RecordTag = recordTag;
        }

        /// <summary>
        /// Add or update a Category.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="category"></param>
        public void AddOrUpdateCategory( RockContext dataContext, Category category )
        {
            var categoryService = new CategoryService( dataContext );

            var existingCategory = categoryService.Queryable().FirstOrDefault( x => x.Guid == category.Guid );

            if ( existingCategory == null )
            {
                categoryService.Add( category );

                existingCategory = category;
            }
            else
            {
                var id = existingCategory.Id;

                existingCategory.CopyPropertiesFrom( category );

                existingCategory.Id = id;
                category.Id = id;
            }
        }

        /// <summary>
        /// Create a new Category
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        /// <param name="appliesToEntityTypeId"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public Category CreateCategory( string name, Guid guid, int appliesToEntityTypeId, int order = 0 )
        {
            var newCategory = new Category();

            newCategory.Name = name;
            newCategory.Guid = guid;
            newCategory.IsSystem = true;
            newCategory.EntityTypeId = appliesToEntityTypeId;
            newCategory.Order = order;

            newCategory.ForeignKey = _RecordTag;

            return newCategory;
        }

        /// <summary>
        /// Remove Categories flagged with the current test record tag.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public int DeleteCategoriesByRecordTag( RockContext dataContext )
        {
            var categoryService = new CategoryService( dataContext );

            var categoryIdList = categoryService.Queryable().Where( x => x.ForeignKey == _RecordTag ).Select( x => x.Id ).ToList();

            var recordsDeleted = categoryIdList.Count;

            while ( categoryIdList.Count > 0 )
            {
                var categoryId = categoryIdList.First();

                var parentCategory = categoryService.Get( categoryId );

                var childCategories = categoryService.GetAllDescendents( categoryId ).Reverse().ToList();

                foreach ( var childCategory in childCategories )
                {
                    categoryService.Delete( childCategory );
                    dataContext.SaveChanges();

                    if ( categoryIdList.Contains( childCategory.Id ) )
                    {
                        categoryIdList.Remove( childCategory.Id );
                    }
                }

                categoryService.Delete( parentCategory );
                dataContext.SaveChanges();

                categoryIdList.Remove( categoryId );
            }

            // Remove Categories associated with the current test record tag.
            //            var recordsDeleted = dataContext.Database.ExecuteSqlCommand( $"sdelete from [Category] where [ForeignKey] = '{_RecordTag}'" );

            Debug.Print( $"Delete Test Data: {recordsDeleted} Categories deleted." );

            return recordsDeleted;
        }
    }
}
