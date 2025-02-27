﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using System.Web.Security.AntiXss;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Web
{
    /// <summary>
    /// Helper class to work with the PageReference field type
    /// </summary>
    public class PageReference
    {
        #region Properties

        /// <summary>
        /// Gets or sets the page id.
        /// </summary>
        public int PageId { get; set; }

        /// <summary>
        /// Gets the route id.
        /// </summary>
        public int RouteId { get; set; }

        /// <summary>
        /// Gets the route parameters.
        /// </summary>
        /// <value>
        /// The route parameters as a case-insensitive dictionary of key/value pairs.
        /// </value>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <value>
        /// The query string.
        /// </value>
        public NameValueCollection QueryString { get; set; } = new NameValueCollection();

        /// <summary>
        /// Gets or sets the bread crumbs.
        /// </summary>
        /// <value>
        /// The bread crumbs.
        /// </value>
        public List<BreadCrumb> BreadCrumbs { get; set; } = new List<BreadCrumb>();

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                if ( PageId != 0 )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class.
        /// </summary>
        public PageReference()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="linkedPageValue">The linked page value.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="queryString">The query string.</param>
        public PageReference( string linkedPageValue, Dictionary<string, string> parameters = null, NameValueCollection queryString = null )
        {
            if ( !string.IsNullOrWhiteSpace( linkedPageValue ) )
            {
                string[] items = linkedPageValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                //// linkedPageValue is in format "Page.Guid,PageRoute.Guid"
                //// If only the Page.Guid is specified this is just a reference to a page without a special route
                //// In case the PageRoute record can't be found from PageRoute.Guid (maybe the pageroute was deleted), fall back to the Page without a PageRoute

                Guid pageGuid = Guid.Empty;
                if ( items.Length > 0 )
                {
                    if ( Guid.TryParse( items[0], out pageGuid ) )
                    {
                        var pageCache = PageCache.Get( pageGuid );
                        if ( pageCache != null )
                        {
                            // Set the page
                            PageId = pageCache.Id;

                            Guid pageRouteGuid = Guid.Empty;
                            if ( items.Length == 2 )
                            {
                                if ( Guid.TryParse( items[1], out pageRouteGuid ) )
                                {
                                    var pageRouteInfo = pageCache.PageRoutes.FirstOrDefault( a => a.Guid == pageRouteGuid );
                                    if ( pageRouteInfo != null )
                                    {
                                        // Set the route
                                        RouteId = pageRouteInfo.Id;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if ( parameters != null )
            {
                Parameters = new Dictionary<string, string>( parameters );
            }

            if ( queryString != null )
            {
                QueryString = new NameValueCollection( queryString );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        public PageReference( int pageId )
        {
            Parameters = new Dictionary<string, string>();
            PageId = pageId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="routeId">The route id.</param>
        public PageReference( int pageId, int routeId )
            : this( pageId )
        {
            RouteId = routeId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="routeId">The route id.</param>
        /// <param name="parameters">The route parameters.</param>
        public PageReference( int pageId, int routeId, Dictionary<string, string> parameters )
            : this( pageId, routeId )
        {
            Parameters = parameters != null ? new Dictionary<string, string>( parameters ) : new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageId">The page id.</param>
        /// <param name="routeId">The route id.</param>
        /// <param name="parameters">The route parameters.</param>
        /// <param name="queryString">The query string.</param>
        public PageReference( int pageId, int routeId, Dictionary<string, string> parameters, NameValueCollection queryString )
            : this( pageId, routeId, parameters )
        {
            QueryString = queryString != null ? new NameValueCollection( queryString ) : new NameValueCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference" /> class.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        public PageReference( PageReference pageReference )
            : this( pageReference.PageId, pageReference.RouteId, pageReference.Parameters, pageReference.QueryString )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageReference"/> class from a url.
        /// </summary>
        /// <param name="uri">The URI e.g.: new Uri( ResolveRockUrlIncludeRoot("~/Person/5")</param>
        /// <param name="applicationPath">The application path e.g.: HttpContext.Current.Request.ApplicationPath</param>
        public PageReference( Uri uri, string applicationPath )
        {
            Parameters = new Dictionary<string, string>();

            var routeInfo = new Rock.Web.RouteInfo( uri, applicationPath );
            if ( routeInfo != null )
            {
                if ( routeInfo.RouteData.Values["PageId"] != null )
                {
                    PageId = routeInfo.RouteData.Values["PageId"].ToString().AsInteger();
                }
                else if ( routeInfo.RouteData.DataTokens["PageRoutes"] != null )
                {
                    var pages = routeInfo.RouteData.DataTokens["PageRoutes"] as List<PageAndRouteId>;
                    if ( pages != null && pages.Count > 0 )
                    {
                        if ( pages.Count == 1 )
                        {
                            var pageAndRouteId = pages.First();
                            PageId = pageAndRouteId.PageId;
                            RouteId = pageAndRouteId.RouteId;
                        }
                        else
                        {
                            SiteCache site = SiteCache.GetSiteByDomain( uri.Host );
                            if ( site != null )
                            {
                                foreach ( var pageAndRouteId in pages )
                                {
                                    var pageCache = PageCache.Get( pageAndRouteId.PageId );
                                    if ( pageCache != null && pageCache.Layout != null && pageCache.Layout.SiteId == site.Id )
                                    {
                                        PageId = pageAndRouteId.PageId;
                                        RouteId = pageAndRouteId.RouteId;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    // Add route parameters.
                    foreach ( var routeParm in routeInfo.RouteData.Values )
                    {
                        Parameters.Add( routeParm.Key, ( string ) routeParm.Value );
                    }
                }
            }

            // Add query parameters.
            QueryString = HttpUtility.ParseQueryString( uri.Query );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Builds the URL.
        /// </summary>
        /// <returns></returns>
        public string BuildUrl()
        {
            return BuildUrl( false );
        }

        /// <summary>
        /// Builds the URL.
        /// </summary>
        /// <param name="removeMagicToken">if set to <c>true</c> [remove magic token].</param>
        /// <returns></returns>
        public string BuildUrl( bool removeMagicToken )
        {
            string url = string.Empty;

            var parms = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            // Add any route parameters
            if ( Parameters != null )
            {
                foreach ( var route in Parameters )
                {
                    if ( ( !removeMagicToken || route.Key.ToLower() != "rckipid" ) && !parms.ContainsKey( route.Key ) )
                    {
                        parms.Add( route.Key, route.Value );
                    }
                }
            }

            // merge parms from query string to the parms dictionary to get a single list of parms
            // skipping those parms that are already in the dictionary
            if ( QueryString != null )
            {
                foreach ( string key in QueryString.AllKeys.Where( a => a.IsNotNullOrWhiteSpace() ) )
                {
                    if ( !removeMagicToken || key.ToLower() != "rckipid" )
                    {
                        // check that the dictionary doesn't already have this key
                        if ( key != null && !parms.ContainsKey( key ) && QueryString[key] != null )
                        {
                            parms.Add( key, QueryString[key].ToString() );
                        }
                    }
                }
            }

            // See if there's a route that matches all parms
            if ( RouteId == 0 )
            {
                RouteId = GetRouteIdFromPageAndParms() ?? 0;
            }

            // load route URL
            if ( RouteId != 0 )
            {
                url = BuildRouteURL( parms );
            }

            // build normal url if route url didn't process
            if ( url == string.Empty )
            {
                url = "page/" + PageId;

                // add parms to the url
                if ( parms != null )
                {
                    string delimitor = "?";
                    foreach ( KeyValuePair<string, string> parm in parms )
                    {
                        url += delimitor + HttpUtility.UrlEncode( parm.Key ) + "=" + HttpUtility.UrlEncode( parm.Value );
                        delimitor = "&";
                    }
                }
            }

            // add base path to url -- Fixed bug #84
            url = ( HttpContext.Current.Request.ApplicationPath == "/" ) ? "/" + url : HttpContext.Current.Request.ApplicationPath + "/" + url;

            return url;
        }

        /// <summary>
        /// Builds and HTML encodes the URL.
        /// </summary>
        /// <returns></returns>
        public string BuildUrlEncoded()
        {
            return BuildUrlEncoded( false );
        }

        /// <summary>
        /// Builds and HTML encodes the URL.
        /// </summary>
        /// <param name="removeMagicToken">if set to <c>true</c> [remove magic token].</param>
        /// <returns></returns>
        public string BuildUrlEncoded( bool removeMagicToken )
        {
            return AntiXssEncoder.HtmlEncode( BuildUrl( removeMagicToken ), false );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the route id from page and parms.
        /// </summary>
        /// <returns></returns>
        private int? GetRouteIdFromPageAndParms()
        {
            var pageCache = PageCache.Get( PageId );
            if ( pageCache != null && pageCache.PageRoutes.Any() )
            {
                var r = new Regex( @"(?<={)[A-Za-z0-9\-]+(?=})" );

                foreach ( var item in pageCache.PageRoutes )
                {
                    // If route contains no parameters, and no parameters were provided, return this route
                    var matches = r.Matches( item.Route );
                    if ( matches.Count == 0 && ( Parameters == null || Parameters.Count == 0 ) )
                    {
                        return item.Id;
                    }

                    // If route contains the same number of parameters as provided, check to see if they all match names
                    if ( matches.Count > 0 && Parameters != null && Parameters.Count == matches.Count )
                    {
                        bool matchesAllParms = true;

                        foreach ( Match match in matches )
                        {
                            if ( !Parameters.ContainsKey( match.Value ) )
                            {
                                matchesAllParms = false;
                                break;
                            }
                        }

                        if ( matchesAllParms )
                        {
                            return item.Id;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Builds the route URL.
        /// </summary>
        /// <param name="parms">The parms.</param>
        /// <returns></returns>
        public string BuildRouteURL( Dictionary<string, string> parms )
        {
            string routeUrl = string.Empty;

            foreach ( var route in RouteTable.Routes.OfType<Route>() )
            {
                if ( route != null && route.DataTokens != null && route.DataTokens.ContainsKey( "PageRoutes" ) )
                {
                    var pageAndRouteIds = route.DataTokens["PageRoutes"] as List<PageAndRouteId>;
                    if ( pageAndRouteIds != null && pageAndRouteIds.Any( r => r.RouteId == RouteId ) )
                    {
                        routeUrl = route.Url;
                        break;
                    }
                }
            }

            // get dictionary of parms in the route
            Dictionary<string, string> routeParms = new Dictionary<string, string>();
            bool allRouteParmsProvided = true;

            var regEx = new Regex( @"{([A-Za-z0-9\-]+)}" );
            foreach ( Match match in regEx.Matches( routeUrl ) )
            {
                // add parm to dictionary
                routeParms.Add( match.Groups[1].Value, match.Value );

                // check that a value for that parm is available
                if ( parms == null || !parms.ContainsKey( match.Groups[1].Value ) )
                {
                    allRouteParmsProvided = false;
                }
            }

            // if we have a value for all route parms build route url
            if ( allRouteParmsProvided )
            {
                // merge route parm values
                foreach ( KeyValuePair<string, string> parm in routeParms )
                {
                    // merge field. This has to be UrlPathEncode to ensure a space is replaced with a
                    // %20 instead of + since this is to the left of the query string delimeter.
                    routeUrl = routeUrl.Replace( parm.Value, HttpUtility.UrlPathEncode( parms[parm.Key] ) );

                    // remove parm from dictionary
                    parms.Remove( parm.Key );
                }

                // add remaining parms to the query string
                if ( parms != null )
                {
                    string delimitor = "?";
                    foreach ( KeyValuePair<string, string> parm in parms )
                    {
                        routeUrl += delimitor + HttpUtility.UrlEncode( parm.Key ) + "=" + HttpUtility.UrlEncode( parm.Value );
                        delimitor = "&";
                    }
                }

                return routeUrl;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Builds the storage key.
        /// </summary>
        /// <param name="suffix">The suffix - if any - that should be added to the base key.</param>
        /// <returns>The base key + any suffix that should be added.</returns>
        private static string BuildStorageKey( string suffix )
        {
            return $"RockPageReferenceHistory{( string.IsNullOrWhiteSpace( suffix ) ? string.Empty : suffix )}";
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( PageId <= 0 )
            {
                return base.ToString();
            }

            var pageCache = PageCache.Get( this.PageId );
            if ( pageCache == null )
            {
                return base.ToString();
            }

            var pageRoute = pageCache.PageRoutes.FirstOrDefault( a => a.Id == this.RouteId );
            return pageRoute != null ? pageRoute.Route : pageCache.InternalName;
        }

        /// <summary>
        /// If this is reference to a PageRoute, this will return the Route, otherwise it will return the normal URL of the page
        /// </summary>
        /// <value>
        /// The route.
        /// </value>
        public string Route
        {
            get
            {
                if ( PageId <= 0 )
                {
                    return null;
                }

                var pageCache = PageCache.Get( PageId );
                if ( pageCache == null )
                {
                    return null;
                }

                var pageRoute = pageCache.PageRoutes.FirstOrDefault( a => a.Id == RouteId );
                return pageRoute != null ? pageRoute.Route : BuildUrl();
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets the parent page references.
        /// </summary>
        /// <param name="rockPage">The rock page.</param>
        /// <param name="currentPage">The current page.</param>
        /// <param name="currentPageReference">The current page reference.</param>
        /// <returns></returns>
        public static List<PageReference> GetParentPageReferences( RockPage rockPage, PageCache currentPage, PageReference currentPageReference )
        {
            return GetParentPageReferences( rockPage, currentPage, currentPageReference, null );
        }

        /// <summary>
        /// Gets the parent page references.
        /// </summary>
        /// <param name="rockPage">The rock page.</param>
        /// <param name="currentPage">The current page.</param>
        /// <param name="currentPageReference">The current page reference.</param>
        /// <param name="keySuffix">The suffix - if any - that should be added to the base key that will be used to get the parent page references.</param>
        /// <returns></returns>
        public static List<PageReference> GetParentPageReferences( RockPage rockPage, PageCache currentPage, PageReference currentPageReference, string keySuffix )
        {
            var key = BuildStorageKey( keySuffix );

            // Get previous page references in nav history
            var pageReferenceHistory = HttpContext.Current.Session[key] as List<PageReference>;

            // Current page hierarchy references
            var pageReferences = new List<PageReference>();

            if ( currentPage != null )
            {
                var parentPage = currentPage.ParentPage;
                if ( parentPage != null )
                {
                    var currentParentPages = parentPage.GetPageHierarchy();
                    if ( currentParentPages != null && currentParentPages.Count > 0 )
                    {
                        currentParentPages.Reverse();
                        foreach ( PageCache page in currentParentPages )
                        {
                            PageReference parentPageReference = null;
                            if ( pageReferenceHistory != null )
                            {
                                parentPageReference = pageReferenceHistory.Where( p => p.PageId == page.Id ).FirstOrDefault();
                            }

                            if ( parentPageReference == null )
                            {
                                parentPageReference = new PageReference();
                                parentPageReference.PageId = page.Id;

                                parentPageReference.BreadCrumbs = new List<BreadCrumb>();
                                parentPageReference.QueryString = new NameValueCollection();
                                parentPageReference.Parameters = new Dictionary<string, string>();

                                string bcName = page.BreadCrumbText;
                                if ( bcName != string.Empty )
                                {
                                    parentPageReference.BreadCrumbs.Add( new BreadCrumb( bcName, parentPageReference.BuildUrl() ) );
                                }

                                foreach ( var block in page.Blocks.Where( b => b.BlockLocation == Model.BlockLocation.Page ) )
                                {
                                    try
                                    {
                                        System.Web.UI.Control control = rockPage.TemplateControl.LoadControl( block.BlockType.Path );
                                        if ( control is RockBlock )
                                        {
                                            RockBlock rockBlock = control as RockBlock;
                                            rockBlock.SetBlock( page, block );
                                            rockBlock.GetBreadCrumbs( parentPageReference ).ForEach( c => parentPageReference.BreadCrumbs.Add( c ) );
                                        }

                                        control = null;
                                    }
                                    catch ( Exception ex )
                                    {
                                        ExceptionLogService.LogException( ex, HttpContext.Current, currentPage.Id, currentPage.Layout.SiteId );
                                    }
                                }
                            }

                            parentPageReference.BreadCrumbs.ForEach( c => c.Active = false );
                            pageReferences.Add( parentPageReference );
                        }
                    }
                }
            }

            return pageReferences;
        }

        /// <summary>
        /// Saves the history.
        /// </summary>
        /// <param name="pageReferences">The page references.</param>
        public static void SavePageReferences( List<PageReference> pageReferences )
        {
            SavePageReferences( pageReferences, null );
        }

        /// <summary>
        /// Saves the history.
        /// </summary>
        /// <param name="pageReferences">The page references.</param>
        /// <param name="keySuffix">The suffix - if any - that should be added to the base key that will be used to save the parent page references.</param>
        public static void SavePageReferences( List<PageReference> pageReferences, string keySuffix )
        {
            var key = BuildStorageKey( keySuffix );

            HttpContext.Current.Session[key] = pageReferences;
        }

        /// <summary>
        /// Gets a reference for the specified page including the route that matches the greatest number of supplied parameters.
        /// Parameters that do not match the route are included as query parameters.
        /// </summary>
        /// <param name="pageId">The target page.</param>
        /// <param name="parameters">The set of parameters that are candidates for matching to the routes associated with the target page.</param>
        /// <returns></returns>
        public static PageReference GetBestMatchForParameters( int pageId, Dictionary<string, string> parameters )
        {
            // Get the definition of the specified page if it exists.
            var outputPage = PageCache.Get( pageId );

            // Find a route associated with the page that contains the maximum number of available parameters.
            int matchedRouteId = 0;
            var routeParameters = new List<string>();
            if ( outputPage != null )
            {
                foreach ( var route in outputPage.PageRoutes )
                {
                    if ( string.IsNullOrEmpty( route.Route ) )
                    {
                        continue;
                    }

                    var matchedTokens = new List<string>();
                    foreach ( var parameterName in parameters.Keys )
                    {
                        var token = "{" + parameterName + "}";
                        if ( route.Route.IndexOf( token, StringComparison.OrdinalIgnoreCase ) > 0 )
                        {
                            matchedTokens.Add( parameterName );
                            matchedRouteId = route.Id;
                            break;
                        }
                    }
                    if ( routeParameters.Count == 0 || matchedTokens.Count > routeParameters.Count )
                    {
                        routeParameters = matchedTokens;
                    }
                }
            }

            // Separate the query and route parameters.
            var queryValues = new NameValueCollection();
            var routeValues = new Dictionary<string, string>();
            foreach ( var p in parameters )
            {
                if ( routeParameters.Contains( p.Key ) )
                {
                    routeValues.Add( p.Key, p.Value );
                }
                else
                {
                    queryValues.Add( p.Key, p.Value );
                }
            }

            // Create and return a new page reference.
            var pageReference = new PageReference( pageId, matchedRouteId, routeValues, queryValues );
            return pageReference;
        }

        #endregion
    }
}