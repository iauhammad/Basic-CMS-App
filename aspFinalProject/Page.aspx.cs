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
    /// Class to handle server side codes for displaying a page
    /// </summary>
    public partial class Page : System.Web.UI.Page
    {

        #region Private Declarations

        int iPageId = 0;
        private string sConnectString = ConfigurationManager.ConnectionStrings["cmsProject"].ConnectionString;
        private SqlConnection objConnection;

        #endregion

        #region Protected Method

        /// <summary>
        /// Method called each time the page loads.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        /// <author>Created by iauhammad on 09/12/2016</author>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Variable declaration
            string sPageRequested;

            //--- Check the query string to retrieve page id
            sPageRequested = Request.QueryString["pageId"];
            iPageId = Convert.ToInt32(sPageRequested);

            //--- Display requested page
            if (iPageId > 0)
            {
                // Display requested page
                pDisplayRequestedPage();
            }
            //--- Page not found
            else
            {
                // Page title
                Page.Title = "404 | Not found";
                html_title.InnerHtml = "The Page Cannot Be Found";
            }
        }

        #endregion

        #region Private Method

        /// <summary>
        /// Method called to fetch and display a page.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        /// <author>Created by iauhammad on 09/12/2016</author>
        private void pDisplayRequestedPage()
        {
            // Variables declaration
            System.Text.StringBuilder sbQueryFetchPage;
            SqlCommand cmdDisplayPage;
            SqlDataReader objReader;

            // Construct query to select page's details
            sbQueryFetchPage = new System.Text.StringBuilder();
            sbQueryFetchPage.AppendLine("    Select page_title");
            sbQueryFetchPage.AppendLine("         , page_body");
            sbQueryFetchPage.AppendLine("         , author");
            sbQueryFetchPage.AppendLine("      From pages");
            sbQueryFetchPage.AppendFormat("   Where page_id = {0};", iPageId);

            // Create a new connection
            objConnection = new SqlConnection(sConnectString);

            try
            {
                // Create a new sql command
                cmdDisplayPage = new SqlCommand(sbQueryFetchPage.ToString(), objConnection);
                objConnection.Open();   // Open the connection

                // Store query results in a sqlReader
                objReader = cmdDisplayPage.ExecuteReader();

                // Use variable objReader to display/validate
                if (objReader.HasRows)
                {
                    // Read and display page details in respective controls
                    objReader.Read();
                    Page.Title = objReader.GetString(0);
                    html_title.InnerHtml = objReader.GetString(0);
                    html_main.InnerHtml = objReader.GetString(1);
                    html_footer.InnerHtml = string.Concat("Copyright \u00a9 | ", objReader.GetString(2));
                }
                else
                {
                    Page.Title = "404 Error";
                    html_title.InnerHtml = "404 Error";
                    html_main.InnerHtml = "The requested page cannot be displayed at the moment";
                    html_footer.InnerHtml = "Copyright \u00a9 | Microsoft Visual Studio 2015";
                }
                objReader.Close();

            }
            catch
            {
                Page.Title = "404 Error";
                html_title.InnerHtml = "404 Error";
                html_main.InnerHtml = "The requested page cannot be displayed at the moment";
                html_footer.InnerHtml = "Copyright \u00a9 | Microsoft Visual Studio 2015";
            }
            finally
            {
                // Close connection to the database
                objConnection.Close();
            }
        }

        #endregion

    }
}