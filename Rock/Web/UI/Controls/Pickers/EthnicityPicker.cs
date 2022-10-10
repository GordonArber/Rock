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
using Rock.Web.Cache;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a person's ethnicity
    /// </summary>
    public class EthnicityPicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EthnicityPicker"/> class.
        /// </summary>
        public EthnicityPicker() : base()
        {
            Label = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_ETHNICITY_LABEL, "Ethnicity" );
            LoadItems( true );
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack && Items.Count == 0 )
            {
                LoadItems( true );
            }
        }

        private void LoadItems( bool includeEmptyOption )
        {
            var selectedItems = this.Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value ).AsIntegerList();

            this.Items.Clear();

            if ( includeEmptyOption )
            {
                // add Empty option first
                this.Items.Add( new ListItem() );
            }

            var ethnicities = DefinedValueCache.All().Where( dv => dv.DefinedType.Guid == Rock.SystemGuid.DefinedType.PERSON_ETHNICITY.AsGuid() );

            foreach ( var ethnicity in ethnicities )
            {
                var listItem = new ListItem( ethnicity.Value, ethnicity.Id.ToString() )
                {
                    Selected = selectedItems.Contains( ethnicity.Id )
                };
                this.Items.Add( listItem );
            }
        }
    }
}
