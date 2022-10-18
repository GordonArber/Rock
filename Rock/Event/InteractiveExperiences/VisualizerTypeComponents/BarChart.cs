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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;

namespace Rock.Event.InteractiveExperiences.ActionTypeComponents
{
    /// <summary>
    /// The Short Answer action type. This displays a question and allows the
    /// individual to provide a short text answer.
    /// </summary>
    [Description( "Displays the answers as a bar chart." )]
    [Export( typeof( VisualizerTypeComponent ) )]
    [ExportMetadata( "ComponentName", "Bar Chart" )]

    [CustomDropdownListField( "Orientation",
        DefaultValue = "horizontal",
        ListSource = "horizontal^Horizontal,vertical^Vertical",
        IsRequired = true,
        Key = AttributeKey.Orientation )]

    [Rock.SystemGuid.EntityTypeGuid( "b1dfd377-9ef7-407f-9097-6206b98aec0d" )]
    internal class BarChart : VisualizerTypeComponent
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Orientation = "visualizerOrientation";
        }

        #endregion
    }
}
