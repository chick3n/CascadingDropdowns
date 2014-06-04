<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Control Language="C#" AutoEventWireup="true" 
    Inherits="FlyingHippo.CascadingDropdowns.Fields.CascadeDropdownFieldEditor, $SharePoint.Project.AssemblyFullName$" 
    CompilationMode="Always" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/InputFormControl.ascx" %>


        <wssuc:InputFormControl runat="server" LabelText="Cascading Type" LabelAssociatedControlId="rblCascadeType">
            <Template_Control>
                <table>
                    <tr>
                        <td>
                            <asp:RadioButtonList runat="server" ID="rblCascadeType" AutoPostBack="true">
                                <asp:ListItem Text="List" Value="List">List</asp:ListItem>
                                <asp:ListItem Text="Child" Value="Child">Child</asp:ListItem>
                            </asp:RadioButtonList>
                        </td>
                    </tr>
                </table>
            </Template_Control>
        </wssuc:InputFormControl>

        <wssuc:InputFormControl runat="server" LabelText="Parent List.">
            <Template_Control>
                <table>
                    <tr>
                        <td><asp:DropDownList ID="ddlParentList" runat="server" AutoPostBack="true" /></td>
                        <td><asp:DropDownList ID="ddlParentListDisplayName" runat="server" AutoPostBack="false" /></td>
                    </tr>
                </table>
            </Template_Control>
        </wssuc:InputFormControl>

        <wssuc:InputFormControl runat="server" LabelText="Cascade Parent">
            <Template_Control>
                <table>
                    <tr>
                        <td><asp:DropDownList ID="ddlChildList" runat="server" AutoPostBack="true" /></td>
                    </tr>
                    <tr>
                        <td><asp:DropDownList ID="ddlChildLookup" runat="server" AutoPostBack="true" /></td>
                        <td><asp:DropDownList ID="ddlChildLookupDisplayName" runat="server" AutoPostBack="false" /></td>
                    </tr>
                </table>
            </Template_Control>
        </wssuc:InputFormControl>