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
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// </summary>
    [DisplayName( "Contribution Statement Generator" )]
    [Category( "Finance" )]
    [Description( "Block for generating a Contribution Statement" )]


    [BooleanField(
        "Allow Person QueryString",
        Key = AttributeKey.AllowPersonQueryString,
        Description = "Determines if any person other than the currently logged in person is allowed to be passed through the query string. For security reasons this is not allowed by default.",
        DefaultBooleanValue = false,
        Order = 0 )]

    [FinancialStatementTemplateField(
        "Statement Template",
        Key = AttributeKey.FinancialStatementTemplate,
        DefaultValue = Rock.SystemGuid.FinancialStatementTemplate.ROCK_DEFAULT,
        Order = 1 )]

    [Rock.SystemGuid.BlockTypeGuid( "E0A699C3-61AA-4522-9067-1FE56FA80972" )]
    public partial class ContributionStatementGenerator : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string FinancialStatementTemplate = "FinancialStatementTemplate";
            public const string AllowPersonQueryString = "AllowPersonQueryString";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string StatementYear = "StatementYear";
            public const string StatementStartMonth = "StatementStartMonth";
            public const string StatementEndMonth = "StatementEndMonth";
            public const string PersonActionIdentifier = "rckid";
            public const string PersonGuid = "PersonGuid";
        }

        #endregion PageParameterKeys

        #region Events

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                DisplayResults();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DisplayResults();
        }

        #endregion

        #region Methods

        private void DisplayResults()
        {
            Person targetPerson = CurrentPerson;

            RockContext rockContext = new RockContext();
                        
            var statementYear = PageParameter( PageParameterKey.StatementYear ).AsIntegerOrNull() ?? RockDateTime.Now.Year;
            var statementStartMonth = PageParameter( PageParameterKey.StatementStartMonth ).AsIntegerOrNull() ?? 0;
            var statementEndMonth = PageParameter( PageParameterKey.StatementEndMonth ).AsIntegerOrNull() ?? 0;
            var personActionId = PageParameter( PageParameterKey.PersonActionIdentifier );
            var personGuid = PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();
            var allowPersonQueryString = GetAttributeValue( AttributeKey.AllowPersonQueryString ).AsBoolean();

            if ( personActionId.IsNotNullOrWhiteSpace() )
            {
                var person = new PersonService( rockContext ).GetByPersonActionIdentifier( personActionId, "contribution-statement" );
                var isCurrentPersonsBusiness = targetPerson != null && targetPerson.GetBusinesses().Any( b => b.Guid == person.Guid );
                if ( person != null && ( allowPersonQueryString || isCurrentPersonsBusiness ) )
                {
                    targetPerson = person;
                }
            }
            else if ( personGuid.HasValue )
            {
                // if "AllowPersonQueryString is False", only use the PersonGuid if it is a Guid of one of the current person's businesses
                var isCurrentPersonsBusiness = targetPerson != null && targetPerson.GetBusinesses().Any( b => b.Guid == personGuid.Value );

                if ( allowPersonQueryString || isCurrentPersonsBusiness )
                {
                    var person = new PersonService( rockContext ).Get( personGuid.Value );
                    if ( person != null )
                    {
                        targetPerson = person;
                    }
                }
            }

            if ( targetPerson == null )
            {
                Response.StatusCode = ( int ) HttpStatusCode.BadRequest;
                Response.Write( "Invalid Person" );
                Response.End();
            }

            FinancialStatementGeneratorOptions financialStatementGeneratorOptions = new FinancialStatementGeneratorOptions();
            var startDate = new DateTime( statementYear, 1, 1 );
            
            // If the statementStartMonth page parameter exists and its value is between 1 and 12 use it, otherwise startDate retains the normal behavior.
            if ( statementStartMonth > 0 && statementStartMonth < 12 )
            {
                startDate = new DateTime( statementYear, statementStartMonth, 1 );
            }
            // If the statementEndMonth page parameter exists and its value is between 1 and 12 use it, otherwise endDate retains the normal behavior.
            var endDate = ( statementEndMonth > 0 && statementEndMonth < 12 ) ? ( new DateTime( statementYear, statementEndMonth, 1 ) ).AddMonths( 1 ) : startDate.AddYears( 1 );
            
            // Ensure that endDate is greater than or equal to startDate
            if ( statementEndMonth > 0 && statementEndMonth < statementStartMonth )
            {
                endDate = startDate.AddYears( 1 ).AddMonths( -1 ); // Checking if endDate is less than the startDate. Removing a month from endDate to create a full year if it is.
            }
            financialStatementGeneratorOptions.StartDate = startDate;
            financialStatementGeneratorOptions.EndDate = endDate;
            financialStatementGeneratorOptions.RenderMedium = "Html";

            var financialStatementTemplateGuid = this.GetAttributeValue( AttributeKey.FinancialStatementTemplate ).AsGuidOrNull() ?? Rock.SystemGuid.FinancialStatementTemplate.ROCK_DEFAULT.AsGuid();

            financialStatementGeneratorOptions.FinancialStatementTemplateId = new FinancialStatementTemplateService( rockContext ).GetId( financialStatementTemplateGuid );

            FinancialStatementGeneratorRecipient financialStatementGeneratorRecipient = new FinancialStatementGeneratorRecipient();

            // It's required that we set the LocationId in order for the GetStatementGeneratorRecipientResult() to
            // fetch all the required data for the Lava.
            financialStatementGeneratorRecipient.LocationId = targetPerson.GetMailingLocation()?.Id;
    
            if ( targetPerson.GivingGroupId.HasValue )
            {
                financialStatementGeneratorRecipient.GroupId = targetPerson.GivingGroupId.Value;
            }
            else
            {
                financialStatementGeneratorRecipient.GroupId = targetPerson.PrimaryFamilyId ?? 0;
                financialStatementGeneratorRecipient.PersonId = targetPerson.Id;
            }

            FinancialStatementGeneratorRecipientRequest financialStatementGeneratorRecipientRequest = new FinancialStatementGeneratorRecipientRequest( financialStatementGeneratorOptions )
            {
                FinancialStatementGeneratorRecipient = financialStatementGeneratorRecipient
            };

            var result = FinancialStatementGeneratorHelper.GetStatementGeneratorRecipientResult( financialStatementGeneratorRecipientRequest, this.CurrentPerson );

            if ( !string.IsNullOrWhiteSpace( result.FooterHtmlFragment ) )
            {
                // Insert the footer text before the closing body tag
                var insertPosition = result.Html.IndexOf( "</body>" );
                if ( insertPosition >= 0 )
                {
                    result.Html = result.Html.Insert( insertPosition, result.FooterHtmlFragment );
                }
                else
                {
                    result.Html += result.FooterHtmlFragment;
                }
            }

            Response.Write( result.Html );
            Response.End();
        }

        #endregion Methods
    }
}
