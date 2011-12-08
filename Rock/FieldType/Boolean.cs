﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldType
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a | delimited list
    /// </summary>
    public class Boolean : Field
    {
        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( string value, bool condensed )
        {
            if ( string.IsNullOrEmpty(value) ? false : System.Boolean.Parse( value ) )
                return condensed ? "Y" : "Yes";
            else
                return condensed ? "N" : "No";
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool IsValid( string value, out string message )
        {
            bool boolValue = false;
            if ( !bool.TryParse( value, out boolValue ) )
            {
                message = "Invalid boolean value";
                return false;
            }

            return base.IsValid( value, out message );
        }

        /// <summary>
        /// Renders the controls neccessary for prompting user for a new value and adds them to the parentControl
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public override Control CreateControl( string value, bool setValue )
        {
            CheckBox cb = new CheckBox();
            if (setValue)
                cb.Checked = string.IsNullOrEmpty(value) ? false : System.Boolean.Parse( value );
            return cb;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public override string ReadValue( Control control )
        {
            if ( control != null && control is CheckBox )
                return ( ( CheckBox )control ).Checked.ToString();
            return null;
        }
    }
}