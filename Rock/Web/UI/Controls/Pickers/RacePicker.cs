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
    /// Control that can be used to select a person's race
    /// </summary>
    public class RacePicker : RockDropDownList, IRacePicker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RacePicker"/> class.
        /// </summary>
        public RacePicker() : base()
        {
            Label = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_RACE_LABEL, "Race" );
            LoadItems( this, true );
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
                LoadItems( this, true );
            }
        }

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        /// <param name="picker">The picker.</param>
        /// <param name="includeEmptyOption">if set to <c>true</c> [include empty option].</param>
        private void LoadItems( IRacePicker picker, bool includeEmptyOption )
        {
            var selectedItems = picker.Items.Cast<ListItem>()
                .Where( i => i.Selected )
                .Select( i => i.Value ).AsIntegerList();

            picker.Items.Clear();

            if ( includeEmptyOption )
            {
                // add Empty option first
                picker.Items.Add( new ListItem() );
            }

            var races = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_RACE ).DefinedValues;

            foreach ( var race in races )
            {
                var li = new ListItem( race.Value, race.Id.ToString() )
                {
                    Selected = selectedItems.Contains( race.Id )
                };
                picker.Items.Add( li );
            }
        }
    }

    /// <summary>
    /// Interface used by race pickers
    /// </summary>
    public interface IRacePicker
    {
        /// <summary>
        /// Gets the items.
        /// </summary>
        ListItemCollection Items { get; }
    }
}
