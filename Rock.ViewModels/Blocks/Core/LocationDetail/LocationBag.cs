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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.LocationDetail
{
    public class LocationBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets threshold that will prevent checkin (no option to override)
        /// </summary>
        public int? FirmRoomThreshold { get; set; }

        /// <summary>
        /// Gets or sets the image identifier.
        /// </summary>
        public int? ImageId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets flag indicating if GeoPoint is locked (shouldn't be geocoded again)
        /// </summary>
        public bool? IsGeoPointLocked { get; set; }

        /// <summary>
        /// Gets or sets the Id of the LocationType Rock.Model.DefinedValue that is used to identify the type of Rock.Model.Location
        /// that this is. Examples: Campus, Building, Room, etc
        /// </summary>
        public ListItemBag LocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the Location's Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the if the location's parent Location. 
        /// </summary>
        public ListItemBag ParentLocation { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Device Id of the printer (if any) associated with the location.
        /// </summary>
        public int? PrinterDeviceId { get; set; }

        /// <summary>
        /// Gets or sets a threshold that will prevent checkin unless a manager overrides
        /// </summary>
        public int? SoftRoomThreshold { get; set; }

        /// <summary>
        /// Gets or sets the city component of the Location's Street/Mailing Address.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the country component of the Location's Street/Mailing Address. 
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the county.
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// Gets or sets the State component of the Location's Street/Mailing Address.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the first line of the Location's Street/Mailing Address.
        /// </summary>
        public string Street1 { get; set; }

        /// <summary>
        /// Gets or sets the second line of the Location's Street/Mailing Address. 
        /// </summary>
        public string Street2 { get; set; }

        public string PostalCode { get; set; }
    }
}
