﻿using System;

namespace Rock.Address
{
    /// <summary>
    /// Helper class for wrapping the properties of a MEF class to use in databinding
    /// </summary>
    public class ServiceDescription
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ServiceDescription"/> is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        public bool Active { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDescription"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="service">The service.</param>
        public ServiceDescription( int id, Rock.Attribute.IHasAttributes service )
        {
            Id = id;

            Type type = service.GetType();

            Name = type.Name;

            // Look for a DescriptionAttribute on the class and if found, use its value for the description
            // property of this class
            var descAttributes = type.GetCustomAttributes( typeof( System.ComponentModel.DescriptionAttribute ), false );
            if ( descAttributes != null )
                foreach ( System.ComponentModel.DescriptionAttribute descAttribute in descAttributes )
                    Description = descAttribute.Description;

            // If the class has an PropertyAttribute with 'Active' as the key get it's value for the property
            // otherwise default to true
            if ( service.AttributeValues.ContainsKey( "Active" ) )
                Active = bool.Parse( service.AttributeValues["Active"].Value );
            else
                Active = true;
        }
    }
}