﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Administration
{
    public partial class PageProperties : Rock.Cms.CmsBlock
    {
        private Rock.Cms.Cached.Page _page = null;
        private string _zoneName = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            try
            {
                int pageId = Convert.ToInt32( PageParameter( "Page" ) );
                _page = Rock.Cms.Cached.Page.Read( pageId );

                if ( _page.Authorized( "Configure", CurrentUser ) )
                {
                    foreach ( Rock.Cms.Cached.Attribute attribute in _page.Attributes )
                    {
                        HtmlGenericControl li = new HtmlGenericControl( "li" );
                        li.ID = string.Format( "attribute-{0}", attribute.Id );
                        li.ClientIDMode = ClientIDMode.AutoID;
                        olProperties.Controls.Add( li );

                        Label lbl = new Label();
                        lbl.ClientIDMode = ClientIDMode.AutoID;
                        lbl.Text = attribute.Name;
                        lbl.AssociatedControlID = string.Format( "attribute-field-{0}", attribute.Id );
                        li.Controls.Add( lbl );

                        Control attributeControl = attribute.CreateControl( _page.AttributeValues[attribute.Key].Value, !Page.IsPostBack );
                        attributeControl.ID = string.Format( "attribute-field-{0}", attribute.Id );
                        attributeControl.ClientIDMode = ClientIDMode.AutoID;
                        li.Controls.Add( attributeControl );

                        if ( !string.IsNullOrEmpty( attribute.Description ) )
                        {
                            HtmlAnchor a = new HtmlAnchor();
                            a.ClientIDMode = ClientIDMode.AutoID;
                            a.Attributes.Add( "class", "attribute-description tooltip" );
                            a.InnerHtml = "<span>" + attribute.Description + "</span>";

                            li.Controls.Add( a );
                        }
                    }
                }
                else
                {
                    DisplayError( "You are not authorized to edit this page" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            if (!Page.IsPostBack && _page.Authorized( "Configure", CurrentUser ) )
            {
                LoadDropdowns();

                tbPageName.Text = _page.Name;
                tbPageTitle.Text = _page.Title;
                ddlLayout.Text = _page.Layout;
                ddlMenuWhen.SelectedValue = ( ( Int32 )_page.DisplayInNavWhen ).ToString();
                cbMenuDescription.Checked = _page.MenuDisplayDescription;
                cbMenuIcon.Checked = _page.MenuDisplayIcon;
                cbMenuChildPages.Checked = _page.MenuDisplayChildPages;
                cbRequiresEncryption.Checked = _page.RequiresEncryption;
                cbEnableViewState.Checked = _page.EnableViewstate;
                cbIncludeAdminFooter.Checked = _page.IncludeAdminFooter;
                tbCacheDuration.Text = _page.OutputCacheDuration.ToString();
                tbDescription.Text = _page.Description;
            }

            base.OnLoad( e );
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                Rock.Services.Cms.PageService pageService = new Rock.Services.Cms.PageService();
                Rock.Models.Cms.Page page = pageService.Get( _page.Id );

                page.Name = tbPageName.Text;
                page.Title = tbPageTitle.Text;
                page.Layout = ddlLayout.Text;
                page.DisplayInNavWhen = ( Rock.Models.Cms.DisplayInNavWhen )Enum.Parse( typeof( Rock.Models.Cms.DisplayInNavWhen ), ddlMenuWhen.SelectedValue );
                page.MenuDisplayDescription = cbMenuDescription.Checked;
                page.MenuDisplayIcon = cbMenuIcon.Checked;
                page.MenuDisplayChildPages = cbMenuChildPages.Checked;
                page.RequiresEncryption = cbRequiresEncryption.Checked;
                page.EnableViewState = cbRequiresEncryption.Checked;
                page.IncludeAdminFooter = cbIncludeAdminFooter.Checked;
                page.OutputCacheDuration = Int32.Parse( tbCacheDuration.Text );
                page.Description = tbDescription.Text;
                
                pageService.Save( page, CurrentPersonId );

                foreach ( Rock.Cms.Cached.Attribute attribute in _page.Attributes )
                {
                    Control control = olProperties.FindControl( string.Format( "attribute-field-{0}", attribute.Id.ToString() ) );
                    if ( control != null )
                        _page.AttributeValues[attribute.Key] = new KeyValuePair<string, string>( attribute.Name, attribute.FieldType.Field.ReadValue( control ) );
                }

                _page.SaveAttributeValues( CurrentPersonId );

                Rock.Cms.Cached.Page.Flush( _page.Id );
            }

            phClose.Controls.AddAt(0, new LiteralControl( @"
    <script type='text/javascript'>
        window.parent.$('#modalDiv').dialog('close');
    </script>
" ));
        }

        private void LoadDropdowns()
        {
            ddlLayout.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( Path.Combine( this.Page.Request.MapPath( this.ThemePath ), "Layouts" ) );
            foreach ( FileInfo fi in di.GetFiles( "*.aspx.cs" ) )
                ddlLayout.Items.Add( new ListItem( fi.Name.Remove( fi.Name.IndexOf( ".aspx.cs" ) ) ) );

            ddlMenuWhen.BindToEnum( typeof( Rock.Models.Cms.DisplayInNavWhen ) );
        }
    }
}