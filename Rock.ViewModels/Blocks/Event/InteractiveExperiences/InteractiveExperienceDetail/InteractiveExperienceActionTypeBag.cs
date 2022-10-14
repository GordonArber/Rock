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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail
{
    /// <summary>
    /// Identifies a single action type that can be used to configure actions
    /// for an experience by the individual.
    /// </summary>
    public class InteractiveExperienceActionTypeBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for this type of action.
        /// </summary>
        /// <value>The unique identifier for this type of action.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the display name for this type of action.
        /// </summary>
        /// <value>The display name for this type of action.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this type supports question text.
        /// </summary>
        /// <value><c>true</c> if this type supports question text; otherwise, <c>false</c>.</value>
        public bool IsQuestionSupported { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this type supports moderation.
        /// </summary>
        /// <value><c>true</c> if this type supports moderation; otherwise, <c>false</c>.</value>
        public bool IsModerationSupported { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this type allows multiple submissions.
        /// </summary>
        /// <value><c>true</c> if this type allows multiple submissions; otherwise, <c>false</c>.</value>
        public bool IsMultipleSubmissionSupported { get; set; }

        /// <summary>
        /// Gets or sets the attributes that are available on this action type.
        /// </summary>
        /// <value>
        /// The attributes that are available on this action type.
        /// </value>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }
    }
}
