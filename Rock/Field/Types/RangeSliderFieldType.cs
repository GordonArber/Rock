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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to select an integer value using a slider
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Advanced )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.RANGE_SLIDER )]
    [IconSvg( @"<svg viewBox=""0 0 16 16"" xmlns=""http://www.w3.org/2000/svg""><path d=""M14.5625 6.875H12.375V5.4375C12.375 5.19688 12.1781 5 11.9375 5H11.0625C10.8219 5 10.625 5.19688 10.625 5.4375V6.875H1.4375C1.19688 6.875 1 7.07188 1 7.3125V8.1875C1 8.42812 1.19688 8.625 1.4375 8.625H10.625V10.0625C10.625 10.3031 10.8219 10.5 11.0625 10.5H11.9375C12.1781 10.5 12.375 10.3031 12.375 10.0625V8.625H14.5625C14.8031 8.625 15 8.42812 15 8.1875V7.3125C15 7.07188 14.8031 6.875 14.5625 6.875Z"" /></svg>" )]
    public class RangeSliderFieldType : FieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            try
            {
                int? intValue = ( int? ) privateValue.AsDecimalOrNull();
                return intValue.ToString();
            }
            catch ( System.OverflowException )
            {
                return "Not a valid integer";
            }
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        private int? GetMinValue( Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( configurationValues != null && configurationValues.ContainsKey( "min" ) )
            {
                return configurationValues["min"].Value.AsIntegerOrNull();
            }

            return null;
        }

        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        private int? GetMaxValue( Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( configurationValues != null && configurationValues.ContainsKey( "max" ) )
            {
                return configurationValues["max"].Value.AsIntegerOrNull();
            }

            return null;
        }

        #endregion

        #region Edit Control

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get { return ComparisonHelper.NumericFilterComparisonTypes; }
        }

        /// <summary>
        /// Gets the filter format script.
        /// </summary>
        /// <param name="configurationValues"></param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        /// <remarks>
        /// This script must set a javascript variable named 'result' to a friendly string indicating value of filter controls
        /// a '$selectedContent' should be used to limit script to currently selected filter fields
        /// </remarks>
        public override string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title )
        {
            string titleJs = System.Web.HttpUtility.JavaScriptStringEncode( title );
            return string.Format( "result = '{0} ' + $('select', $selectedContent).find(':selected').text() + ( $('.js-filter-control', $selectedContent).filter(':visible').length ?  (' \\'' +  $('input', $selectedContent).val()  + '\\'') : '' )", titleJs );
        }

        /// <summary>
        /// Gets a filter expression for an attribute value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            if ( filterValues.Count == 1 )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, "ValueAsNumeric" );
                ComparisonType comparisonType = ComparisonType.EqualTo;
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, AttributeConstantExpression( filterValues[0] ) );
            }

            return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
        }

        /// <summary>
        /// Determines whether the filter's comparison type and filter compare value(s) evaluates to true for the specified value
        /// </summary>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is compared to value] [the specified filter values]; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsComparedToValue( List<string> filterValues, string value )
        {
            if ( filterValues == null || filterValues.Count < 2 )
            {
                return false;
            }

            ComparisonType? filterComparisonType = filterValues[0].ConvertToEnumOrNull<ComparisonType>();
            ComparisonType? equalToCompareValue = GetEqualToCompareValue().ConvertToEnumOrNull<ComparisonType>();
            var filterValueAsDecimal = filterValues[1].AsDecimalOrNull();
            var valueAsDecimal = value.AsDecimalOrNull();

            return ComparisonHelper.CompareNumericValues( filterComparisonType.Value, valueAsDecimal, filterValueAsDecimal, null );
        }

        /// <summary>
        /// Gets the name of the attribute value field that should be bound to (Value, ValueAsDateTime, ValueAsBoolean, or ValueAsNumeric)
        /// </summary>
        /// <value>
        /// The name of the attribute value field.
        /// </value>
        public override string AttributeValueFieldName
        {
            get
            {
                return "ValueAsNumeric";
            }
        }

        /// <summary>
        /// Attributes the constant expression.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override ConstantExpression AttributeConstantExpression( string value )
        {
            return Expression.Constant( value.AsDecimal(), typeof( decimal ) );
        }

        /// <summary>
        /// Gets the type of the attribute value field.
        /// </summary>
        /// <value>
        /// The type of the attribute value field.
        /// </value>
        public override Type AttributeValueFieldType
        {
            get
            {
                return typeof( decimal? );
            }
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object ValueAsFieldType( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return value.AsDecimalOrNull();
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // return ValueAsFieldType which returns the value as a Decimal
            return this.ValueAsFieldType( parentControl, value, configurationValues );
        }

        /// <summary>
        /// Gets the align value that should be used when displaying value
        /// </summary>
        public override HorizontalAlign AlignValue
        {
            get
            {
                return HorizontalAlign.Right;
            }
        }


        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( "min" );
            configKeys.Add( "max" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var nbMin = new NumberBox();
            controls.Add( nbMin );
            nbMin.NumberType = ValidationDataType.Integer;
            nbMin.AutoPostBack = true;
            nbMin.TextChanged += OnQualifierUpdated;
            nbMin.Label = "Min Value";
            nbMin.Help = "The minimum value allowed for the slider range";

            var nbMax = new NumberBox();
            controls.Add( nbMax );
            nbMax.NumberType = ValidationDataType.Integer;
            nbMax.AutoPostBack = true;
            nbMax.TextChanged += OnQualifierUpdated;
            nbMax.Label = "Max Value";
            nbMax.Help = "The maximum value allowed for the slider range";
            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( "min", new ConfigurationValue( "Min Value", "The minimum value allowed for the slider range", string.Empty ) );
            configurationValues.Add( "max", new ConfigurationValue( "Max Value", "The maximum value allowed for the slider range", string.Empty ) );

            if ( controls != null && controls.Count == 2 )
            {
                var nbMin = controls[0] as NumberBox;
                var nbMax = controls[1] as NumberBox;
                if ( nbMin != null )
                {
                    configurationValues["min"].Value = nbMin.Text;
                }

                if ( nbMax != null )
                {
                    configurationValues["max"].Value = nbMax.Text;
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 2 && configurationValues != null )
            {
                var nbMin = controls[0] as NumberBox;
                var nbMax = controls[1] as NumberBox;
                if ( nbMin != null && configurationValues.ContainsKey( "min" ) )
                {
                    nbMin.Text = configurationValues["min"].Value;
                }

                if ( nbMax != null && configurationValues.ContainsKey( "max" ) )
                {
                    nbMax.Text = configurationValues["max"].Value;
                }
            }
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new RangeSlider { ID = id, MinValue = GetMinValue( configurationValues ), MaxValue = GetMaxValue( configurationValues ) };
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var panel = new Panel();
            panel.ID = string.Format( "{0}_ctlComparePanel", id );
            panel.AddCssClass( "js-filter-control" );

            var control = EditControl( configurationValues, id );
            control.ID = string.Format( "{0}_ctlCompareValue", id );
            panel.Controls.Add( control );

            return panel;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return base.GetFilterValueValue( control.Controls[0], configurationValues );
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            base.SetFilterValueValue( control.Controls[0], configurationValues, value );
        }

#endif
        #endregion
    }
}