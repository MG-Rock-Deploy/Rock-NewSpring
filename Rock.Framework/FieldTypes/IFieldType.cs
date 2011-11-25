﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Models.Core;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Interface that a custom field type must implement
    /// </summary>
    public interface IFieldType
    {
        /// <summary>
        /// Gets the qualifiers.
        /// </summary>
        List<FieldQualifier> Qualifiers { get; }

        /// <summary>
        /// Gets or sets the qualifier values.
        /// </summary>
        /// <value>
        /// The qualifier values. The Dictionary's key contains the qualifier key, the KeyValuePair's
        /// key contains the qualifier name, and the KeyValuePair's value contains the qualifier 
        /// value
        /// </value>
        Dictionary<string, KeyValuePair<string, string>> QualifierValues { get; set;  }

        /// <summary>
        /// Formats the value based on the type and qualifiers
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        string FormatValue( string value, bool condensed );

        /// <summary>
        /// Creates an HTML control.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <returns></returns>
        Control CreateControl( string value, bool setValue );

        /// <summary>
        /// Reads the value of the control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        string ReadValue( Control control );
    }
}
