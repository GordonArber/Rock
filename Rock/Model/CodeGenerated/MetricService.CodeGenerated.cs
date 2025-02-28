//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Metric Service class
    /// </summary>
    public partial class MetricService : Service<Metric>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public MetricService(RockContext context) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Metric item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }

    [HasQueryableAttributes( typeof( Metric.MetricQueryableAttributeValue ), nameof( MetricAttributeValues ) )]
    public partial class Metric
    {
        /// <summary>
        /// Gets the entity attribute values. This should only be used inside
        /// LINQ statements when building a where clause for the query. This
        /// property should only be used inside LINQ statements for filtering
        /// or selecting values. Do <b>not</b> use it for accessing the
        /// attributes after the entity has been loaded.
        /// </summary>
        public virtual ICollection<MetricQueryableAttributeValue> MetricAttributeValues { get; set; } 

        /// <inheritdoc/>
        public class MetricQueryableAttributeValue : QueryableAttributeValue
        {
        }
    }

    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class MetricExtensionMethods
    {
        /// <summary>
        /// Clones this Metric object to a new Metric object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static Metric Clone( this Metric source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as Metric;
            }
            else
            {
                var target = new Metric();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this Metric object to a new Metric object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static Metric CloneWithoutIdentity( this Metric source )
        {
            var target = new Metric();
            target.CopyPropertiesFrom( source );

            target.Id = 0;
            target.Guid = Guid.NewGuid();
            target.ForeignKey = null;
            target.ForeignId = null;
            target.ForeignGuid = null;
            target.CreatedByPersonAliasId = null;
            target.CreatedDateTime = RockDateTime.Now;
            target.ModifiedByPersonAliasId = null;
            target.ModifiedDateTime = RockDateTime.Now;

            return target;
        }

        /// <summary>
        /// Copies the properties from another Metric object to this Metric object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this Metric target, Metric source )
        {
            target.Id = source.Id;
            target.AdminPersonAliasId = source.AdminPersonAliasId;
            target.AutoPartitionOnPrimaryCampus = source.AutoPartitionOnPrimaryCampus;
            target.DataViewId = source.DataViewId;
            target.Description = source.Description;
            target.EnableAnalytics = source.EnableAnalytics;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.IconCssClass = source.IconCssClass;
            target.IsCumulative = source.IsCumulative;
            target.IsSystem = source.IsSystem;
            target.LastRunDateTime = source.LastRunDateTime;
            target.MeasurementClassificationValueId = source.MeasurementClassificationValueId;
            target.MetricChampionPersonAliasId = source.MetricChampionPersonAliasId;
            target.NumericDataType = source.NumericDataType;
            target.ScheduleId = source.ScheduleId;
            target.SourceLava = source.SourceLava;
            target.SourceSql = source.SourceSql;
            target.SourceValueTypeId = source.SourceValueTypeId;
            target.Subtitle = source.Subtitle;
            target.Title = source.Title;
            target.UnitType = source.UnitType;
            target.XAxisLabel = source.XAxisLabel;
            target.YAxisLabel = source.YAxisLabel;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}
