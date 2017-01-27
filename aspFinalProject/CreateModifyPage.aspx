<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateModifyPage.aspx.cs" Inherits="aspFinalProject.CreateModifyPage" validateRequest="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Manage Page</title>
    <link rel="stylesheet" type="text/css" href="styles/managePage.css" />
    <script type="text/javascript">
        function hideControls() {
            setTimeout(function () {
                document.getElementById("<%= lbl_operation_status.ClientID %>").innerHTML = "";
                document.getElementById("<%= linkHome.ClientID %>").style.display = "none";
            }, 7000);
        };
    </script>
</head>
<body>
    <form id="frm_page" runat="server">
    <div>

        <%-- Dynamic text for page title (Add or Modify) --%>
        <div>
            <h1><asp:Literal ID="page_content_title" runat="server"></asp:Literal></h1>
        </div>

        <%-- Input: Page Title --%>
        <div>
            <asp:Label ID="lbl_page_title" CssClass="formLabel" runat="server" AssociatedControlID="txt_page_title" Text="Title" />
            <asp:RequiredFieldValidator ID="chk_page_title" CssClass="errMsg" runat="server" ControlToValidate="txt_page_title" Text="Page title cannot be empty" /> <br />
            <asp:TextBox ID="txt_page_title" CssClass="formInput" runat="server" placeholder="Enter title here"></asp:TextBox>
        </div>
        
        <%-- Input: Navigation Menu Text --%>
        <div>
            <asp:Label ID="lbl_nav_term" CssClass="formLabel" runat="server" AssociatedControlID="txt_nav_term" Text="Menu term" />
            <asp:TextBox ID="txt_nav_term" CssClass="formInput" runat="server" placeholder="Enter menu term"></asp:TextBox>
            <asp:RequiredFieldValidator ID="chk_nav_term" CssClass="errMsg" runat="server" ControlToValidate="txt_nav_term" Text="Enter text to display in navigation bar" />
        </div>
        
        <%-- Input: Page Attribute --%>
        <div>
            <asp:Label ID="lbl_page_parent" CssClass="formLabel" runat="server" AssociatedControlID="ddlPageParent" Text="Parent" />
            <asp:DropDownList ID="ddlPageParent" runat="server" AppendDataBoundItems="true"></asp:DropDownList>
        </div>

        <%-- Input: Page Content --%>
        <div>
            <asp:TextBox ID="txt_page_content" CssClass="formInput" runat="server" placeholder="Enter page content here" TextMode="MultiLine" Rows="10"></asp:TextBox>
        </div>

        <%-- Input: (Edit Disabled) Page Author --%>
        <div>
            <asp:Label ID="lbl_author" CssClass="formLabel" runat="server" AssociatedControlID="txt_author" Text="Author" />
            <asp:TextBox ID="txt_author" CssClass="formInput" runat="server" Enabled="false"></asp:TextBox>
        </div>

        <%-- Buttons: Add/Update/Back --%>
        <div id="buttons-container">
            <asp:Button ID="btn_add" CssClass="actionBtn" runat="server" Text="Add Page" OnClick="btn_add_Click" />
            <asp:Button ID="btn_modify" CssClass="actionBtn" runat="server" Text="Update" OnClick="btn_modify_Click" />
            <asp:Button ID="btn_back" CssClass="actionBtn" runat="server" Text="Back" OnClientClick="JavaScript:window.location='Homepage.aspx';return false;" />
        </div>
        
        <%-- Label & Link: Operation status label & Link to navigate back to homepage --%>
        <div>
            <asp:Label ID="lbl_operation_status" runat="server" />
            <asp:HyperLink ID="linkHome" runat="server" NavigateUrl="~/Homepage.aspx" Text="Back to Homepage" Visible="false" />
        </div>

    </div>
    </form>
</body>
</html>
