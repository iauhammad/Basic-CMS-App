using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;

namespace aspFinalProject
{

    /// <summary>
    /// Class to handle server side codes for homepage and Grids operations
    /// </summary>
    public partial class Homepage : System.Web.UI.Page
    {

        #region Private Declarations

        const string sSHOW_DELETED_PAGES = "View Deleted Pages";
        const string sHIDE_DELETED_PAGES = "Hide Deleted Pages";

        private string sConnectString = ConfigurationManager.ConnectionStrings["cmsProject"].ConnectionString;
        private SqlConnection objConnection;
        private SqlDataReader objReader;

        #endregion

        #region Protected Methods

        /// <summary>
        /// Method called each time the page loads.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        /// <author>Created by iauhammad on 04/12/2016</author>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Generate navigation menu
            pGenerateNavBar();

            // Fill grid with list of pages
            pDisplayListOfPages();

            // Fill grid with pages marked as delete
            pDisplayListOfPagesToDelete();

            // Display/Hide trash grid on post back
            if (IsPostBack)
            {
                if(lb_viewDeletedPages.Text == sSHOW_DELETED_PAGES) // If label says SHOW, means we need to hide the container
                {
                    divTrashContainer.Attributes.Add("class", "hiddencol");
                    lb_viewDeletedPages.Text = sSHOW_DELETED_PAGES;
                }
                else // If label says HIDE, we need to show the container
                {
                    divTrashContainer.Attributes.Remove("class");
                    lb_viewDeletedPages.Text = sHIDE_DELETED_PAGES;
                }
                //Hide label after 5 seconds
                ClientScript.RegisterStartupScript(this.GetType(), "cmdReport", "hideLabel();", true);
                ClientScript.RegisterStartupScript(this.GetType(), "cmdTrash", "hideTrashLabel();", true);
            }
        }

        /// <summary>
        /// Event raised when a EDIT command is executed from the gridview.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        /// <author>Created by iauhammad on 04/12/2016</author>
        protected void gvListOfPages_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            //--- Variables declaration
            int iRowSelected;
            string sPageIdSelected, sPrimary_y_n, sParent_id;

            //--- Get the current row to perform required operation
            //--- -------------------------------------------------
            // Get selected row
            iRowSelected = Convert.ToInt32(e.CommandArgument);

            // Get required data from the current row
            sPageIdSelected = gvListOfPages.Rows[iRowSelected].Cells[0].Text;
            sPrimary_y_n = gvListOfPages.Rows[iRowSelected].Cells[7].Text;
            sParent_id = gvListOfPages.Rows[iRowSelected].Cells[8].Text;

            //--- If 'Edit' command was clicked
            if (e.CommandName == "cmdEditPage")
            {
                // Redirect user to the Edit page -> passing the page_id, indicating if it's a primary page and the parent_id if any as parameters
                Response.Redirect(string.Format("~/CreateModifyPage.aspx?pageId={0}&fPrimary={1}&parentId={2}", sPageIdSelected, sPrimary_y_n, sParent_id));
            }
        }

        /// <summary>
        /// Event triggered when a DELETE command is executed from main grid.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        /// <author>Created by iauhammad on 04/12/2016</author>
        protected void lbDeletePage_Command(object sender, CommandEventArgs e)
        {
            //--- Variables declaration
            int iRowSelected;
            string sPageIdSelected, sPrimary_y_n;

            //--- Get the page id of the current row to update
            //--- --------------------------------------------
            // Get index of selected row
            iRowSelected = Convert.ToInt32(e.CommandArgument);

            // Get the Page_Id of the current row
            sPageIdSelected = gvListOfPages.Rows[iRowSelected].Cells[0].Text;
            sPrimary_y_n = gvListOfPages.Rows[iRowSelected].Cells[7].Text;

            if (e.CommandName == "cmdDeletePage")
            {
                // Mark page(s) as delete, but not actually deleting the page(s)
                pMarkPagesAsDelete(Convert.ToInt32(sPageIdSelected), (sPrimary_y_n == "Y"));
            }
        }

        /// <summary>
        /// Event triggered when PERMANENT DELETE/RESTORE command is executed from trash grid.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        /// <author>Created by iauhammad on 10/12/2016</author>
        protected void Trash_Command(object sender, CommandEventArgs e)
        {
            //--- Variables declaration
            int iRowSelected;
            string sPageIdSelected, sPrimary_y_n;

            //--- Get the page id of the current row to update
            //--- --------------------------------------------
            // Get index of selected row
            iRowSelected = Convert.ToInt32(e.CommandArgument);

            // Get the Page_Id of the current row
            sPageIdSelected = gvTrash.Rows[iRowSelected].Cells[0].Text;
            sPrimary_y_n = gvTrash.Rows[iRowSelected].Cells[5].Text;

            if (e.CommandName == "cmdRestore")
            {
                // Restore page(s)
                pRestorePage(Convert.ToInt32(sPageIdSelected), (sPrimary_y_n == "Y"));
            }

            if (e.CommandName == "cmdDeletePermanent")
            {
                // Method to delete page(s) permanently
                pDeletePages(Convert.ToInt32(sPageIdSelected), (sPrimary_y_n == "Y"));
            }

        }

        /// <summary>
        /// Event triggered when user toggle VIEW/HIDE trash grid.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        /// <author>Created by iauhammad on 10/12/2016</author>
        protected void lb_viewDeletedPages_Command(object sender, CommandEventArgs e)
        {
            // Show trash gridview
            if (lb_viewDeletedPages.Text == sSHOW_DELETED_PAGES)
            {
                divTrashContainer.Attributes.Remove("class");
                lb_viewDeletedPages.Text = sHIDE_DELETED_PAGES;
            }
            else // Hide trash gridview
            {
                divTrashContainer.Attributes.Add("class", "hiddencol");
                lb_viewDeletedPages.Text = sSHOW_DELETED_PAGES;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method to retrieve and display the list of pages not marked for delete.
        /// </summary>
        /// <author>Created by iauhammad on 04/12/2016</author>
        private void pDisplayListOfPages()
        {
            // Variables declaration
            System.Text.StringBuilder sbListAllPages;
            SqlCommand cmdDisplayListPages;

            // Hide query report label
            lbl_queryResult.Text = string.Empty;

            // Construct query to select pages details
            sbListAllPages = new System.Text.StringBuilder();
            sbListAllPages.AppendLine("         Select p.page_id");
            sbListAllPages.AppendLine("              , CASE WHEN pn.nav_text IS NULL THEN sn.nav_text ELSE pn.nav_text END as nav_text");
            sbListAllPages.AppendLine("              , p.page_title");
            sbListAllPages.AppendLine("              , p.author");
            sbListAllPages.AppendLine("              , CASE WHEN p.date_published IS NULL THEN 'Not yet published'");
            sbListAllPages.AppendLine("                     WHEN p.date_published IS NOT NULL THEN CONVERT(nvarchar(30), p.date_published, 111)");
            sbListAllPages.AppendLine("                END as date_published");
            sbListAllPages.AppendLine("              , p.primary_nav_y_n");
            sbListAllPages.AppendLine("              , p.parent_page_id");
            sbListAllPages.AppendLine("           From pages p");
            sbListAllPages.AppendLine("Left Outer Join primary_nav pn");
            sbListAllPages.AppendLine("             On p.page_id = pn.page_id");
            sbListAllPages.AppendLine("Left Outer Join secondary_nav sn");
            sbListAllPages.AppendLine("             On p.page_id = sn.page_id");
            sbListAllPages.AppendLine("          WHERE p.page_status <> 'D'");
            sbListAllPages.AppendLine("       ORDER BY p.page_id;");

            // Create a new connection
            objConnection = new SqlConnection(sConnectString);

            try
            {
                // Create a new sql command
                cmdDisplayListPages = new SqlCommand(sbListAllPages.ToString(), objConnection);
                objConnection.Open();   // Open the connection

                // Store query results in a sqlReader
                objReader = cmdDisplayListPages.ExecuteReader();

                // Use variable objReader to display/validate
                if (!objReader.HasRows)
                {
                    lbl_queryResult.Text = "No results were found.";
                    lbl_queryResult.ForeColor = System.Drawing.Color.OrangeRed;
                }

                // Pass the objReader to the datasource of the gridview
                gvListOfPages.DataSource = objReader;
                gvListOfPages.DataBind();
            }
            catch
            {
                lbl_queryResult.Text = "Unable to display data at the moment.";
                lbl_queryResult.ForeColor = System.Drawing.Color.OrangeRed;
            }
            finally
            {
                // Close connection to the database
                objConnection.Close();
            }
        }

        /// <summary>
        /// Method to retrieve and display the list of pages marked for delete.
        /// </summary>
        /// <author>Created by iauhammad on 04/12/2016</author>
        private void pDisplayListOfPagesToDelete()
        {
            // Variables declaration
            System.Text.StringBuilder sbQueryPagesToDelete;
            SqlCommand cmdDisplayListPages;

            // Hide query report label
            lblTrashQuery.Text = string.Empty;

            // Construct query to select pages details
            sbQueryPagesToDelete = new System.Text.StringBuilder();
            sbQueryPagesToDelete.AppendLine("         Select p.page_id");
            sbQueryPagesToDelete.AppendLine("              , CASE WHEN pn.nav_text IS NULL THEN sn.nav_text ELSE pn.nav_text END as nav_text");
            sbQueryPagesToDelete.AppendLine("              , p.page_title");
            sbQueryPagesToDelete.AppendLine("              , p.last_modified_by");
            sbQueryPagesToDelete.AppendLine("              , p.last_modified_on");
            sbQueryPagesToDelete.AppendLine("              , p.primary_nav_y_n");
            sbQueryPagesToDelete.AppendLine("           From pages p");
            sbQueryPagesToDelete.AppendLine("Left Outer Join primary_nav pn");
            sbQueryPagesToDelete.AppendLine("             On p.page_id      = pn.page_id");
            sbQueryPagesToDelete.AppendLine("Left Outer Join secondary_nav sn");
            sbQueryPagesToDelete.AppendLine("             On p.page_id      = sn.page_id");
            sbQueryPagesToDelete.AppendLine("          WHERE p.page_status  = 'D'");
            sbQueryPagesToDelete.AppendLine("       ORDER BY p.last_modified_on desc;");

            // Create a new connection
            objConnection = new SqlConnection(sConnectString);

            try
            {
                // Create a new sql command
                cmdDisplayListPages = new SqlCommand(sbQueryPagesToDelete.ToString(), objConnection);
                objConnection.Open();   // Open the connection

                // Store query results in a sqlReader
                objReader = cmdDisplayListPages.ExecuteReader();

                // Use variable objReader to display/validate
                if (!objReader.HasRows)
                {
                    lblTrashQuery.Text = "No pages marked for delete.";
                    lblTrashQuery.ForeColor = System.Drawing.Color.Blue;
                    // Hide container if resultset empty
                    divTrashContainer.Attributes.Add("class", "hiddencol");
                    lb_viewDeletedPages.Text = sSHOW_DELETED_PAGES;
                }

                // Pass the objReader to the datasource of the gridview
                gvTrash.DataSource = objReader;
                gvTrash.DataBind();
            }
            catch
            {
                lblTrashQuery.Text = "Unable to display pages at the moment.";
                lblTrashQuery.ForeColor = System.Drawing.Color.OrangeRed;
            }
            finally
            {
                // Close connection to the database
                objConnection.Close();
            }
        }

        /// <summary>
        /// Method to generate the navigation menu dynamically.
        /// </summary>
        /// <author>Created by iauhammad on 04/12/2016</author>
        private void pGenerateNavBar()
        {
            // Variables declaration
            string sQueryPrimaryNav, sQuerySecondaryNav;
            SqlCommand cmdPrimaryNav, cmdSecondaryNav;
            SqlDataReader sdrPrimaryNav, sdrSecondaryNav;
            System.Text.StringBuilder sbNavigation, sbSecondaryLinks;
            System.Data.DataTable dtbPrimary, dtbSecondary;

            // Initialisations
            dtbPrimary = new System.Data.DataTable();
            dtbSecondary = new System.Data.DataTable();

            // Query to retrieve primary nav
            sQueryPrimaryNav = "select pn.* from primary_nav pn inner join pages p on pn.page_id = p.page_id where p.page_status <> 'D' order by p.page_id;";
            sQuerySecondaryNav = "select sn.* from secondary_nav sn inner join pages p on sn.page_id = p.page_id where p.page_status <> 'D' order by p.page_id;";

            // Create new connection
            objConnection = new SqlConnection(sConnectString);

            try
            {
                // Create new sql commands
                cmdPrimaryNav = new SqlCommand(sQueryPrimaryNav, objConnection);
                cmdSecondaryNav = new SqlCommand(sQuerySecondaryNav, objConnection);

                // Execute SQL command
                objConnection.Open();   // Open connection to db
                sdrPrimaryNav = cmdPrimaryNav.ExecuteReader();
                dtbPrimary.Load(sdrPrimaryNav);

                // Build navigation links for each link found
                if (dtbPrimary.Rows.Count > 0)
                {
                    // Find secondary links if primary links exists
                    sdrSecondaryNav = cmdSecondaryNav.ExecuteReader();
                    dtbSecondary.Load(sdrSecondaryNav);

                    sbNavigation = new System.Text.StringBuilder();
                    foreach (System.Data.DataRow drPrimaryLink in dtbPrimary.Rows)  // Loop through each primary menu items
                    {

                        sbNavigation.AppendFormat("<li><a href=\"Page.aspx?pageId={0}\" id=\"{0}\" runat=\"server\">{1}</a>", drPrimaryLink["page_id"], drPrimaryLink["nav_text"]);
                        if (dtbSecondary.Rows.Count > 0)    // Checks if there exists any secondary links
                        {
                            sbSecondaryLinks = new System.Text.StringBuilder();
                            foreach (System.Data.DataRow drSecondaryLink in dtbSecondary.Rows) // Loop through secondary links to check if the current primary link has any secondary ones
                            {
                                if (Convert.ToInt32(drSecondaryLink["primary_nav_id"]) == Convert.ToInt32(drPrimaryLink["nav_id"])) // If secondary table has fk on primary nav_id column
                                {
                                    sbSecondaryLinks.AppendFormat("<li><a href=\"Page.aspx?pageId={0}\" id=\"{0}\" runat=\"server\">{1}</a></li>", drSecondaryLink["page_id"], drSecondaryLink["nav_text"]);
                                }
                            }
                            // If there exists at least one secondary link, add to respective primary link
                            if (!string.IsNullOrEmpty(sbSecondaryLinks.ToString()))
                            {
                                sbNavigation.AppendLine("<ul>");
                                sbNavigation.AppendLine(sbSecondaryLinks.ToString());
                                sbNavigation.AppendLine("</ul>");
                            }
                        }
                        sbNavigation.AppendLine("</li>");

                    }

                    // Update the page navigation
                    mainLinks.InnerHtml = sbNavigation.ToString();
                }

            }
            catch
            {
                // Navigation bar won't be displayed
            }
            finally
            {
                objConnection.Close();  // Close connection to db
                if (dtbPrimary != null) // Dispose memory of data table
                {
                    dtbPrimary.Clear();
                    dtbPrimary.Dispose();
                    dtbPrimary = null;
                }
                if (dtbSecondary != null) // Dispose memory of data table
                {
                    dtbSecondary.Clear();
                    dtbSecondary.Dispose();
                    dtbSecondary = null;
                }
            }
        }

        /// <summary>
        /// Method to mark a page as deleted.
        /// </summary>
        /// <param name="iPageIdToDelete">Id of the page to mark as delete.</param>
        /// <param name="fPageIsPrimary">Flag indicating if it is a primary page.</param>
        /// <author>Created by iauhammad on 10/12/2016</author>
        private void pMarkPagesAsDelete(int iPageIdToDelete, bool fPageIsPrimary)
        {
            //--- Variables declaration
            int iSubPages = 0, iMainPage = 0;
            System.Text.StringBuilder sbQueryDeletePage, sbQueryDeleteSubPages;
            SqlCommand cmdDeleteAPage, cmdDeleteSubPages;
            SqlTransaction objTransaction;

            //--- Programming logic to mark page and subpages if any as delete
            //--- ------------------------------------------------------------
            // Mark any attached secondary pages as delete
            sbQueryDeleteSubPages = new System.Text.StringBuilder();
            if (fPageIsPrimary)
            {
                sbQueryDeleteSubPages.AppendLine("  UPDATE pages");
                sbQueryDeleteSubPages.AppendLine("   SET page_status        = 'D'");
                sbQueryDeleteSubPages.AppendFormat("     , last_modified_by = '{0}'", Environment.UserName);
                sbQueryDeleteSubPages.AppendLine("       , last_modified_on = GETDATE()");
                sbQueryDeleteSubPages.AppendFormat(" WHERE parent_page_id   = {0}", iPageIdToDelete);
                sbQueryDeleteSubPages.AppendLine("   AND page_status        <> 'D';");
            }

            // Construct query to mark page as delete
            sbQueryDeletePage = new System.Text.StringBuilder();
            sbQueryDeletePage.AppendLine("  UPDATE pages");
            sbQueryDeletePage.AppendLine("   SET page_status        = 'D'");
            sbQueryDeletePage.AppendFormat("     , last_modified_by = '{0}'", Environment.UserName);
            sbQueryDeletePage.AppendLine("       , last_modified_on = GETDATE()");
            sbQueryDeletePage.AppendFormat(" WHERE page_id          = {0}", iPageIdToDelete);
            sbQueryDeletePage.AppendLine("   AND page_status        <> 'D';");

            // Create & open a new connection
            objConnection = new SqlConnection(sConnectString);
            objConnection.Open();

            // Begin transaction
            objTransaction = objConnection.BeginTransaction();

            try
            {
                if (fPageIsPrimary)
                {
                    // Create & Execute query for sub pages if any
                    cmdDeleteSubPages = new SqlCommand(sbQueryDeleteSubPages.ToString(), objConnection, objTransaction);
                    iSubPages = cmdDeleteSubPages.ExecuteNonQuery();
                }

                // Create & Execute command
                cmdDeleteAPage = new SqlCommand(sbQueryDeletePage.ToString(), objConnection, objTransaction);
                iMainPage = cmdDeleteAPage.ExecuteNonQuery();

                // Refresh grid after successful mark as delete
                if (iSubPages == 1 || iMainPage == 1)
                {
                    objTransaction.Commit();    // Commit transaction
                    pDisplayListOfPages();
                    pDisplayListOfPagesToDelete();
                    pGenerateNavBar();
                    lbl_gridCmdReport.Text = "Page(s) deleted successfully!";
                    lbl_gridCmdReport.ForeColor = System.Drawing.Color.ForestGreen;
                }
                else
                {
                    // Unable to mark page as delete
                    objTransaction.Rollback();
                    lbl_gridCmdReport.Text = "Unable to delete at the moment!";
                    lbl_gridCmdReport.ForeColor = System.Drawing.Color.OrangeRed;
                }

            }
            catch
            {
                //--- SQL Error occured
                objTransaction.Rollback();
                lbl_gridCmdReport.Text = "SQLException occured while trying to delete selected page.";
                lbl_gridCmdReport.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                // Close connection to the database
                if (objTransaction != null)
                {
                    objTransaction.Dispose();
                    objTransaction = null;
                }
                objConnection.Close();
                // Hide label after 5 seconds
                lbl_gridCmdReport.Visible = true;
                ClientScript.RegisterStartupScript(this.GetType(), "cmdReport", "hideLabel();", true);
            }
        }

        /// <summary>
        /// Method to restore page from trash.
        /// </summary>
        /// <param name="iPageIdToDelete">Id of the page to restore.</param>
        /// <param name="fPageIsPrimary">Flag indicating if it is a primary page.</param>
        /// <author>Created by iauhammad on 10/12/2016</author>
        private void pRestorePage(int iPageIdToRestore, bool fPageIsPrimary)
        {
            //--- Variables declaration
            int iParentPage = 0, iActualPage = 0;
            System.Text.StringBuilder sbRestoreParent, sbRestorePage;
            SqlCommand cmdRestoreParent, cmdRestorePage;
            SqlTransaction objTransaction;

            //--- Initialisations
            sbRestorePage = new System.Text.StringBuilder();
            sbRestoreParent = new System.Text.StringBuilder();

            //--- Programming logic to restore page and parent page if required
            //--- -------------------------------------------------------------
            if (!fPageIsPrimary)    // Restore parent if marked as delete
            {
                sbRestoreParent.AppendLine("    UPDATE p2");
                sbRestoreParent.AppendLine("       SET page_status      = 'M'");
                sbRestoreParent.AppendFormat("       , last_modified_by = '{0}'", Environment.UserName);
                sbRestoreParent.AppendLine("         , last_modified_on = GETDATE()");
                sbRestoreParent.AppendLine("      FROM pages p1");
                sbRestoreParent.AppendLine("INNER JOIN pages p2");
                sbRestoreParent.AppendLine("        ON p1.parent_page_id = p2.page_id");
                sbRestoreParent.AppendFormat("   WHERE p1.page_id = {0}", iPageIdToRestore);
                sbRestoreParent.AppendLine("       AND p2.primary_nav_y_n   = 'Y'");
                sbRestoreParent.AppendLine("       AND p2.page_status       = 'D';");
            }

            // Query to restore desired page
            sbRestorePage.AppendLine("  UPDATE pages");
            sbRestorePage.AppendLine("   SET page_status        = 'M'");
            sbRestorePage.AppendFormat("     , last_modified_by = '{0}'", Environment.UserName);
            sbRestorePage.AppendLine("       , last_modified_on = GETDATE()");
            sbRestorePage.AppendFormat(" WHERE page_id          = {0}", iPageIdToRestore);
            sbRestorePage.AppendLine("   AND page_status        <> 'M';");

            // Create & Open a new connection to the db
            objConnection = new SqlConnection(sConnectString);
            objConnection.Open();

            // Begin transaction
            objTransaction = objConnection.BeginTransaction();

            try
            {
                if (!fPageIsPrimary) // Try restore parent page if needs be
                {
                    cmdRestoreParent = new SqlCommand(sbRestoreParent.ToString(), objConnection, objTransaction);
                    iParentPage = cmdRestoreParent.ExecuteNonQuery();
                }

                // Restore page
                cmdRestorePage = new SqlCommand(sbRestorePage.ToString(), objConnection, objTransaction);
                iActualPage = cmdRestorePage.ExecuteNonQuery();

                if (iParentPage == 1 || iActualPage == 1)
                {
                    objTransaction.Commit();
                    pDisplayListOfPages();
                    pDisplayListOfPagesToDelete();
                    pGenerateNavBar();
                    lbl_trashCmdReport.Text = "Page(s) restored successfully!";
                    lbl_trashCmdReport.ForeColor = System.Drawing.Color.ForestGreen;
                }
                else
                {
                    // Unable to restore page
                    objTransaction.Rollback();
                    lbl_trashCmdReport.Text = "Unable to restore page(s) at the moment!";
                    lbl_trashCmdReport.ForeColor = System.Drawing.Color.OrangeRed;
                }
            }
            catch
            {
                //--- SQL Error occured
                objTransaction.Rollback();
                lbl_trashCmdReport.Text = "SQLException occured while trying to restore selected page.";
                lbl_trashCmdReport.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                // Close connection to the database
                if (objTransaction != null)
                {
                    objTransaction.Dispose();
                    objTransaction = null;
                }
                objConnection.Close();
                // Hide label after 5 seconds
                lbl_trashCmdReport.Visible = true;
                ClientScript.RegisterStartupScript(this.GetType(), "cmdTrash", "hideTrashLabel();", true);
            }
        }

        /// <summary>
        /// Method to delete pages permanently.
        /// </summary>
        /// <param name="iPageIdToDelete">Id of the page to delete.</param>
        /// <param name="fPageIsPrimary">Flag indicating if it is a primary page.</param>
        /// <author>Created by iauhammad on 10/12/2016</author>
        private void pDeletePages(int iPageToDelete, bool fPageIsPrimary)
        {
            //--- Variables declaration
            int iSubPages = 0, iMainPage = 0;
            System.Text.StringBuilder sbNavSubPages, sbDelSubPagesPerm, sbNavPage, sbDelPagePerm;
            SqlCommand cmdNavSubPages, cmdDelSubPages, cmdNavPage, cmdDelPage;
            SqlTransaction objTransaction;

            //--- Initialisations
            sbNavPage = new System.Text.StringBuilder();
            sbNavSubPages = new System.Text.StringBuilder();
            sbDelPagePerm = new System.Text.StringBuilder();
            sbDelSubPagesPerm = new System.Text.StringBuilder();

            //--- Programming logic to delete page and subpages
            //--- ---------------------------------------------
            //--- If page to delete is primary
            if (fPageIsPrimary)
            {
                // Delete navigations of any associated sub pages
                sbNavSubPages.AppendLine("    DELETE sn");
                sbNavSubPages.AppendLine("      FROM secondary_nav sn");
                sbNavSubPages.AppendLine("INNER JOIN pages p");
                sbNavSubPages.AppendLine("        ON p.page_id = sn.page_id");
                sbNavSubPages.AppendFormat("   WHERE p.parent_page_id = {0}", iPageToDelete);

                // Delete associated sub pages
                sbDelSubPagesPerm.AppendFormat("DELETE FROM pages WHERE parent_page_id = {0};", iPageToDelete);

                // Delete actual page's navigation
                sbNavPage.AppendFormat("DELETE FROM primary_nav WHERE page_id = {0};", iPageToDelete);
            }

            //--- If page to delete is secondary
            if (!fPageIsPrimary)
            {
                sbNavPage.AppendFormat("DELETE FROM secondary_nav WHERE page_id = {0};", iPageToDelete);
            }

            //--- Delete the actual page
            sbDelPagePerm.AppendFormat("DELETE FROM pages WHERE page_id = {0};", iPageToDelete);

            // Create & Open new connection to db
            objConnection = new SqlConnection(sConnectString);
            objConnection.Open();

            // Begin Transaction
            objTransaction = objConnection.BeginTransaction();

            try
            {
                //--- Execute queries
                if (fPageIsPrimary)
                {
                    // Create & Execute commands for sub pages
                    cmdNavSubPages = new SqlCommand(sbNavSubPages.ToString(), objConnection, objTransaction);
                    cmdDelSubPages = new SqlCommand(sbDelSubPagesPerm.ToString(), objConnection, objTransaction);

                    cmdNavSubPages.ExecuteNonQuery();
                    iSubPages = cmdDelSubPages.ExecuteNonQuery();
                }

                // Create & Execute commands for main page
                cmdNavPage = new SqlCommand(sbNavPage.ToString(), objConnection, objTransaction);
                cmdDelPage = new SqlCommand(sbDelPagePerm.ToString(), objConnection, objTransaction);

                cmdNavPage.ExecuteNonQuery();
                iMainPage = cmdDelPage.ExecuteNonQuery();

                // Refresh grid after successful mark as delete
                if (iSubPages == 1 || iMainPage == 1)
                {
                    objTransaction.Commit();    // Commit transaction
                    pDisplayListOfPages();
                    pDisplayListOfPagesToDelete();
                    pGenerateNavBar();
                    lbl_trashCmdReport.Text = "Page(s) deleted permanently!";
                    lbl_trashCmdReport.ForeColor = System.Drawing.Color.ForestGreen;
                }
                else
                {
                    // Unable to mark page as delete
                    objTransaction.Rollback();
                    lbl_trashCmdReport.Text = "Unable to delete at the moment!";
                    lbl_trashCmdReport.ForeColor = System.Drawing.Color.OrangeRed;
                }
            }
            catch
            {
                //--- SQL Error occured
                objTransaction.Rollback();
                lbl_trashCmdReport.Text = "SQLException occured while trying to delete permanently.";
                lbl_trashCmdReport.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                // Close connection to the database
                if (objTransaction != null)
                {
                    objTransaction.Dispose();
                    objTransaction = null;
                }
                objConnection.Close();
                // Hide label after 5 seconds
                lbl_trashCmdReport.Visible = true;
                ClientScript.RegisterStartupScript(this.GetType(), "cmdTrash", "hideTrashLabel();", true);
            }

        }

        #endregion

    }
}