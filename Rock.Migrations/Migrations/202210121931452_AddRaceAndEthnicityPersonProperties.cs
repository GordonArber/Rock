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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AddRaceAndEthnicityPersonProperties : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Person", "Person Race", "Person's race", Rock.SystemGuid.DefinedType.PERSON_RACE );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSON_RACE, "White", "For people of White/Caucasian race", Rock.SystemGuid.DefinedValue.WHITE, false );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSON_RACE, "Black or African American", "For people of Black or African American race", Rock.SystemGuid.DefinedValue.BLACK_OR_AFRICAN_AMERICAN, false );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSON_RACE, "American Indian or Alaska Native", "For people of American Indian or Alaska Native race", Rock.SystemGuid.DefinedValue.AMERICAN_INDIAN_OR_ALASKAN_NATIVE, false );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSON_RACE, "Asian", "For people of Asianrace", Rock.SystemGuid.DefinedValue.ASIAN, false );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSON_RACE, "Native Hawaiian or Pacific Islander", "For people of Native Hawaiian or Pacific Islander race", Rock.SystemGuid.DefinedValue.NATIVE_HAWAIIAN_OR_PACIFIC_ISLANDER, false );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSON_RACE, "Other", "For people of race not specified as an option", Rock.SystemGuid.DefinedValue.OTHER, false );

            RockMigrationHelper.AddDefinedType( "Person", "Person Ethnicity", "Person's Ethnicity", Rock.SystemGuid.DefinedType.PERSON_ETHNICITY );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSON_ETHNICITY, "Hispanic or Latino", "For people of Hispanic or Latino ethnicity", Rock.SystemGuid.DefinedValue.HISPANIC_OR_LATINO, false );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSON_ETHNICITY, "Not Hispanic or Latino", "For people not of Hispanic or Latino ethnicity", Rock.SystemGuid.DefinedValue.NOT_HISPANIC_OR_LATINO, false );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "7525c4cb-ee6b-41d4-9b64-a08048d5a5c0", "GroupTypePurposeValueId", "142", "Display race on children", "", 0, "Hide", "12FFAC55-F8C4-4B73-91A4-D7CAE30CFE3D", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "7525c4cb-ee6b-41d4-9b64-a08048d5a5c0", "GroupTypePurposeValueId", "142", "Display ethnicity on children", "", 0, "Hide", "8B3B904E-981B-4257-ACF3-D06B57BBF93D", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "7525c4cb-ee6b-41d4-9b64-a08048d5a5c0", "GroupTypePurposeValueId", "142", "Display race on adults", "", 0, "Hide", "8408517A-4738-4F8F-91DB-D743CC0070AF", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "7525c4cb-ee6b-41d4-9b64-a08048d5a5c0", "GroupTypePurposeValueId", "142", "Display ethnicity on adults", "", 0, "Hide", "BA9EEB6F-0C35-4392-9049-3723473523D9", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS );

            AddColumn("dbo.Person", "RaceValueId", c => c.Int());
            AddColumn("dbo.Person", "EthnicityValueId", c => c.Int());

            AddColumn("dbo.WorkflowActionForm", "RaceAndEthnicityEntryOption", c => c.Int(nullable: false));
            AddColumn("dbo.WorkflowActionForm", "PersonEntryRaceValueId", c => c.Int());
            AddColumn("dbo.WorkflowActionForm", "PersonEntryEthnicityValueId", c => c.Int());

            CreateIndex("dbo.Person", "RaceValueId");
            CreateIndex("dbo.Person", "EthnicityValueId");
            AddForeignKey("dbo.Person", "EthnicityValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.Person", "RaceValueId", "dbo.DefinedValue", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Person", "RaceValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Person", "EthnicityValueId", "dbo.DefinedValue");
            DropIndex("dbo.Person", new[] { "EthnicityValueId" });
            DropIndex("dbo.Person", new[] { "RaceValueId" });

            DropColumn("dbo.WorkflowActionForm", "PersonEntryEthnicityValueId");
            DropColumn("dbo.WorkflowActionForm", "PersonEntryRaceValueId");
            DropColumn("dbo.WorkflowActionForm", "RaceAndEthnicityEntryOption");

            DropColumn("dbo.Person", "EthnicityValueId");
            DropColumn("dbo.Person", "RaceValueId");

            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.WHITE );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.BLACK_OR_AFRICAN_AMERICAN );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.AMERICAN_INDIAN_OR_ALASKAN_NATIVE );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.ASIAN );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.NATIVE_HAWAIIAN_OR_PACIFIC_ISLANDER );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.OTHER );

            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.PERSON_RACE );

            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.HISPANIC_OR_LATINO );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.NOT_HISPANIC_OR_LATINO );

            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.PERSON_ETHNICITY );
        }
    }
}
