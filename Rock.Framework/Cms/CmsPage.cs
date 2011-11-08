﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using System.Text.RegularExpressions;
using System.Web;

using Rock.Models.Cms;
using Rock.Models.Crm;
using Rock.Services.Cms;
using Rock.Helpers;
using Rock.Cms.Security;

namespace Rock.Cms
{
    /// <summary>
    /// CmsPage is the base abstract class that all page templates should inherit from
    /// </summary>
    public abstract class CmsPage : System.Web.UI.Page
    {
        #region Private Variables

        private Dictionary<string, Control> _zones;
        private PlaceHolder phLoadTime;

        #endregion

        #region Protected Variables

        protected string UserName = string.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current Rock page instance being requested.  This value is set 
        /// by the RockRouteHandler immediately after instantiating the page
        /// </summary>
        public Rock.Cms.Cached.Page PageInstance { get; set; }

        /// <summary>
        /// The content areas on a layout page that blocks can be added to 
        /// </summary>
        public Dictionary<string, Control> Zones
        {
            get
            {
                if ( _zones == null )
                {
                    _zones = new Dictionary<string, Control>();
                    DefineZones();
                }
                return _zones;
            }
        }

        /// <summary>
        /// The Person ID of the currently logged in user.  Returns null if there is not a user logged in
        /// </summary>
        public int? CurrentPersonId
        {
            get
            {
                MembershipUser user = Membership.GetUser();
                if ( user != null )
                {
                    if ( user.ProviderUserKey != null )
                        return ( int )user.ProviderUserKey;
                    else
                        return null;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the currently logged in person.  Returns null if there is not a user logged in
        /// </summary>
        public Person CurrentPerson
        {
            get
            {
                int? personId = CurrentPersonId;
                if ( personId != null )
                {
                    if ( Context.Items.Contains( "CurrentPerson" ) )
                    {
                        return ( Person )Context.Items["CurrentPerson"];
                    }
                    else
                    {
                        Rock.Services.Crm.PersonService personService = new Services.Crm.PersonService();
                        Person person = personService.GetPerson( personId.Value );
                        Context.Items.Add( "CurrentPerson", person );
                        return person;
                    }
                }
                return null;
            }

            private set
            {
                Context.Items.Add( "CurrentPerson", value );
            }
        }

        public string AppPath
        {
            get
            {
                return ResolveUrl( "~" );
            }
        }

        public string ThemePath
        {
            get
            {
                return ResolveUrl( string.Format( "~/Themes/{0}", PageInstance.Site.Theme ) );
            }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Each layout page should define it's content zones in this method
        /// <code>
        ///     Zones.Add( "FirstColumn", FirstColumn );
        /// </code>
        /// </summary>
        protected abstract void DefineZones();

        #endregion

        #region Protected Methods

        protected virtual Control FindZone( string zoneName )
        {
            // First look in the Zones dictionary
            if ( Zones.ContainsKey( zoneName ) )
                return Zones[zoneName];

            // Then try to find a control with the zonename as the id
            Control zone = RecurseControls( this, zoneName );
            if ( zone != null )
                return zone;

            // If still no match, just add module to the form
            return this.Form;
        }

        /// <summary>
        /// Recurses the page's control heirarchy looking for any control who's id ends
        /// with the conrolId property
        /// </summary>
        /// <param name="parentControl"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected virtual Control RecurseControls( Control parentControl, string controlId )
        {
            if ( parentControl.ID != null && parentControl.ID.ToLower().EndsWith( controlId.ToLower() ) )
                return parentControl;

            foreach ( Control childControl in parentControl.Controls )
            {
                Control zoneControl = RecurseControls( childControl, controlId.ToLower() );
                if ( zoneControl != null )
                    return zoneControl;
            }

            return null;
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Initializes the page's culture to use the culture specified by the browser ("auto")
        /// </summary>
        protected override void InitializeCulture()
        {
            base.UICulture = "auto";
            base.Culture = "auto";

            base.InitializeCulture();
        }

        /// <summary>
        /// Loads all of the configured blocks for the current page into the control tree
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            ScriptManager sm = ScriptManager.GetCurrent( this.Page );
            if ( sm == null )
            {
                sm = new ScriptManager();
                sm.ID = "sManager";
                Page.Form.Controls.AddAt( 0, sm );
            }

            // Get current user/person info
            MembershipUser user = Membership.GetUser();

            if ( user != null )
            {
                UserName = user.UserName;

                if ( user.ProviderUserKey != null && user.ProviderUserKey is int)
                {
                    int personId = ( int )user.ProviderUserKey;
                    string personNameKey = "PersonName_" + personId.ToString();
                    if ( Session[personNameKey] != null )
                    {
                        UserName = Session[personNameKey].ToString();
                    }
                    else
                    {
                        Rock.Services.Crm.PersonService personService = new Services.Crm.PersonService();
                        Rock.Models.Crm.Person person = personService.GetPerson( personId );
                        if ( person != null )
                        {
                            UserName = person.FullName;
                            CurrentPerson = person;
                        }

                        Session[personNameKey] = UserName;
                    }
                }
            }

            // If a PageInstance exists
            if ( PageInstance != null )
            {
                // check if page should have been loaded via ssl
                if ( !Request.IsSecureConnection && PageInstance.RequiresEncryption )
                {
                    string redirectUrl = Request.Url.ToString().Replace( "http:", "https:" );
                    Response.Redirect( redirectUrl ); 
                }
                
                if ( !PageInstance.Authorized( "View", user ) )
                {
                    if ( user == null || !user.IsApproved )
                        FormsAuthentication.RedirectToLoginPage();
                }
                else
                {
                    // set page title
                    if ( PageInstance.Title != null && PageInstance.Title != "" )
                        this.Title = PageInstance.Title;
                    else
                        this.Title = PageInstance.Name;

                    // set viewstate on/off
                    this.EnableViewState = PageInstance.EnableViewstate;

                    // Cache object used for block output caching
                    ObjectCache cache = MemoryCache.Default;

                    bool canEditPage = PageInstance.Authorized( "Edit", user );

                    // Add config elements
                    if (canEditPage)
                        AddConfigElements();

                    // Load the blocks and insert them into page zones
                    foreach ( Rock.Cms.Cached.BlockInstance blockInstance in PageInstance.BlockInstances )
                    {
                        // Get current user's permissions for the block instance
                        bool canView = blockInstance.Authorized( "View", user );
                        bool canEdit = blockInstance.Authorized( "Edit", user );
                        bool canConfig = blockInstance.Authorized( "Configure", user );  

                        // If user can't view and they haven't logged in, redirect to the login page
                        if ( !canView )
                        {
                            if ( user == null || !user.IsApproved )
                                FormsAuthentication.RedirectToLoginPage();
                        }
                        else
                        {
                            // Create block wrapper control (implements INamingContainer so child control IDs are unique for
                            // each block instance
                            Rock.Controls.HtmlGenericContainer blockWrapper = new Rock.Controls.HtmlGenericContainer( "div" );
                            blockWrapper.ID = string.Format("bid_{0}", blockInstance.Id);
                            blockWrapper.ClientIDMode = ClientIDMode.Static;
                            FindZone( blockInstance.Zone ).Controls.Add( blockWrapper );
                            blockWrapper.Attributes.Add( "class", "block-instance " +
                                ( canEdit || canConfig ? "can-edit " : "" ) +
                                HtmlHelper.CssClassFormat( blockInstance.Block.Name ) );

                            // Check to see if block is configured to use a "Cache Duration'
                            string blockCacheKey = string.Format( "Rock:BlockInstanceOutput:{0}", blockInstance.Id );
                            if ( blockInstance.OutputCacheDuration > 0 && cache.Contains( blockCacheKey ) )
                            {
                                // If the current block exists in our custom output cache, add the cached output instead of adding the control
                                blockWrapper.Controls.Add( new LiteralControl( cache[blockCacheKey] as string ) );
                            }
                            else
                            {
                                // Load the control and add to the control tree
                                Control control = TemplateControl.LoadControl( blockInstance.Block.Path );
                                control.ClientIDMode = ClientIDMode.AutoID;

                                CmsBlock cmsBlock = null;

                                // Check to see if the control was a PartialCachingControl or not
                                if ( control is CmsBlock )
                                    cmsBlock = control as CmsBlock;
                                else
                                {
                                    if ( control is PartialCachingControl && ( ( PartialCachingControl )control ).CachedControl != null )
                                        cmsBlock = ( CmsBlock )( ( PartialCachingControl )control ).CachedControl;
                                }

                                // If the current control is a cmsBlock, set it's properties
                                if ( cmsBlock != null )
                                {
                                    cmsBlock.PageInstance = PageInstance;
                                    cmsBlock.BlockInstance = blockInstance;

                                    // If the block's BlockInstanceProperty values have not yet been verified verify them.
                                    // (This provides a mechanism for block developers to define the needed blockinstance 
                                    //  attributes in code and have them automatically added to the database)
                                    if ( !blockInstance.Block.InstancePropertiesVerified )
                                        cmsBlock.VerifyInstanceAttributes();

                                    // Add the block configuration scripts and icons if user is authorized
                                    AddBlockConfig(blockWrapper, cmsBlock, blockInstance, canConfig, canEdit);

                                    // Add the block
                                    blockWrapper.Controls.Add( control );
                                }
                                else
                                    // add the generic control
                                    blockWrapper.Controls.Add( control );
                            }
                        }
                    }

                    // Add favicon and apple touch icons to page
                    if ( PageInstance.Site.FaviconUrl != null )
                    {
                        System.Web.UI.HtmlControls.HtmlLink faviconLink = new System.Web.UI.HtmlControls.HtmlLink();

                        faviconLink.Attributes.Add( "rel", "shortcut icon" );
                        faviconLink.Attributes.Add( "href", ResolveUrl("~/" + PageInstance.Site.FaviconUrl) );

                        PageInstance.AddHtmlLink( this.Page, faviconLink );
                    }

                    if ( PageInstance.Site.AppleTouchUrl != null )
                    {
                        System.Web.UI.HtmlControls.HtmlLink touchLink = new System.Web.UI.HtmlControls.HtmlLink();

                        touchLink.Attributes.Add( "rel", "apple-touch-icon" );
                        touchLink.Attributes.Add( "href", ResolveUrl("~/" + PageInstance.Site.AppleTouchUrl) );

                        PageInstance.AddHtmlLink( this.Page, touchLink );
                    }

                    // Add the page admin footer if the user is authorized to edit the page
                    if ( PageInstance.IncludeAdminFooter && canEditPage)
                    {
                        HtmlGenericControl adminFooter = new HtmlGenericControl( "div" );
                        adminFooter.ID = "cms-admin-footer";
                        adminFooter.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        this.Form.Controls.Add( adminFooter );

                        phLoadTime = new PlaceHolder();
                        adminFooter.Controls.Add( phLoadTime );

                        HtmlGenericControl buttonBar = new HtmlGenericControl( "div" );
                        adminFooter.Controls.Add( buttonBar );
                        buttonBar.Attributes.Add( "class", "button-bar" );

                        HtmlGenericControl aBlockConfig = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aBlockConfig );
                        aBlockConfig.Attributes.Add( "class", "block-config icon-button" );
                        aBlockConfig.Attributes.Add( "href", "#" );
                        aBlockConfig.Attributes.Add( "Title", "Block Configuration" );
                        aBlockConfig.InnerText = "BlockSettings";

                        HtmlGenericControl aAttributes = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aAttributes );
                        aAttributes.Attributes.Add( "class", "attributes icon-button" );
                        aAttributes.Attributes.Add( "href", "#" );
                        aAttributes.Attributes.Add( "Title", "Page Properties" );
                        aAttributes.InnerText = "Page Properties";

                        HtmlGenericControl aChildPages = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aChildPages );
                        aChildPages.Attributes.Add( "class", "page-child-pages icon-button" );
                        aChildPages.Attributes.Add( "href", ResolveUrl( string.Format( "~/pages/{0}", PageInstance.Id ) ) );
                        aChildPages.Attributes.Add( "Title", "Child Pages" );
                        aChildPages.Attributes.Add( "instance-id", PageInstance.Id.ToString() );
                        aChildPages.InnerText = "Child Pages";

                        HtmlGenericControl aPageZones = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aPageZones );
                        aPageZones.Attributes.Add( "class", "page-zones icon-button" );
                        aPageZones.Attributes.Add( "href", "#" );
                        aPageZones.Attributes.Add( "Title", "Page Zones" );
                        aPageZones.InnerText = "Page Zones";

                        HtmlGenericControl aPageSecurity = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aPageSecurity );
                        aPageSecurity.Attributes.Add( "class", "page-security icon-button" );
                        aPageSecurity.Attributes.Add( "href", ResolveUrl( string.Format( "~/Secure/{0}/{1}",
                            Rock.Cms.Security.Authorization.EncodeEntityTypeName( PageInstance.GetType() ), PageInstance.Id ) ) );
                        aPageSecurity.Attributes.Add( "Title", "Page Security" );
                        aPageSecurity.Attributes.Add( "instance-id", PageInstance.Id.ToString() );
                        aPageSecurity.InnerText = "Page Security";

                        string footerScript = @"
    $(document).ready(function () {

        $('#cms-admin-footer .block-config').click(function (ev) {
            $('.block-configuration').toggle();
            $('.block-instance').toggleClass('outline');
            return false;
        });

        $('#cms-admin-footer .page-zones').click(function (ev) {
            $('.zone-configuration').toggle();
            $('.zone-instance').toggleClass('outline');
            return false;
        });

        $('#cms-admin-footer .page-child-pages').click(function (ev) {

            var instanceId = $(this).attr('instance-id')

            $('#modalIFrame').attr('src', $(this).attr('href'));

            $('#modalDiv').bind('dialogclose', function(event, ui) {
                $('#modalDiv').unbind('dialogclose');
                $('#modalIFrame').attr('src', '');
            });
            
            $('#modalDiv').dialog('option', 'title', 'Child Pages');

            $('#modalDiv').dialog('open');
            
            return false;

        });

        $('#cms-admin-footer .page-security').click(function (ev) {

            var instanceId = $(this).attr('instance-id')

            $('#modalIFrame').attr('src', $(this).attr('href'));

            $('#modalDiv').bind('dialogclose', function(event, ui) {
                $('#modalDiv').unbind('dialogclose');
                $('#modalIFrame').attr('src', '');
            });
            
            $('#modalDiv').dialog('option', 'title', 'Page Security');

            $('#modalDiv').dialog('open');
            
            return false;

        });

    });
";

                        this.ClientScript.RegisterClientScriptBlock( this.GetType(), "cms-admin-footer", footerScript, true );
                    }

                    // Check to see if page output should be cached.  The RockRouteHandler
                    // saves the PageCacheData information for the current page to memorycache 
                    // so it should always exist
                    if ( PageInstance.OutputCacheDuration > 0 )
                    {
                        Response.Cache.SetCacheability( System.Web.HttpCacheability.Public );
                        Response.Cache.SetExpires( DateTime.Now.AddSeconds( PageInstance.OutputCacheDuration ) );
                        Response.Cache.SetValidUntilExpires( true );
                    }
                }
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            Page.Header.DataBind();
        }
        
        
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( phLoadTime != null  )
            {
                TimeSpan tsDuration = DateTime.Now.Subtract( ( DateTime )Context.Items["Request_Start_Time"] );
                phLoadTime.Controls.Add( new LiteralControl( string.Format( "{0}: {1:N2}s", "Page Load Time", tsDuration.TotalSeconds ) ) );
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the current page's value(s) for the selected attribute
        /// If the attribute doesn't exist an empty string is returned.  If there
        /// is more than one value for the attribute, the values are returned delimited
        /// by a bar character (|).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string AttributeValue( string key )
        {
            if ( PageInstance == null )
                return string.Empty;

            if ( PageInstance.AttributeValues == null )
                return string.Empty;

            if ( !PageInstance.AttributeValues.ContainsKey( key ) )
                return string.Empty;

            return string.Join( "|", PageInstance.AttributeValues[key].Value );
        }

        #endregion

        #region CMS Admin Content

        private void AddConfigElements()
        {
            string script = @"
    $(document).ready(function () {

        $('#modalDiv').dialog({ 
            autoOpen: false,
            width: 580,
            height: 600,
            modal: true
        })

        $('a.zone-blocks').click(function () {
            $('#modalIFrame').attr('src', $(this).attr('href'));
            $('#modalDiv').dialog('option', 'title', 'Zone Blocks');
            $('#modalDiv').dialog('open');
            return false;
        });

        $('a.attributes-show').click(function () {

            var instanceId = $(this).attr('instance-id')

            $('#modalIFrame').attr('src', $(this).attr('href'));

            $('#modalDiv').bind('dialogclose', function(event, ui) {
                $('#blck-cnfg-trggr-' + instanceId).click();
                $('#modalDiv').unbind('dialogclose');
                $('#modalIFrame').attr('src', '');
            });
            
            $('#modalDiv').dialog('option', 'title', 'Block Properties');

            $('#modalDiv').dialog('open');
            
            return false;

        });

        $('a.blockinstance-secure').click(function () {

            var instanceId = $(this).attr('instance-id')

            $('#modalIFrame').attr('src', $(this).attr('href'));

            $('#modalDiv').bind('dialogclose', function(event, ui) {
                $('#modalDiv').unbind('dialogclose');
                $('#modalIFrame').attr('src', '');
            });
            
            $('#modalDiv').dialog('option', 'title', 'Block Security');

            $('#modalDiv').dialog('open');
            
            return false;

        });

        $('a.blockinstance-move').click(function () {
            var elementId = $(this).attr('href');
            alert('block instance move logic goes here! (element: ' + elementId + ')');
            return false;
        });

        $('a.blockinstance-delete').click(function () {
            var elementId = $(this).attr('href');
            if (confirm('Are you sure? (element: ' + elementId + ')'))
            {
                alert('block instance delete logic goes here!');
            }
            return false;
        });

    });
";
            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "config-script", script, true);

            // Add iFrame popup div
            HtmlGenericControl modalDiv = new HtmlGenericControl("div");
            modalDiv.ClientIDMode = ClientIDMode.AutoID;
            modalDiv.Attributes.Add("id", "modalDiv");
            this.Form.Controls.Add(modalDiv);

            HtmlGenericControl modalIFrame = new HtmlGenericControl("iframe");
            modalIFrame.Attributes.Add("id", "modalIFrame");
            modalIFrame.Attributes.Add("width", "100%");
            modalIFrame.Attributes.Add("height", "545px");
            modalIFrame.Attributes.Add("marginWidth", "0");
            modalIFrame.Attributes.Add("marginHeight", "0");
            modalIFrame.Attributes.Add("frameBorder", "0");
            modalIFrame.Attributes.Add("scrolling", "auto");
            modalDiv.Controls.Add(modalIFrame);

            // Add Zone Wrappers
            foreach ( KeyValuePair<string, Control> zoneControl in this.Zones )
            {
                Control parent = zoneControl.Value.Parent;

                HtmlGenericControl zoneWrapper = new HtmlGenericControl( "div" );
                parent.Controls.AddAt( parent.Controls.IndexOf( zoneControl.Value ), zoneWrapper );
                zoneWrapper.ID = string.Format( "zone-{0}", zoneControl.Value.ID );
                zoneWrapper.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                zoneWrapper.Attributes.Add( "class", "zone-instance can-edit" );

                HtmlGenericControl zoneConfig = new HtmlGenericControl( "div" );
                zoneWrapper.Controls.Add( zoneConfig );
                zoneConfig.Attributes.Add( "style", "display: none;" );
                zoneConfig.Attributes.Add( "class", "zone-configuration" );

                zoneConfig.Controls.Add( new LiteralControl( string.Format( "<p>{0}</p> ", zoneControl.Value.ID ) ) );

                HtmlGenericControl aBlockConfig = new HtmlGenericControl( "a" );
                zoneConfig.Controls.Add( aBlockConfig );
                aBlockConfig.Attributes.Add( "class", "zone-blocks icon-button" );
                aBlockConfig.Attributes.Add( "href", ResolveUrl( string.Format("~/ZoneBlocks/{0}/{1}", PageInstance.Id, zoneControl.Value.ID ) ) );
                aBlockConfig.Attributes.Add( "Title", "Zone Blocks" );
                aBlockConfig.Attributes.Add( "zone", zoneControl.Key );
                aBlockConfig.InnerText = "Blocks";

                parent.Controls.Remove( zoneControl.Value );
                zoneWrapper.Controls.Add( zoneControl.Value );
            }

        }

        private void AddBlockConfig( Rock.Controls.HtmlGenericContainer blockWrapper, CmsBlock cmsBlock, 
            Cached.BlockInstance blockInstance, bool canConfig, bool canEdit )
        {
            if ( canConfig || canEdit )
            {
                // Add the config buttons
                HtmlGenericControl blockConfig = new HtmlGenericControl( "div" );
                blockConfig.ClientIDMode = ClientIDMode.AutoID;
                blockConfig.Attributes.Add( "class", "block-configuration" );
                blockConfig.Attributes.Add( "style", "display: none" );
                blockWrapper.Controls.Add( blockConfig );

                foreach ( Control configControl in cmsBlock.GetConfigurationControls( canConfig, canEdit ) )
                {
                    configControl.ClientIDMode = ClientIDMode.AutoID;
                    blockConfig.Controls.Add( configControl );
                }
            }
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Adds a new CSS link that will be added to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">Current System.Web.UI.Page</param>
        /// <param name="href">Path to css file.  Should be relative to layout template.  Will be resolved at runtime</param>
        public static void AddCSSLink( System.Web.UI.Page page, string href )
        {
            AddCSSLink( page, href, string.Empty );
        }

        public static void AddCSSLink( System.Web.UI.Page page, string href, string mediaType )
        {
            System.Web.UI.HtmlControls.HtmlLink htmlLink = new System.Web.UI.HtmlControls.HtmlLink();

            htmlLink.Attributes.Add( "type", "text/css" );
            htmlLink.Attributes.Add( "rel", "stylesheet" );
            htmlLink.Attributes.Add( "href", page.ResolveUrl( href ) );
            if ( mediaType != string.Empty )
                htmlLink.Attributes.Add( "media", mediaType );

            AddHtmlLink( page, htmlLink );
        }

        /// <summary>
        /// Adds a new Html link that will be added to the page header prior to the page being rendered
        /// </summary>
        public static void AddHtmlLink( System.Web.UI.Page page, HtmlLink htmlLink )
        {
            if ( page != null && page.Header != null )
                if ( !HtmlLinkExists( page, htmlLink ) )
                {
                    // Find last Link element
                    int index = 0;
                    for ( int i = page.Header.Controls.Count - 1; i >= 0; i-- )
                        if ( page.Header.Controls[i] is HtmlLink )
                        {
                            index = i;
                            break;
                        }

                    if ( index == page.Header.Controls.Count )
                    {
                        page.Header.Controls.Add( new LiteralControl( "\n\t" ) );
                        page.Header.Controls.Add( htmlLink );
                    }
                    else
                    {
                        page.Header.Controls.AddAt( ++index, new LiteralControl( "\n\t" ) );
                        page.Header.Controls.AddAt( ++index, htmlLink );
                    }
                }
        }

        private static bool HtmlLinkExists( System.Web.UI.Page page, HtmlLink newLink )
        {
            bool existsAlready = false;

            if ( page != null && page.Header != null )
                foreach ( Control control in page.Header.Controls )
                    if ( control is HtmlLink )
                    {
                        HtmlLink existingLink = ( HtmlLink )control;

                        bool sameAttributes = true;

                        foreach ( string attributeKey in newLink.Attributes.Keys )
                            if ( existingLink.Attributes[attributeKey] != null &&
                                existingLink.Attributes[attributeKey].ToLower() != newLink.Attributes[attributeKey].ToLower() )
                            {
                                sameAttributes = false;
                                break;
                            }

                        if ( sameAttributes )
                        {
                            existsAlready = true;
                            break;
                        }
                    }
            return existsAlready;
        }

        /// <summary>
        /// Adds a new script tag to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">Current System.Web.UI.Page</param>
        /// <param name="href">Path to script file.  Should be relative to layout template.  Will be resolved at runtime</param>
        public static void AddScriptLink( System.Web.UI.Page page, string path )
        {
            string relativePath = page.ResolveUrl( path );

            bool existsAlready = false;

            if ( page != null && page.Header != null )
                foreach ( Control control in page.Header.Controls )
                {
                    if ( control is LiteralControl )
                        if ( ( ( LiteralControl )control ).Text.ToLower().Contains( "src=" + relativePath.ToLower() ) )
                        {
                            existsAlready = true;
                            break;
                        }

                    if ( control is HtmlGenericControl )
                    {
                        HtmlGenericControl genericControl = ( HtmlGenericControl )control;
                        if ( genericControl.TagName.ToLower() == "script" &&
                           genericControl.Attributes["src"] != null &&
                                genericControl.Attributes["src"].ToLower() == relativePath.ToLower() )
                        {
                            existsAlready = true;
                            break;
                        }
                    }
                }

            if ( !existsAlready )
            {
                HtmlGenericControl genericControl = new HtmlGenericControl();
                genericControl.TagName = "script";
                genericControl.Attributes.Add( "src", relativePath );
                genericControl.Attributes.Add( "type", "text/javascript" );

                int index = 0;
                for ( int i = page.Header.Controls.Count - 1; i >= 0; i-- )
                    if ( page.Header.Controls[i] is HtmlGenericControl ||
                         page.Header.Controls[i] is LiteralControl )
                    {
                        index = i;
                        break;
                    }

                if ( index == page.Header.Controls.Count )
                {
                    page.Header.Controls.Add( new LiteralControl( "\n\t" ) );
                    page.Header.Controls.Add( genericControl );
                }
                else
                {
                    page.Header.Controls.AddAt( ++index, new LiteralControl( "\n\t" ) );
                    page.Header.Controls.AddAt( ++index, genericControl );
                }
            }
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageId">Page to link to</param>
        /// <param name="parms">Dictionary of parameters</param>
        public static string BuildUrl( int pageId, Dictionary<string, string> parms )
        {
            return BuildUrl(new Rock.Helpers.PageReference(pageId, -1), parms, null);
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageId">Page to link to</param>
        /// <param name="parms">Dictionary of parameters</param>
        /// <param name="queryString">Querystring to include paramters from</param>
        public static string BuildUrl( int pageId, Dictionary<string, string> parms, System.Collections.Specialized.NameValueCollection queryString )
        {
            return BuildUrl( new Rock.Helpers.PageReference( pageId, -1 ), parms, queryString );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageRef">PageReference to use for the link</param>
        /// <param name="parms">Dictionary of parameters</param>
        public static string BuildUrl( Rock.Helpers.PageReference pageRef, Dictionary<string, string> parms )
        {
            return BuildUrl( pageRef, parms, null );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageRef">PageReference to use for the link</param>
        /// <param name="parms">Dictionary of parameters</param>
        /// <param name="queryString">Querystring to include paramters from</param>
        public static string BuildUrl( Rock.Helpers.PageReference pageRef, Dictionary<string, string> parms, System.Collections.Specialized.NameValueCollection queryString )
        {
            string url = string.Empty;

            // merge parms from query string to the parms dictionary to get a single list of parms
            // skipping those parms that are already in the dictionary
            if ( queryString != null )
            {
                foreach ( string key in queryString.AllKeys )
                {
                    // check that the dictionary doesn't already have this key
                    if ( !parms.ContainsKey( key ) )
                        parms.Add( key, queryString[key].ToString() );
                }
            }

            // load route URL 
            if ( pageRef.RouteId != -1 )
            {
                url = BuildRouteURL( pageRef.RouteId, parms );
            }

            // build normal url if route url didn't process
            if ( url == string.Empty )
            {
                url = "page/" + pageRef.PageId;

                // add parms to the url
                string delimitor = "?";
                foreach ( KeyValuePair<string, string> parm in parms )
                {
                    url += delimitor + parm.Key + "=" + HttpUtility.UrlEncode( parm.Value );
                    delimitor = "&";
                }
            }
            
            // add base path to url
            url = HttpContext.Current.Request.ApplicationPath + "/" + url;
            
            return url;
        }

        // returns route based url if all required parmameters were provided
        private static string BuildRouteURL( int routeId, Dictionary<string, string> parms ) 
        {
            string routeUrl = string.Empty;
            
            foreach ( Route route in RouteTable.Routes )
            {
                if ( route.DataTokens != null && route.DataTokens["RouteId"].ToString() == routeId.ToString() )
                    routeUrl = route.Url;
            }

            // get dictionary of parms in the route
            Dictionary<string, string> routeParms = new Dictionary<string, string>();
            bool allRouteParmsProvided = true;

            var r = new Regex( @"{([A-Za-z0-9\-]+)}" );
            foreach ( Match match in r.Matches( routeUrl ) )
            {
                // add parm to dictionary
                routeParms.Add( match.Groups[1].Value, match.Value );

                // check that a value for that parm is available
                if ( !parms.ContainsKey( match.Groups[1].Value ) )
                    allRouteParmsProvided = false;
            }

            // if we have a value for all route parms build route url
            if ( allRouteParmsProvided )
            {
                // merge route parm values
                foreach ( KeyValuePair<string,string> parm in routeParms )
                {
                    // merge field
                    routeUrl = routeUrl.Replace(parm.Value, parms[parm.Key]);

                    // remove parm from dictionary
                    parms.Remove(parm.Key);
                }

                // add remaining parms to the query string
                string delimitor = "?";
                foreach ( KeyValuePair<string,string> parm in parms ) 
                {
                    routeUrl += delimitor + parm.Key + "=" + HttpUtility.UrlEncode( parm.Value );
                    delimitor = "&";
                }
                
                return routeUrl;
            }
            else
                return string.Empty;

            
        }

        #endregion

        #region Event Handlers

        //void btnSaveAttributes_Click( object sender, EventArgs e )
        //{
        //    Button btnSave = ( Button )sender;
        //    int blockInstanceId = Convert.ToInt32( btnSave.ID.Replace( "attributes-", "" ).Replace( "-hide", "" ) );

        //    Cached.BlockInstance blockInstance = PageInstance.BlockInstances.Where( b => b.Id == blockInstanceId ).FirstOrDefault();
        //    if ( blockInstance != null )
        //    {
        //        // Find the container control
        //        Control blockWrapper = RecurseControls(this, string.Format("bid_{0}", blockInstance.Id));
        //        if ( blockWrapper != null )
        //        {
        //            foreach ( Rock.Cms.Cached.Attribute attribute in blockInstance.Attributes )
        //            {
        //                //HtmlGenericControl editCell = ( HtmlGenericControl )blockWrapper.FindControl( string.Format( "attribute-{0}", attribute.Id.ToString() ) );
        //                Control control = blockWrapper.FindControl( string.Format( "attribute-field-{0}", attribute.Id.ToString() ) );
        //                if ( control != null )
        //                    blockInstance.AttributeValues[attribute.Key] = new KeyValuePair<string, string>( attribute.Name, attribute.FieldType.Field.ReadValue( control ) );
        //            }

        //            blockInstance.SaveAttributeValues( CurrentPersonId );

        //            if ( BlockInstanceAttributesUpdated != null )
        //                BlockInstanceAttributesUpdated( sender, new BlockInstanceAttributesUpdatedEventArgs( blockInstanceId ) );
        //        }
        //    }
        //}

        #endregion
    }

    #region Event Argument Classes

    internal class BlockInstanceAttributesUpdatedEventArgs : EventArgs
    {
        public int BlockInstanceID { get; private set; }

        public BlockInstanceAttributesUpdatedEventArgs( int blockInstanceID )
        {
            BlockInstanceID = blockInstanceID;
        }
    }

    internal class JsonResult
    {
        public string Action { get; set; }
        public object Result { get; set; }

        public JsonResult( string action, object result )
        {
            Action = action;
            Result = result;
        }

        public string Serialize()
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer =
                new System.Web.Script.Serialization.JavaScriptSerializer();

            StringBuilder sb = new StringBuilder();

            serializer.Serialize( this, sb );

            return sb.ToString();
        }
    }


    #endregion

}
