<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Page.aspx.cs" Inherits="aspFinalProject.Page" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="styles/page.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <%-- Title of page --%>
        <h1 id="html_title" runat="server"></h1>

         <%-- Main content of page --%> 
        <main id="html_main" runat="server"></main>

        <%-- Page Footer --%>
        <footer id="html_footer" runat="server"></footer>
    </div>
    </form>
</body>
</html>
