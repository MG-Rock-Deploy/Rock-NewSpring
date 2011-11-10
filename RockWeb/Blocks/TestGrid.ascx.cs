﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks
{
    public partial class TestGrid : Rock.Cms.CmsBlock
    {
        Rock.Services.Cms.PageService pageService = new Rock.Services.Cms.PageService();

        protected override void OnInit( EventArgs e )
        {
            rGrid.DataKeyNames = new string[] { "id" };
            rGrid.EnableAdd = true;
            //rGrid.ClientAddScript = "return addItem();";
            rGrid.GridAdd += new Rock.Controls.GridAddEventHandler( rGrid_GridAdd );
            rGrid.RowDeleting += new GridViewDeleteEventHandler( rGrid_RowDeleting );
            rGrid.GridReorder += new Rock.Controls.GridReorderEventHandler( rGrid_GridReorder );
            rGrid.GridRebind += new Rock.Controls.GridRebindEventHandler( rGrid_GridRebind );

            string script = string.Format( @"
    Sys.Application.add_load(function () {{
        $('{0} td.grid-icon-cell.delete a').click(function(){{
            return confirm('Are you sure you want to delete this Page?');
            }});
    }});
", rGrid.ClientID );

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            BindGrid();
            base.OnLoad( e );
        }

        private void BindGrid()
        {
            rGrid.DataSource = pageService.GetPagesByParentPageId( null ).ToList();
            rGrid.DataBind();
        }

        void rGrid_GridAdd( object sender, EventArgs e )
        {
            Rock.Models.Cms.Page page = new Rock.Models.Cms.Page();
            page.Name = "New Page";

            Rock.Models.Cms.Page lastPage = pageService.Queryable().
                Where( p => !p.ParentPageId.HasValue).
                OrderByDescending( b => b.Order ).FirstOrDefault();

            if ( lastPage != null )
                page.Order = lastPage.Order + 1;
            else
                page.Order = 0;

            pageService.AddPage( page );
            pageService.Save( page, CurrentPersonId );

            BindGrid();
        }

        protected void rGrid_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {
            Rock.Models.Cms.Page page = pageService.GetPage((int)e.Keys["id"]);
            if ( page != null )
            {
                pageService.DeletePage( page );
                pageService.Save( page, CurrentPersonId );
            }

            BindGrid();
        }

        void rGrid_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            pageService.Reorder( (List<Rock.Models.Cms.Page>)rGrid.DataSource, 
                e.OldIndex, e.NewIndex, CurrentPersonId );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }
    }
}