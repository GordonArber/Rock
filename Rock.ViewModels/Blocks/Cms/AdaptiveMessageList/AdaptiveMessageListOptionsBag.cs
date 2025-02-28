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

namespace Rock.ViewModels.Blocks.Cms.AdaptiveMessageList
{
    /// <summary>
    /// 
    /// </summary>
    public class AdaptiveMessageListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the grid should be visible.
        /// The grid will not be displayed if the 'FilterCategoryFromQueryString' block setting is set to true
        /// and no CategoryId is provided in the URL.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is grid visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsGridVisible { get; set; }
    }
}
