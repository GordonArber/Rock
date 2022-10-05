<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InteractiveExperienceList.ascx.cs" Inherits="RockWeb.Blocks.Event.InteractiveExperience.InteractiveExperienceList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-list"></i>Experiences
                </h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfExperiences" runat="server">
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" ForceVisible="true" />
                        <Rock:RockCheckBox ID="cbShowInactive" runat="server" Checked="false" Label="Include Inactive" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gExperienceList" runat="server" AllowSorting="true" OnRowSelected="gExperienceList_RowSelected" CssClass="js-experience-list">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="js-experience-name" />
                            <Rock:DateTimeField DataField="NextStartDateTime" HeaderText="Next Start" SortExpression="NextStartDateTime" />
                            <Rock:RockBoundField DataField="ActionCount" HeaderText="Actions" SortExpression="ActionCount" />
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:LinkButtonField>
                                <ItemTemplate>
                                    <a class="btn btn-default btn-sm" href="#">
                                        <i class="fa fa-desktop"></i>
                                    </a>
                                </ItemTemplate>
                            </Rock:LinkButtonField>
                            <Rock:DeleteField OnClick="gExperienceList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
