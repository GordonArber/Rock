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

namespace Rock.ViewModels.Blocks.Lms.LearningClassAnnouncementDetail
{
    /// <summary>
    /// The additional configuration options for the Learning Class Announcement Detail block.
    /// </summary>
    public class LearningClassAnnouncementDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the character limit for the configured SMS medium.
        /// </summary>
        public int SmsCharacterLimit { get; set; }
    }
}
