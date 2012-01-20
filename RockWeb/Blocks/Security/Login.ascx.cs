﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

using Facebook;

using Rock.CMS;
using Rock.CRM;

namespace RockWeb.Blocks.Security
{
    [Rock.Attribute.Property( 0, "Enable Facebook Login", "FacebookEnabled", "", "Enables the user to login using Facebook.  This assumes that the site is configured with both a Facebook App Id and Secret.", false, "True", "Rock", "Rock.FieldTypes.Boolean" )]
    public partial class Login : Rock.Web.UI.Block
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            pnlMessage.Visible = false;

            // Add the jquery that will handle client-side validation and display the invalid controls correctly
            PageInstance.AddScriptLink( Page, "~/Scripts/Rock/validation.js" );

            tbPassword.TextBox.TextMode = TextBoxMode.Password;

            // Determine if Facebook login enabled
            string facebookAppId = PageInstance.Site.FacebookAppId;
            string facebookAppSecret = PageInstance.Site.FacebookAppSecret;
            bool facebookEnabled = Convert.ToBoolean( AttributeValue( "FacebookEnabled" ) );

            // disable the facebook login button if it's not able to be used
            if ( !facebookEnabled ||  facebookAppId == "" || facebookAppSecret == "")
                phFacebookLogin.Visible = false;
            
            // Check for Facebook query string params. If exists, assume it's a redirect back from Facebook.
            if ( Request.QueryString["code"] != null )
                ProcessOAuth( Request.QueryString["code"], Request.QueryString["state"] );
        }

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnLogin_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                if ( Rock.CMS.User.ValidateUser( tbUserName.Text, tbPassword.Text ) )
                {
                    // Save the current user's authentication cookie
                    FormsAuthenticationTicket tkt;
                    string cookiestr;
                    HttpCookie ck;
                    tkt = new FormsAuthenticationTicket( 1, tbUserName.Text, DateTime.Now, DateTime.Now.AddMinutes( 30 ), cbRememberMe.Checked, "your custom data" );
                    cookiestr = FormsAuthentication.Encrypt( tkt );
                    ck = new HttpCookie( FormsAuthentication.FormsCookieName, cookiestr );
                    if ( cbRememberMe.Checked )
                        ck.Expires = tkt.Expiration;
                    ck.Path = FormsAuthentication.FormsCookiePath;
                    Response.Cookies.Add( ck );

                    // Redirect to any specified url, or the default url if non was specified
                    string strRedirect;
                    strRedirect = Request["ReturnUrl"];
                    if ( strRedirect == null )
                        strRedirect = FormsAuthentication.DefaultUrl;
                    Response.Redirect( strRedirect, true );
                }
                else
                {
                    DisplayError( "Invalid Login Information" );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnNewAccount_Click( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }

        /// <summary>
        /// Redirects to Facebook w/ necessary permissions required to gain user approval.
        /// </summary>
        /// <param name="sender">Trigger object of event</param>
        /// <param name="e">Arguments passed in</param>
        protected void lbFacebookLogin_Click( object sender, EventArgs e )
        {
            var returnUrl = Request.QueryString["returnurl"];
            var oAuthClient = new FacebookOAuthClient( FacebookApplication.Current ) { RedirectUri = new Uri( GetOAuthRedirectUrl() ) };
            oAuthClient.AppId = PageInstance.Site.FacebookAppId;
            oAuthClient.AppSecret = PageInstance.Site.FacebookAppSecret;

            // setup some facebook connection settings
            var settings = new Dictionary<string, object>
            {
                { "display", "popup" },
                { "scope", "user_birthday,email,read_stream,read_friendlists"},
                { "state", returnUrl ?? FormsAuthentication.DefaultUrl}
            };

            // Grab publically available information. No special permissions needed for authentication.
            var loginUri = oAuthClient.GetLoginUrl( settings );
            Response.Redirect( loginUri.AbsoluteUri );
        }

        /// <summary>
        /// Awaits permission of facebook user and will issue authenication cookie if successful.
        /// </summary>
        /// <param name="code">Facebook authorization code</param>
        /// <param name="state">Redirect url</param>
        private void ProcessOAuth( string code, string state )
        {
            FacebookOAuthResult oAuthResult;

            if ( FacebookOAuthResult.TryParse( Request.Url, out oAuthResult ) && oAuthResult.IsSuccess )
            {
                try
                {
                    // create client to read response
                    var oAuthClient = new FacebookOAuthClient( FacebookApplication.Current ) { RedirectUri = new Uri( GetOAuthRedirectUrl() ) };
                    oAuthClient.AppId = PageInstance.Site.FacebookAppId;
                    oAuthClient.AppSecret = PageInstance.Site.FacebookAppSecret;
                    dynamic tokenResult = oAuthClient.ExchangeCodeForAccessToken( code );
                    string accessToken = tokenResult.access_token;

                    FacebookClient fbClient = new FacebookClient( accessToken );
                    dynamic me = fbClient.Get( "me" );
                    string facebookId = me.id.ToString();

                    // query for matching id in the user table 
                    UserService userService = new UserService();
                    var user = userService.Queryable().FirstOrDefault( u => u.UserName == facebookId && u.AuthenticationType == AuthenticationType.Facebook ); 

                    // if not user was found see if we can find a match in the person table
                    if ( user == null )
                    {
                        try
                        {
                            // determine if we can find a match and if so add an user login record

                            // get properties from Facebook dynamic object
                            string lastName = me.last_name.ToString();
                            string firstName = me.first_name.ToString();
                            string email = me.email.ToString();

                            var personService = new PersonService();
                            var person = personService.Queryable().FirstOrDefault( u => u.LastName == lastName && (u.FirstName == firstName || u.NickName == firstName) && u.Email == email );

                            if ( person != null )
                            {
                                User.CreateUser( person, AuthenticationType.Facebook, facebookId, "fb", true, person.Id );

                                // since we have the data enter the birthday from Facebook to the db if we don't have it yet
                                DateTime birthdate = Convert.ToDateTime( me.birthday.ToString() );

                                if ( person.BirthDay == null )
                                {
                                    person.BirthDate = birthdate;
                                    personService.Save( person, person.Id );
                                }

                            }
                            else
                            {
                                // exact person match wasn't found
                                // TODO: figure out what to do next the plan is:
                                //         + Look for records with matching emails as the one from Facebook and allow the user to pick the correct person
                                //         + If no match email is found (or the user states none of the provided ones is him) then allow then to register
                                //           Registration form should have facebook fields pre-populated.

                                return;
                            }
                        }
                        catch //( Exception ex )
                        {
                            // TODO: probably should report something...
                        }

                        // TODO: Show label indicating inability to find user corresponding to facebook id
                    }
                    else
                    {
                        // update user record noting the login datetime
                        user.LastLoginDate = DateTime.Now;
                        user.LastActivityDate = DateTime.Now;
                        userService.Save( user, user.PersonId );
                    }

                    FormsAuthentication.SetAuthCookie( user.UserName, false );

                    if ( state != null )
                    {
                        Response.Redirect( state );
                    }
                }
                catch //( FacebookOAuthException oae )
                {
                    // TODO: Add error handeling
                    // Error validating verification code. (usually from wrong return url very picky with formatting)
                    // Error validating client secret.
                    // Error validating application.
                }
            }
        }

        private string GetOAuthRedirectUrl()
        {
            Uri uri = new Uri( HttpContext.Current.Request.Url.ToString() );
            return uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + uri.LocalPath;
        }


    }

    // helpful links
    //  http://blog.prabir.me/post/Facebook-CSharp-SDK-Writing-your-first-Facebook-Application.aspx
}