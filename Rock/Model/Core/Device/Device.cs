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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a device or component that interacts with and is manageable through Rock.  Examples of these can be check-in kiosks, giving kiosks, label printers, badge printers,
    /// displays, etc.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "Device" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "C06EE1FE-AF12-410A-A364-7A366CD72414")]
    public partial class Device : Model<Device>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the device name. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the Name of the device.
        /// </value>
        [Required]
        [Index( IsUnique = true )]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the device.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the device.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Id of the DeviceType <see cref="Rock.Model.DefinedValue"/> that identifies
        /// what type of device this is.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Device Type <see cref="Rock.Model.DefinedValue"/>
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.DEVICE_TYPE )]
        [EnableAttributeQualification]
        public int DeviceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Location"/> where this device is located at.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Location"/> where this device is located at.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the device.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the IP address of the device.
        /// </value>
        [MaxLength( 45 )]
        [DataMember]
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the DeviceId of the printer that is associated with this device. This is mostly used if this device is a kiosk.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DeviceId of the printer that is associated with this device. If there is not a printer 
        /// associated with this Device, this value will be null.
        /// </value>
        [DataMember]
        public int? PrinterDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the device that will handle proxying commands
        /// to this device. Currently this means a printer proxy.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the DeviceId.
        /// </value>
        [DataMember]
        public int? ProxyDeviceId { get; set; }

        /// <summary>
        /// Gets or sets where print jobs for this device originates from.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PrintFrom"/> to indicate how print jobs should be handled from this device. If <c>PrintFrom.Client</c> the print job will
        /// be handled from the client, otherwise <c>PrintFrom.Server</c> and the print job will be handled from the server.
        /// </value>
        [DataMember]
        public PrintFrom PrintFrom { get; set; }

        /// <summary>
        /// Gets or sets a flag that overrides which printer the print job is set to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PrintTo"/> that indicates overrides where the print job is set to.  If <c>PrintTo.Default</c> the print job will be sent to the default
        /// printer, if <c>PrintTo.Kiosk</c> the print job will be sent to the printer associated with the kiosk, if <c>PrintTo.Location</c> the print job will be sent to the 
        /// printer at the check in location.
        /// </value>
        [DataMember]
        public PrintTo PrintToOverride { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance has camera.
        /// Only applies when <see cref="DeviceTypeValueId" /> is Checkin-Kiosk.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has camera; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool HasCamera { get; set; }

        /// <summary>
        /// Gets or sets the camera barcode configuration.
        /// This is currently only used for reading barcodes on iPads.
        /// </summary>
        /// <value>
        /// The type of the camera barcode configuration.
        /// </value>
        [DataMember]
        public CameraBarcodeConfiguration? CameraBarcodeConfigurationType { get; set; }

        /// <summary>
        /// The type of checkin client this Check-in Kiosk could be using.
        /// Only applies when <see cref="DeviceTypeValueId" /> is Checkin-Kiosk.
        /// </summary>
        /// <value>The type of the kiosk.</value>
        [DataMember]
        public KioskType? KioskType { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the physical location or geographic fence for the device.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Location"/> entity that represents the physical location of or the geographic fence for the device.
        /// </value>
        /// <remarks>
        /// A physical location would signify where the device is at. A situation where a geographic fence could be used would be for mobile check in, 
        /// where if the device is within the fence, a user would be able to check in from their mobile device.
        /// </remarks>
        [LavaVisible]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.Location">Locations</see> that use this device.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Location">Locations</see> that use this device.
        /// </value>
        [LavaVisible]
        public virtual ICollection<Location> Locations
        {
            get { return _locations ?? ( _locations = new Collection<Location>() ); }
            set { _locations = value; }
        }

        private ICollection<Location> _locations;

        /// <summary>
        /// Gets or sets the printer that is associated with this device. 
        /// </summary>
        /// <value>
        /// The printer that is associated with the device.
        /// </value>
        [LavaVisible]
        public virtual Device PrinterDevice { get; set; }

        /// <summary>
        /// Gets or sets the proxy that is associated with this device. 
        /// </summary>
        /// <value>
        /// The proxy that is associated with the device.
        /// </value>
        [LavaVisible]
        public virtual Device ProxyDevice { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> that represents the type of the device.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> that represents the type of the device.
        /// </value>
        [DataMember]
        public virtual DefinedValue DeviceType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class DeviceConfiguration : EntityTypeConfiguration<Device>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceConfiguration"/> class.
        /// </summary>
        public DeviceConfiguration()
        {
            this.HasOptional( d => d.Location ).WithMany().HasForeignKey( d => d.LocationId ).WillCascadeOnDelete( false );
            this.HasMany( d => d.Locations ).WithMany().Map( d => { d.MapLeftKey( "DeviceId" ); d.MapRightKey( "LocationId" ); d.ToTable( "DeviceLocation" ); } );
            this.HasOptional( d => d.PrinterDevice ).WithMany().HasForeignKey( d => d.PrinterDeviceId ).WillCascadeOnDelete( false );
            this.HasOptional( d => d.ProxyDevice ).WithMany().HasForeignKey( d => d.ProxyDeviceId ).WillCascadeOnDelete( false );
            this.HasRequired( d => d.DeviceType ).WithMany().HasForeignKey( d => d.DeviceTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
