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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyValueList : ValueList
    {
        /// <summary>
        /// Regular expression pattern used to encode special characters '^', '|', and ','.
        /// </summary>
        private static readonly Regex _encodeExpression = new Regex( "[\\^\\|\\,]" );

        #region Properties

        /// <summary>
        /// Gets or sets the key prompt.
        /// </summary>
        /// <value>
        /// The key prompt.
        /// </value>
        public string KeyPrompt
        {
            get { return ViewState["KeyPrompt"] as string ?? "Key"; }
            set { ViewState["KeyPrompt"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display value first].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display value first]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayValueFirst
        {
            get { return ViewState["DisplayValueFirst"] as bool? ?? false; }
            set { ViewState["DisplayValueFirst"] = value; }
        }

        /// <summary>
        /// Gets or sets the custom keys.
        /// </summary>
        /// <value>
        /// The custom keys.
        /// </value>
        public Dictionary<string, string> CustomKeys
        {
            get { return ViewState["CustomKeys"] as Dictionary<string, string>; }
            set { ViewState["CustomKeys"] = value; }
        }

        #endregion

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            Dictionary<string, string> values = null;
            if ( DefinedTypeId.HasValue )
            {
                values = new Dictionary<string, string>();
                new DefinedValueService( new RockContext() )
                    .GetByDefinedTypeId( DefinedTypeId.Value )
                    .Where( v => v.IsActive )
                    .ToList()
                    .ForEach( v => values.Add( v.Id.ToString(), v.Value ) );
            } 
            else if ( CustomValues != null )
            {
                values = CustomValues;
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "key-value-list " + this.CssClass );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.WriteLine();

            _hfValue.RenderControl( writer );
            _hfValueDisableVrm.RenderControl( writer );

            writer.WriteLine();

            StringBuilder html = new StringBuilder();
            html.Append( @"<div class=""controls controls-row form-control-group"">");
            
            // write key/value html
            if ( this.DisplayValueFirst )
            {
                WriteValueHtml( html, values );
                WriteKeyHtml( html );
            }
            else
            {
                WriteKeyHtml( html );
                WriteValueHtml( html, values );
            }
            

            html.Append( @"<a href=""#"" class=""btn btn-sm btn-danger key-value-remove""><i class=""fa fa-times""></i></a></div>" );

            var hfValueHtml = new HtmlInputHidden();
            hfValueHtml.AddCssClass( "js-value-html" );
            hfValueHtml.Value = html.ToString();
            hfValueHtml.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "key-value-rows" );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            writer.WriteLine();

            string[] nameValues = this.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string nameValue in nameValues )
            {
                string[] nameAndValue = nameValue.Split( new char[] { '^' } );
                nameAndValue = nameAndValue.Select( s => HttpUtility.UrlDecode( s ) ).ToArray(); // url decode array items

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "controls controls-row form-control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.WriteLine();

                if ( DisplayValueFirst )
                {
                    WriteValueControls( writer, nameAndValue, values );
                    WriteKeyControls( writer, nameAndValue );
                }
                else
                {
                    WriteKeyControls( writer, nameAndValue );
                    WriteValueControls( writer, nameAndValue, values );
                }

                // Write Remove Button
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-sm btn-danger key-value-remove" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-times" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.WriteLine();

                writer.RenderEndTag();
                writer.WriteLine();
            }

            writer.RenderEndTag();
            writer.WriteLine();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-actions" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            var addButtonCssClass = "btn btn-action btn-square btn-xs key-value-add";
            if ( !this.Enabled )
            {
                addButtonCssClass += " aspNetDisabled disabled";
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, addButtonCssClass );
            writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
            writer.RenderBeginTag( HtmlTextWriterTag.A );
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "fa fa-plus-circle");
            writer.RenderBeginTag( HtmlTextWriterTag.I );
            
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.WriteLine();

            writer.RenderEndTag();
            writer.WriteLine();

            if ( this.ValueChanged != null )
            {
                var postBackChangedscript = this.Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "ValueChanged" ), true );
                postBackChangedscript = postBackChangedscript.Replace( '\'', '"' );
                var script = string.Format( @"function raiseKeyValueList(){{ window.location = 'javascript: {0}'; }}", postBackChangedscript );
                ScriptManager.RegisterStartupScript( this, this.GetType(), "key-value-script" + this.ClientID, script, true );
            }
        }

        /// <summary>
        /// Occurs when [value changed].
        /// </summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Writes the key controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="nameAndValue">The name and value.</param>
        private void WriteKeyControls( HtmlTextWriter writer, string[] nameAndValue )
        {
            if ( CustomKeys != null && CustomKeys.Any() )
            {
                DropDownList ddl = new DropDownList();
                ddl.AddCssClass( "key-value-key form-control input-width-md js-key-value-input" );
                ddl.DataTextField = "Value";
                ddl.DataValueField = "Key";
                ddl.DataSource = CustomKeys;
                ddl.DataBind();
                if ( nameAndValue.Length >= 1 )
                {
                    ddl.SelectedValue = nameAndValue[0];
                }
                ddl.RenderControl( writer );
            }
            else
            {
                // Write Name
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "key-value-key form-control input-width-md js-key-value-input" );
                writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                writer.AddAttribute( HtmlTextWriterAttribute.Value, nameAndValue.Length >= 1 ? nameAndValue[0] : string.Empty );
                writer.AddAttribute( "placeholder", KeyPrompt );
                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();
            }

            writer.Write( " " );
            writer.WriteLine();
        }

        /// <summary>
        /// Writes the value controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="nameAndValue">The name and value.</param>
        /// <param name="values">The values.</param>
        private void WriteValueControls( HtmlTextWriter writer, string[] nameAndValue, Dictionary<string, string> values )
        {
            if ( values != null && values.Any() )
            {
                DropDownList ddl = new DropDownList();
                ddl.AddCssClass( "key-value-value form-control input-width-md js-key-value-input" );
                ddl.DataTextField = "Value";
                ddl.DataValueField = "Key";
                ddl.DataSource = values;
                ddl.DataBind();
                if ( nameAndValue.Length >= 2 )
                {
                    ddl.SelectedValue = HttpUtility.UrlDecode( nameAndValue[1] );
                }
                ddl.RenderControl( writer );
            }
            else
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "key-value-value form-control input-width-md js-key-value-input" );
                writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                writer.AddAttribute( HtmlTextWriterAttribute.Value, nameAndValue.Length >= 2 ? nameAndValue[1] : string.Empty );
                writer.AddAttribute( "placeholder", ValuePrompt );
                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();
            }

            writer.Write( " " );
            writer.WriteLine();
        }

        /// <summary>
        /// Writes the key HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        private void WriteKeyHtml( StringBuilder html )
        {
            if ( CustomKeys != null && CustomKeys.Any() )
            {
                html.Append( @"<select class=""key-value-key form-control input-width-md js-key-value-input"">" );
                foreach ( var key in CustomKeys )
                {
                    html.AppendFormat( @"<option value=""{0}"">{1}</option>", key.Key, key.Value );
                }
                html.Append( @"</select>" );
            }
            else
            {
                html.AppendFormat( @"<input class=""key-value-key form-control js-key-value-input"" type=""text"" placeholder=""{0}""></input> ", KeyPrompt );
            }
        }

        /// <summary>
        /// Writes the value HTML.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="values">The values.</param>
        private void WriteValueHtml( StringBuilder html, Dictionary<string,string> values )
        {
            if ( values != null && values.Any() )
            {
                html.Append( @"<select class=""key-value-value form-control input-width-md js-key-value-input"">" );
                foreach ( var value in values )
                {
                    html.AppendFormat( @"<option value=""{0}"">{1}</option>", value.Key, value.Value );
                }
                html.Append( @"</select>" );
            }
            else
            {
                html.AppendFormat( @"<input class=""key-value-value form-control js-key-value-input"" type=""text"" placeholder=""{0}""></input>", ValuePrompt );
            }
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument == "ValueChanged" )
            {
                if ( ValueChanged != null )
                {
                    ValueChanged( this, new EventArgs() );
                }
            }
        }

        /// <summary>
        /// Sets the value of the control from the dictionary of key/value
        /// pairs.
        /// </summary>
        /// <param name="values">The dictionary containing the values to set.</param>
        public void SetValue( IDictionary<string, string> values )
        {
            if ( values == null )
            {
                Value = null;
                return;
            }

            var pairs = values.Select( pair => $"{Encode( pair.Key )}^{Encode( pair.Value )}" );

            Value = string.Join( "|", pairs );
        }

        /// <summary>
        /// Retrieves the keys and values from the control and returns them
        /// as a dictionary.
        /// </summary>
        /// <returns>A dictionary containing the values from the list.</returns>
        public Dictionary<string, string> GetValueAsDictionary()
        {
            return Value?.AsDictionary() ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Retrieves the keys and values from the control and returns them
        /// as a dictionary.
        /// </summary>
        /// <returns>A dictionary containing the values from the list or <c>null</c> if there were no items.</returns>
        public Dictionary<string, string> GetValueAsDictionaryOrNull()
        {
            return Value?.AsDictionaryOrNull();
        }

        /// <summary>
        /// Encodes the specified string so it can be used as a key or value
        /// component in the value passed to JavaScript.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>A new string.</returns>
        internal static string Encode( string str )
        {
            // Replace "^", "|" and "," with an encoded value.
            return _encodeExpression.Replace( str, m =>
            {
                // WebUtility.UrlEncode doesn't encode comma, so do it manually.
                return $"%{( byte ) m.Value[0]:X2}";
            } );
        }
    }
}