﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

using Rock.CMS;

namespace RockWeb.Blocks.Security
{
    public partial class CreateAccount : Rock.Web.UI.Block
    {
        private string _returnUrl;

        protected void Page_Init( object sender, EventArgs e )
        {
            string clientId = cuWizard.CreateUserStep.ContentTemplateContainer.FindControl( "UserName" ).ClientID;
            string jqScript = @"
    $(document).ready(function () {
        var usernameUnavailableRow = $('#usernameUnavailableRow');
        var availabilityMessage = $('#availabilityMessage');
        var usernameTextbox = $('#" + clientId + @"');

        usernameUnavailableRow.hide();

        usernameTextbox.blur(function () {
            if ($(this).val()) {
                $.getJSON('AspxServices/UsernameAvailable.aspx?' + escape($(this).val()), function (results) {
                    if (results.available) {
                        if (usernameUnavailableRow.is(':visible')) {
                            availabilityMessage.html('This username is available.');
                            availabilityMessage.addClass('usernameAvailable');
                            availabilityMessage.removeClass('usernameTaken');
                        }
                    }
                    else {
                        usernameUnavailableRow.show();
                        availabilityMessage.html('This username is already taken!');
                        availabilityMessage.addClass('usernameTaken');
                        availabilityMessage.removeClass('usernameAvailable');
                    }
                });
            }
        });
    });";
            Page.ClientScript.RegisterClientScriptBlock( this.GetType(), "usernameAvailable", jqScript, true );

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _returnUrl = Request.QueryString["returnurl"];
        }

        

        private string GetCreateAccountUrl()
        {
            return new Uri(string.Format("~/page/{0}{1}", PageInstance.Id, Request.QueryString)).AbsoluteUri;
        }
   }
}