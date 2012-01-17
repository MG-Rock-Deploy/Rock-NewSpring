//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Configuration;
using System.ComponentModel.DataAnnotations;

using Rock.Data;

namespace Rock.CRM
{
    //[MetadataType(typeof(CommentPerson))]
    public partial class Person
    {
        /// <summary>
        /// Gets the full name.
        /// </summary>
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        [NotMapped]
        public DateTime? BirthDate
        {
            // notes
            // if no birthday is available then DateTime.MinValue is returned
            // if no birth year is given then the birth year will be DateTime.MinValue.Year
            get
            {
                if ( BirthDay == null || BirthMonth == null )
                {
                    return null;
                }
                else
                {
                    string birthYear = ( BirthYear ?? DateTime.MinValue.Year ).ToString();
                    return Convert.ToDateTime( BirthMonth.ToString() + "/" + BirthDay.ToString() + "/" + birthYear );
                }
            }

            set
            {
                if ( value.HasValue )
                {
                    BirthMonth = value.Value.Month;
                    BirthDay = value.Value.Day;
                    BirthYear = value.Value.Year;
                }
                else
                {
                    BirthMonth = null;
                    BirthDay = null;
                    BirthYear = null;
                }
            }
        }

        public string EncryptedID
        {
            get
            {
                string encryptionPhrase = ConfigurationManager.AppSettings["EncryptionPhrase"];
                if (String.IsNullOrWhiteSpace(encryptionPhrase))
                    encryptionPhrase = "Rock Rocks!";

                string identifier = this.Guid.ToString() + "|" + this.Id.ToString();
                return Rock.Security.Encryption.EncryptString( identifier, encryptionPhrase );
            }
        }
    }

#pragma warning disable
    
    public class CommentPerson
    {
        [TrackChanges]
        [Required( ErrorMessage = "First Name must be between 1 and 12 characters" )]
        [StringLength( 12, ErrorMessage = "First Name is required" )]
        public string FirstName { get; set; }

        [TrackChanges]
        public string NickName { get; set; }

        [TrackChanges]
        public string LastName { get; set; }
    }

#pragma warning restore

    /// <summary>
    /// The gender of a person
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Male
        /// </summary>
        Male = 1,

        /// <summary>
        /// Female
        /// </summary>
        Female = 2
    }


}
