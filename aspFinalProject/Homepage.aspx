<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Homepage.aspx.cs" Inherits="aspFinalProject.Homepage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Homepage | CMS</title>
    <link rel="stylesheet" type="text/css" href="styles/homepage.css" />
    <script type="text/javascript">
            function hideLabel() {
                var d = setTimeout(function () {
                    document.getElementById("<%=lbl_gridCmdReport.ClientID %>").style.display = "none";
                }, 5000);
            };
            function hideTrashLabel() {
                var t = setTimeout(function () {
                    document.getElementById("<%=lbl_trashCmdReport.ClientID %>").style.display = "none";
                }, 5000);
            };
    </script>
</head>
<body>
    <form id="frm_lstOfPages" runat="server">
    <div>
    
        <%-- Dynamically generated menu-bar based on pages created through the CMS --%>
        <nav id="main-menu">
            <ul id="mainLinks" runat="server" class="menu"></ul>
        </nav>

        <%-- Title page & link to allow creation of new pages --%>
        <div>
            <h1>List of Pages</h1>
            <asp:LinkButton ID="btn_linkAddPage" Text="Add New" runat="server" PostBackUrl="~/CreateModifyPage.aspx?pageId=0" />
        </div>

        <%-- List of pages displayed in a grid table --%>
        <div id="grid-container">
            <asp:Label ID="lbl_queryResult" runat="server" />
            [<asp:LinkButton ID="lb_viewDeletedPages" runat="server" OnCommand="lb_viewDeletedPages_Command" Text="View Deleted Pages"></asp:LinkButton>]<br />
            <asp:GridView ID="gvListOfPages" runat="server" CellPadding="5" ForeColor="#000333" AutoGenerateColumns="False" OnRowCommand="gvListOfPages_RowCommand">
                <Columns>
                    <asp:BoundField DataField="page_id" HeaderText="Page Id" ItemStyle-CssClass="hiddencol" HeaderStyle-CssClass="hiddencol" >
                        <HeaderStyle CssClass="hiddencol"></HeaderStyle>
                        <ItemStyle CssClass="hiddencol"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="nav_text" HeaderText="Menu Title" />
                    <asp:BoundField DataField="page_title" HeaderText="Page Title" />
                    <asp:BoundField DataField="author" HeaderText="Author" />
                    <asp:BoundField DataField="date_published" HeaderText="Date Published" />
                    <asp:ButtonField CommandName="cmdEditPage" Text="Edit" >
                        <ItemStyle ForeColor="#3333CC" HorizontalAlign="Center" VerticalAlign="Middle" Width="50px" />
                    </asp:ButtonField>
                    <asp:TemplateField ShowHeader="False">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbDeletePage" runat="server" CausesValidation="false" OnCommand="lbDeletePage_Command" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>" CommandName="cmdDeletePage" Text="Delete"
                                 OnClientClick="return confirm('Are you sure you want to delete this page and any subpages?'); ">
                            </asp:LinkButton>
                        </ItemTemplate>
                        <ItemStyle ForeColor="#FF9900" HorizontalAlign="Center" VerticalAlign="Middle" Width="100px" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="primary_nav_y_n" HeaderText="Primary Nav" ItemStyle-CssClass="hiddencol" HeaderStyle-CssClass="hiddencol" >
                        <HeaderStyle CssClass="hiddencol"></HeaderStyle>
                        <ItemStyle CssClass="hiddencol"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="parent_page_id" HeaderText="Parent Page" ItemStyle-CssClass="hiddencol" HeaderStyle-CssClass="hiddencol" >
                        <HeaderStyle CssClass="hiddencol"></HeaderStyle>
                        <ItemStyle CssClass="hiddencol"></ItemStyle>
                    </asp:BoundField>
                </Columns>
                <HeaderStyle BackColor="darkred" ForeColor="white" />
            </asp:GridView><br />
            <asp:Label ID="lbl_gridCmdReport" runat ="server" Text="" Visible="false" />
        </div>

        <%-- List of pages marked for delete --%>
        <div id="divTrashContainer" runat="server" class="hiddencol">
            <h1>List of Pages in Trash</h1>

            <asp:Label ID="lblTrashQuery" runat="server" />
            <asp:GridView ID="gvTrash" runat="server" CellPadding="5" ForeColor="#000333" AutoGenerateColumns="False" OnRowCommand="gvListOfPages_RowCommand">
                <Columns>
                    <asp:BoundField DataField="page_id" HeaderText="Page Id" ItemStyle-CssClass="hiddencol" HeaderStyle-CssClass="hiddencol" >
                        <HeaderStyle CssClass="hiddencol"></HeaderStyle>
                        <ItemStyle CssClass="hiddencol"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="nav_text" HeaderText="Menu Title" />
                    <asp:BoundField DataField="page_title" HeaderText="Page Title" />
                    <asp:BoundField DataField="last_modified_by" HeaderText="Deleted By" />
                    <asp:BoundField DataField="last_modified_on" HeaderText="Date Deleted" />
                    <asp:BoundField DataField="primary_nav_y_n" HeaderText="Primary Nav" ItemStyle-CssClass="hiddencol" HeaderStyle-CssClass="hiddencol" >
                        <HeaderStyle CssClass="hiddencol"></HeaderStyle>
                        <ItemStyle CssClass="hiddencol"></ItemStyle>
                    </asp:BoundField>
                    <asp:TemplateField ShowHeader="False">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbRestorePage" runat="server" CausesValidation="false" OnCommand="Trash_Command" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>" CommandName="cmdRestore" Text="Restore"
                                 OnClientClick="return confirm('Restoring a secondary page might restore its primary page (if also marked for delete)?'); ">
                            </asp:LinkButton>
                        </ItemTemplate>
                        <ItemStyle ForeColor="#FF9900" HorizontalAlign="Center" VerticalAlign="Middle" Width="100px" />
                    </asp:TemplateField>
                    <asp:TemplateField ShowHeader="False">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbDeletePage" runat="server" CausesValidation="false" OnCommand="Trash_Command" CommandArgument="<%# ((GridViewRow) Container).RowIndex %>" CommandName="cmdDeletePermanent" Text="Delete Permanently"
                                 OnClientClick="return confirm('Are you sure you want to PERMANENTLY delete this page and any subpages if any?'); ">
                            </asp:LinkButton>
                        </ItemTemplate>
                        <ItemStyle ForeColor="#FF9900" HorizontalAlign="Center" VerticalAlign="Middle" Width="200px" />
                    </asp:TemplateField>
                </Columns>
                <HeaderStyle BackColor="darkred" ForeColor="white" />
            </asp:GridView><br />
            <asp:Label ID="lbl_trashCmdReport" runat ="server" Text="" Visible="false" />
        </div>

    </div>
    </form>
</body>
</html>
