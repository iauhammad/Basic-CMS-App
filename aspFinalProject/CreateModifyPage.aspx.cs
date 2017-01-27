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
    /// Class to handle server side codes for adding a new page or modifying an existing page
    /// </summary>
    public partial class CreateModifyPage : System.Web.UI.Page
    {

        #region Private Declarations

        int iPageId = 0;    // Either 0: Creation or > 0: Modification
        int iInitialParentPageId = 0;
        int iInitialNavigationId = 0;
        bool fPrimaryPage = true;
        string sConnectString = ConfigurationManager.ConnectionStrings["cmsProject"].ConnectionString;  // Connection string
        SqlConnection objConnection;    // SQL connection to the DB
        SqlTransaction objTransaction;  // SQL transaction

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
            //--- Initialisations when page loads
            txt_author.Text = Environment.UserName;

            //--- Check the query string to know whether to start in Creation or Modification mode
            iPageId = Convert.ToInt32(Request.QueryString["pageId"]);

            //--- Load the parent pages into the dropdown list
            if (!IsPostBack)
            {
                pLoadParentPages();
            }

            //--- Modify/update an existing page
            if (iPageId > 0)
            {
                // Get more parameters if in edit mode
                fPrimaryPage = (Request.QueryString["fPrimary"] == "Y");    // Check if page to modify is a parent or child

                if (!fPrimaryPage)  // If child page, capture parent id
                {
                    iInitialParentPageId = Convert.ToInt32(Request.QueryString["parentId"]);
                }

                // Page title
                Page.Title = "Update Page | Manage Page";
                page_content_title.Text = "Modify Current Page";

                // Disable 'Add Page' button
                btn_add.Enabled = false;

                // Retrieve page data from DB and populate form
                pDisplayPageToUpdate();
            }
            //--- Create a new page
            else
            {
                // Page title
                Page.Title = "Add Page | Manage Page";
                page_content_title.Text = "Add New Page";

                // Disable 'Update' button
                btn_modify.Enabled = false;
            }

        }

        /// <summary>
        /// Create a new page and store it in the pages table.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        /// <author>Created by iauhammad on 04/12/2016</author>
        protected void btn_add_Click(object sender, EventArgs e)
        {
            //--- Variables declaration
            int iPageId = 0, iNavAdded = 0, iParentIdAttachedTo = 0;
            string sPageTitle, sPageBody, sAuthor, sNavTerm;
            DateTime dDateCreated;
            SqlCommand objCmdAddPage, objCmdAddLink;
            System.Text.StringBuilder sbQueryNewPage, sbQueryNewLink;

            //--- Get user input from form
            sPageTitle = txt_page_title.Text.Trim();
            sPageBody = txt_page_content.Text.Trim();
            sAuthor = txt_author.Text.Trim();
            dDateCreated = DateTime.Now;
            sNavTerm = txt_nav_term.Text.Trim();
            iParentIdAttachedTo = Convert.ToInt32(ddlPageParent.SelectedValue);

            //--- Build query to insert new page
            sbQueryNewPage = new System.Text.StringBuilder();
            sbQueryNewPage.AppendLine("INSERT INTO pages (page_title, page_body, author, primary_nav_y_n, parent_page_id, date_created) OUTPUT INSERTED.page_id ");
            sbQueryNewPage.AppendLine("SELECT @p_title,  @p_body, @p_author");
            sbQueryNewPage.AppendFormat(", '{0}'", (iParentIdAttachedTo == 0) ? "Y" : "N");
            if (iParentIdAttachedTo > 0)
            {
                sbQueryNewPage.AppendFormat(", {0}", iParentIdAttachedTo);
            }
            else
            {
                sbQueryNewPage.AppendLine(", NULL");
            }
            sbQueryNewPage.AppendFormat(", '{0}'", dDateCreated.ToString());

            //--- Build query to insert navigation
            sbQueryNewLink = new System.Text.StringBuilder();
            if (iParentIdAttachedTo == 0) // Build query to insert primary nav
            {
                sbQueryNewLink.AppendLine("INSERT INTO primary_nav (page_id, nav_text, nav_link_value) ");
                sbQueryNewLink.AppendLine("VALUES (@n_pageId, @n_navText, @n_navValue)");
            }
            else  // Build query to insert secondary nav
            {
                sbQueryNewLink.AppendLine("INSERT INTO secondary_nav (primary_nav_id, page_id, nav_text, nav_link_value) ");
                sbQueryNewLink.AppendLine("     SELECT nav_id as primary_nav_id, @n_pageId, @n_navText, @n_navValue");
                sbQueryNewLink.AppendLine("       FROM primary_nav");
                sbQueryNewLink.AppendFormat("    WHERE page_id = {0}", iParentIdAttachedTo);
            }

            //--- Open connection to DB FIRST, then start transaction
            objConnection = new SqlConnection(sConnectString);
            objConnection.Open();

            //--- Initialise transaction object
            objTransaction = objConnection.BeginTransaction("AddNewPage");

            //--- Pass parameters to prevent SQL injection
            objCmdAddPage = new SqlCommand(sbQueryNewPage.ToString(), objConnection, objTransaction);
            objCmdAddPage.Parameters.AddWithValue("@p_title", sPageTitle);
            objCmdAddPage.Parameters.AddWithValue("@p_body", sPageBody);
            objCmdAddPage.Parameters.AddWithValue("@p_author", sAuthor);

            try
            {
                //--- Execute query
                iPageId = (Int32)objCmdAddPage.ExecuteScalar(); // Execute an insert statement and return the identity generated by this query
                if (iPageId > 0)
                {
                    //--- Insert Link ONLY if page has been inserted successfully
                    objCmdAddLink = new SqlCommand(sbQueryNewLink.ToString(), objConnection, objTransaction);
                    objCmdAddLink.Parameters.AddWithValue("@n_pageId", iPageId);
                    objCmdAddLink.Parameters.AddWithValue("@n_navText", sNavTerm.Trim());
                    objCmdAddLink.Parameters.AddWithValue("@n_navValue", sNavTerm.Trim().ToLower().Replace(" ", "_")); // Replace space with underscore (e.g. Contact Us -> contact_us)

                    //--- Execute query
                    iNavAdded = objCmdAddLink.ExecuteNonQuery();
                    if (iNavAdded > 0)
                    {
                        //--- Commit transaction
                        objTransaction.Commit();

                        // Reset page controls
                        pResetFields();
                        if (iParentIdAttachedTo == 0)   // Refresh dropdown with just added primary page
                        {
                            pLoadParentPages();
                        }

                        // Display message page added
                        lbl_operation_status.Text = "Your new page was added successfully";
                        lbl_operation_status.ForeColor = System.Drawing.Color.ForestGreen;
                        linkHome.Visible = true;
                    }
                    else
                    {
                        //--- Rollback transaction
                        objTransaction.Rollback();

                        //--- Display message page not added
                        lbl_operation_status.Text = "The page was not added.";
                        lbl_operation_status.ForeColor = System.Drawing.Color.OrangeRed;
                    }

                }
                else
                {
                    //--- Rollback transaction
                    objTransaction.Rollback();

                    //--- Display message page not added
                    lbl_operation_status.Text = "The page was not added.";
                    lbl_operation_status.ForeColor = System.Drawing.Color.OrangeRed;
                }

            }
            catch (Exception ex)
            {
                //--- Rollback transaction
                objTransaction.Rollback();

                //--- Error occured when adding page
                lbl_operation_status.Text = string.Format("Error while adding new page.{0}{1}", Environment.NewLine, ex.Message);
                lbl_operation_status.ForeColor = System.Drawing.Color.OrangeRed;
            }
            finally
            {
                if (objTransaction != null)
                {
                    objTransaction.Dispose();
                    objTransaction = null;
                }
                objConnection.Close();  // Close connection

                // Hide operation status label and homepage link after 7 seconds
                ClientScript.RegisterStartupScript(this.GetType(), "cmdAddPage", "hideControls();", true);
            }
        }

        /// <summary>
        /// Update page and/or navigation details.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        /// <author>Created by iauhammad on 04/12/2016</author>
        /// <author>Modified by iauhammad on 10/12/2016: Managed dynamic creation and modfication of secondary pages</author>
        protected void btn_modify_Click(object sender, EventArgs e)
        {
            //--- Variables declaration
            int iNewParentId = 0, iPrimaryNavUpdated = 0, iSecondaryNavUpdated = 0;
            System.Text.StringBuilder sbQueryUpdatePage;
            System.Text.StringBuilder sbQueryPrimaryNav, sbQuerySecondaryNav;
            SqlCommand cmdUpdateAPage;
            SqlCommand cmdUpdatePrimaryNav, cmdUpdateSecondaryNav;

            //--- Initialisations
            objTransaction = null;
            iNewParentId = Convert.ToInt32(ddlPageParent.SelectedValue);
            sbQueryPrimaryNav = new System.Text.StringBuilder();
            sbQuerySecondaryNav = new System.Text.StringBuilder();


            //--- Programming Logic to update Navigation: FOUR types of changes a page can undergo:
            //--- ---------------------------------------------------------------------------------
            //--- 1. Change from PRIMARY page to SECONDARY page
            if (fPrimaryPage && iNewParentId > 0)
            {
                // Delete from Primary nav
                sbQueryPrimaryNav = new System.Text.StringBuilder();
                sbQueryPrimaryNav.AppendFormat("DELETE FROM primary_nav WHERE nav_id = {0};", iInitialNavigationId);

                // Insert new Navigation in secondary nav
                sbQuerySecondaryNav = new System.Text.StringBuilder();
                sbQuerySecondaryNav.AppendLine("INSERT INTO secondary_nav (primary_nav_id, page_id, nav_text, nav_link_value) ");
                sbQuerySecondaryNav.AppendLine("     SELECT nav_id as primary_nav_id, @n_pageId, @n_navText, @n_navValue");
                sbQuerySecondaryNav.AppendLine("       FROM primary_nav");
                sbQuerySecondaryNav.AppendFormat("    WHERE page_id = {0};", iNewParentId);
            }

            //--- 2. Change from SECONDARY page to PRIMARY page
            if (!fPrimaryPage && iNewParentId == 0)
            {
                // Delete from secondary nav
                sbQuerySecondaryNav = new System.Text.StringBuilder();
                sbQuerySecondaryNav.AppendFormat("DELETE FROM secondary_nav WHERE nav_id = {0};", iInitialNavigationId);

                // Insert new Navigation in primary nav
                sbQueryPrimaryNav = new System.Text.StringBuilder();
                sbQueryPrimaryNav.AppendLine("INSERT INTO primary_nav (page_id, nav_text, nav_link_value) ");
                sbQueryPrimaryNav.AppendLine("     VALUES (@n_pageId, @n_navText, @n_navValue);");
            }

            //--- 3. Is a secondary page and remains a secondary page, BUT:
            if (!fPrimaryPage && (iNewParentId > 0))
            {
                // Update secondary nav
                sbQuerySecondaryNav = new System.Text.StringBuilder();
                sbQuerySecondaryNav.AppendLine(" UPDATE secondary_nav");
                sbQuerySecondaryNav.AppendLine("    SET nav_text        = @n_navText");
                sbQuerySecondaryNav.AppendLine("       ,nav_link_value  = @n_navValue");
                if (iInitialParentPageId != iNewParentId)   // Either, parent page could change
                {
                    sbQuerySecondaryNav.AppendLine("       ,primary_nav_id  = pn.nav_id");
                    sbQuerySecondaryNav.AppendLine("   FROM primary_nav pn");
                    sbQuerySecondaryNav.AppendFormat("WHERE pn.page_id = {0}", iNewParentId);
                }
                else   // Or, just navigation text could change
                {
                    sbQuerySecondaryNav.AppendLine("WHERE nav_link_value <> @n_navValue");
                }
                sbQuerySecondaryNav.AppendFormat("AND secondary_nav.nav_id = {0};", iInitialNavigationId);
            }

            //--- 4. A primary didnt change parent but only nav text
            if (fPrimaryPage && iNewParentId == 0)
            {
                //Update nav text
                sbQueryPrimaryNav = new System.Text.StringBuilder();
                sbQueryPrimaryNav.AppendLine(" UPDATE primary_nav");
                sbQueryPrimaryNav.AppendLine("    SET nav_text          = @n_navText");
                sbQueryPrimaryNav.AppendLine("       ,nav_link_value    = @n_navValue");
                sbQueryPrimaryNav.AppendFormat("WHERE nav_id            = {0}", iInitialNavigationId);
                sbQueryPrimaryNav.AppendLine("     AND nav_link_value   <> @n_navValue;");
            }


            //--- Programming logic to update page content and/or nav
            //--- ---------------------------------------------------
            // Construct query to update page content
            sbQueryUpdatePage = new System.Text.StringBuilder();
            sbQueryUpdatePage.AppendLine("  UPDATE pages");
            sbQueryUpdatePage.AppendLine("     SET page_title = @pageTitle");
            sbQueryUpdatePage.AppendLine("       , page_body = @pageBody");
            if (fPrimaryPage && iNewParentId > 0)   // Primary to Secondary
            {
                sbQueryUpdatePage.AppendLine("   , primary_nav_y_n = 'N'");
                sbQueryUpdatePage.AppendFormat(" , parent_page_id = {0}", iNewParentId);
            }
            if (!fPrimaryPage && iNewParentId == 0)  // Secondary to Primary
            {
                sbQueryUpdatePage.AppendLine("   , primary_nav_y_n = 'Y'");
                sbQueryUpdatePage.AppendLine("   , parent_page_id = NULL");
            }
            if (!fPrimaryPage && (iNewParentId > 0) && (iInitialParentPageId != iNewParentId)) // Secondary stays Secondary but changes Primary
            {
                sbQueryUpdatePage.AppendFormat(" , parent_page_id = {0}", iNewParentId);
            }
            sbQueryUpdatePage.AppendLine("       , page_status = 'M'");
            sbQueryUpdatePage.AppendFormat("     , last_modified_by = '{0}'", Environment.UserName);
            sbQueryUpdatePage.AppendLine("       , last_modified_on = GETDATE()");
            sbQueryUpdatePage.AppendFormat(" WHERE page_id = {0}", iPageId);
            sbQueryUpdatePage.AppendFormat("     AND (page_title <> @pageTitle or page_body <> @pageBody or ISNULL(parent_page_id, 0) <> {0});", iNewParentId);


            // Create a new connection
            objConnection = new SqlConnection(sConnectString);
            objConnection.Open();   // Open the connection
            objTransaction = objConnection.BeginTransaction("UpdatePageDetails");   // BEGIN TRANSACTION

            try
            {
                //--- Update page content
                //--- -------------------
                // Create a new sql command
                cmdUpdateAPage = new SqlCommand(sbQueryUpdatePage.ToString(), objConnection, objTransaction);

                // Pass parameters to command
                cmdUpdateAPage.Parameters.AddWithValue("@pageTitle", txt_page_title.Text.Trim());
                cmdUpdateAPage.Parameters.AddWithValue("@pageBody", txt_page_content.Text.Trim());

                int iPageUpdated = cmdUpdateAPage.ExecuteNonQuery(); // Execute query on DB


                //--- Update nav details
                //--- ------------------
                string sNavText = txt_nav_term.Text.Trim();
                string sNavValue = txt_nav_term.Text.Trim().ToLower().Replace(" ", "_");

                // 1. Primary -> Secondary
                if (fPrimaryPage && (iNewParentId > 0))
                {
                    // Create a new sql command to delete primary nav (No parameters required)
                    cmdUpdatePrimaryNav = new SqlCommand(sbQueryPrimaryNav.ToString(), objConnection, objTransaction);

                    // Create a new sql command to create secondary nav
                    cmdUpdateSecondaryNav = new SqlCommand(sbQuerySecondaryNav.ToString(), objConnection, objTransaction);
                    // Pass parameters to command
                    cmdUpdateSecondaryNav.Parameters.AddWithValue("@n_pageId", iPageId);
                    cmdUpdateSecondaryNav.Parameters.AddWithValue("@n_navText", sNavText);
                    cmdUpdateSecondaryNav.Parameters.AddWithValue("@n_navValue", sNavValue);

                    // Execute Queries
                    iPrimaryNavUpdated = cmdUpdatePrimaryNav.ExecuteNonQuery();
                    iSecondaryNavUpdated = cmdUpdateSecondaryNav.ExecuteNonQuery();
                }

                // 2. Secondary -> Primary
                if (!fPrimaryPage && (iNewParentId == 0))
                {
                    // Create a new sql command to delete secondary nav (No parameters required)
                    cmdUpdateSecondaryNav = new SqlCommand(sbQuerySecondaryNav.ToString(), objConnection, objTransaction);

                    // Create a new sql command to create primary nav
                    cmdUpdatePrimaryNav = new SqlCommand(sbQueryPrimaryNav.ToString(), objConnection, objTransaction);
                    // Pass parameters to command
                    cmdUpdatePrimaryNav.Parameters.AddWithValue("@n_pageId", iPageId);
                    cmdUpdatePrimaryNav.Parameters.AddWithValue("@n_navText", sNavText);
                    cmdUpdatePrimaryNav.Parameters.AddWithValue("@n_navValue", sNavValue);

                    // Execute Queries
                    iSecondaryNavUpdated = cmdUpdateSecondaryNav.ExecuteNonQuery();
                    iPrimaryNavUpdated = cmdUpdatePrimaryNav.ExecuteNonQuery();
                }

                // 3. Secondary page stays a Secondary page
                if (!fPrimaryPage && (iNewParentId > 0))
                {
                    // Create a new sql command
                    cmdUpdateSecondaryNav = new SqlCommand(sbQuerySecondaryNav.ToString(), objConnection, objTransaction);
                    // Pass parameters to command
                    cmdUpdateSecondaryNav.Parameters.AddWithValue("@n_navText", sNavText);
                    cmdUpdateSecondaryNav.Parameters.AddWithValue("@n_navValue", sNavValue);

                    // Execute Query
                    iSecondaryNavUpdated = cmdUpdateSecondaryNav.ExecuteNonQuery();
                }

                // 4. Primary page stays a Primary page
                if (fPrimaryPage && iNewParentId == 0)
                {
                    // Create a new sql command
                    cmdUpdatePrimaryNav = new SqlCommand(sbQueryPrimaryNav.ToString(), objConnection, objTransaction);
                    // Pass parameters to command
                    cmdUpdatePrimaryNav.Parameters.AddWithValue("@n_navText", sNavText);
                    cmdUpdatePrimaryNav.Parameters.AddWithValue("@n_navValue", sNavValue);

                    // Execute Query
                    iPrimaryNavUpdated = cmdUpdatePrimaryNav.ExecuteNonQuery();
                }


                //--- Refresh grid after successful update(s)
                //--- ---------------------------------------
                if (iPageUpdated == 1 || iPrimaryNavUpdated == 1 || iSecondaryNavUpdated == 1)
                {
                    objTransaction.Commit();    // If updates were made, COMMIT TRANSACTION
                    pDisplayPageToUpdate();     // Redisplay updated page details
                    lbl_operation_status.Text = "Page updated successfully!";
                    lbl_operation_status.ForeColor = System.Drawing.Color.ForestGreen;
                }
                else
                {
                    objTransaction.Rollback();  // If errors, ROLLBACK TRANSACTION
                    lbl_operation_status.Text = "No changes have been made!";
                    lbl_operation_status.ForeColor = System.Drawing.Color.Blue;
                }

            }
            catch
            {
                //--- SQL Error occured
                objTransaction.Rollback();  // If errors, ROLLBACK TRANSACTION
                lbl_operation_status.Text = "SQLException occured while trying to update selected page.";
                lbl_operation_status.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                if (objTransaction != null)
                {
                    objTransaction.Dispose();
                    objTransaction = null;
                }
                // Close connection to the database
                objConnection.Close();
                // Hide label after 5 seconds
                ClientScript.RegisterStartupScript(this.GetType(), "cmdReport", "hideControls();", true);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method to retrieve/display details of page selected for update.
        /// </summary>
        /// <author>Created by iauhammad on 04/12/2016</author>
        private void pDisplayPageToUpdate()
        {
            // Variables declaration
            System.Text.StringBuilder sbQueryFetchPageDate;
            SqlCommand cmdDisplayPageData;
            SqlDataReader objReader;

            // Construct query to select pages details
            sbQueryFetchPageDate = new System.Text.StringBuilder();
            sbQueryFetchPageDate.AppendLine("    Select p.page_title");
            sbQueryFetchPageDate.AppendLine("         , p.page_body");
            sbQueryFetchPageDate.AppendLine("         , n.nav_id");
            sbQueryFetchPageDate.AppendLine("         , n.nav_text");
            sbQueryFetchPageDate.AppendLine("      From pages p");
            if (fPrimaryPage)   // Join on primary_nav table if primary page
            {
                sbQueryFetchPageDate.AppendLine("Inner Join primary_nav n");
                sbQueryFetchPageDate.AppendLine("        On p.page_id = n.page_id");
            }
            else   // Join on secondary_nav table if secondary page
            {
                sbQueryFetchPageDate.AppendLine("Inner Join secondary_nav n");
                sbQueryFetchPageDate.AppendLine("        On p.page_id = n.page_id");
            }
            sbQueryFetchPageDate.AppendFormat("   Where p.page_id = {0};", iPageId);

            // Create a new connection
            objConnection = new SqlConnection(sConnectString);

            try
            {
                // Create a new sql command
                cmdDisplayPageData = new SqlCommand(sbQueryFetchPageDate.ToString(), objConnection);
                objConnection.Open();   // Open the connection

                // Store query results in a sqlReader
                objReader = cmdDisplayPageData.ExecuteReader();

                // Use variable objReader to display/validate
                if (objReader.HasRows)
                {
                    // Read and display page details in respective controls
                    objReader.Read();
                    if (!IsPostBack)    // Prevent loss of updated information entered by user on postback
                    {
                        txt_page_title.Text = objReader.GetString(0);
                        txt_page_content.Text = objReader.GetString(1);
                        txt_nav_term.Text = objReader.GetString(3);
                        if (!fPrimaryPage)
                        {
                            ddlPageParent.SelectedValue = Convert.ToString(iInitialParentPageId);
                        }
                    }
                    iInitialNavigationId = objReader.GetInt32(2);
                }
                objReader.Close();

            }
            catch
            {
                lbl_operation_status.Text = "Unable to display pade details at the moment.";
                lbl_operation_status.ForeColor = System.Drawing.Color.OrangeRed;
            }
            finally
            {
                // Close connection to the database
                objConnection.Close();
            }
        }

        /// <summary>
        /// Method to populate dropdownlist with all parent pages.
        /// </summary>
        /// <author>Created by iauhammad on 09/12/2016</author>
        private void pLoadParentPages()
        {
            // Variables declaration
            SqlDataReader sdrParentPages;
            SqlCommand cmdQueryParentPages;
            System.Data.DataTable dtbParentPages;
            System.Text.StringBuilder sbSelectParentPages;

            // Initialisation
            dtbParentPages = new System.Data.DataTable();

            // Build query to select all parent pages
            sbSelectParentPages = new System.Text.StringBuilder();
            sbSelectParentPages.AppendLine("    SELECT page_id, nav_text ");
            sbSelectParentPages.AppendLine("      FROM ( ");
            sbSelectParentPages.AppendLine("                SELECT 0 AS page_id, '( no parent )' AS nav_text ");
            sbSelectParentPages.AppendLine("            UNION ");
            sbSelectParentPages.AppendLine("                SELECT p.page_id, pn.nav_text");
            sbSelectParentPages.AppendLine("                  FROM pages p");
            sbSelectParentPages.AppendLine("            INNER JOIN primary_nav pn");
            sbSelectParentPages.AppendLine("                    ON p.page_id = pn.page_id");
            sbSelectParentPages.AppendLine("                 WHERE primary_nav_y_n = 'Y'"); // Retrieve only primary pages
            sbSelectParentPages.AppendLine("                   AND page_status <> 'D'");    // Do not retrieve pages that are marked as delete
            if (iPageId > 0)    // While in modification, prevent a page from having the option of being his own parent page
            {
                sbSelectParentPages.AppendFormat("AND p.page_id <> {0}", iPageId);
            }
            sbSelectParentPages.AppendLine("           ) temp ");
            sbSelectParentPages.AppendLine("  ORDER BY page_id;");

            // Create a new db connection
            objConnection = new SqlConnection(sConnectString);

            try
            {
                // Create & Execute a new sql command
                cmdQueryParentPages = new SqlCommand(sbSelectParentPages.ToString(), objConnection);
                objConnection.Open();
                sdrParentPages = cmdQueryParentPages.ExecuteReader();
                dtbParentPages.Load(sdrParentPages);

                // Link the dropdown to the datatable
                ddlPageParent.Items.Clear();
                ddlPageParent.DataSource = dtbParentPages;
                ddlPageParent.DataValueField = "page_id";
                ddlPageParent.DataTextField = "nav_text";
                ddlPageParent.DataBind();

            }
            catch
            {
                // If exception occurs, dropdown list will not be populated
            }
            finally
            {
                if (dtbParentPages != null)
                {
                    dtbParentPages.Clear();
                    dtbParentPages.Dispose();
                    dtbParentPages = null;
                }
            }
        }

        /// <summary>
        /// Method to reset form fields after creation of a new page.
        /// </summary>
        /// <author>Created by iauhammad on 04/12/2016</author>
        private void pResetFields()
        {
            txt_page_title.Text = string.Empty;
            txt_page_content.Text = string.Empty;
            txt_nav_term.Text = string.Empty;
            ddlPageParent.ClearSelection();
        }

        #endregion

    }
}