//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
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
using System.Linq;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Entities
{
    /// <summary>
    /// SignatureDocument View Model
    /// </summary>
    public partial class SignatureDocumentBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the applies to person alias identifier.
        /// </summary>
        /// <value>
        /// The applies to person alias identifier.
        /// </value>
        public int? AppliesToPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the assigned to person alias identifier.
        /// </summary>
        /// <value>
        /// The assigned to person alias identifier.
        /// </value>
        public int? AssignedToPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the binary file identifier.
        /// </summary>
        /// <value>
        /// The binary file identifier.
        /// </value>
        public int? BinaryFileId { get; set; }

        /// <summary>
        /// The date and time the document completion email was sent.
        /// </summary>
        /// <value>
        /// The completion email sent date and time.
        /// </value>
        public DateTime? CompletionEmailSentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the document key.
        /// </summary>
        /// <value>
        /// The document key.
        /// </value>
        public string DocumentKey { get; set; }

        /// <summary>
        /// The ID of the entity to which the document is related.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the EntityId of the entity that this signature document entity applies to.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// The EntityType that this document is related to (example Rock.Model.Registration)
        /// </summary>
        /// <value>
        /// A System.Int32 representing the EntityTypeId of the Rock.Model.EntityType of the entity that this signature document applies to.
        /// </value>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the invite count.
        /// </summary>
        /// <value>
        /// The invite count.
        /// </value>
        public int InviteCount { get; set; }

        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        /// <value>
        /// The request date.
        /// </value>
        public DateTime? LastInviteDate { get; set; }

        /// <summary>
        /// Gets or sets the last status date.
        /// </summary>
        /// <value>
        /// The last status date.
        /// </value>
        public DateTime? LastStatusDate { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// The encrypted data that was collected during a drawn signature type.
        /// Use Rock.Model.SignatureDocument.SignatureData to set this from the unencrypted drawn signature.
        /// </summary>
        /// <value>
        /// A System.String representing the signature data.
        /// </value>
        public string SignatureDataEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the SignatureDocumentTemplateId of the Rock.Model.SignatureDocumentTemplate that this SignatureDocument instance is executing.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the SignatureDocumentTemplateId fo the Rock.Model.SignatureDocumentTemplate that is being executed.
        /// </value>
        public int SignatureDocumentTemplateId { get; set; }

        /// <summary>
        /// The computed SHA1 hash for the SignedDocumentText, SignedClientIP address, SignedClientUserAgent, SignedDateTime, SignedByPersonAliasId, SignatureData, and SignedName.
        /// This hash can be used to prove the authenticity of the unaltered signature document.
        /// This is only calculated once during the pre-save event when the SignedDateTime was originally null/empty but now has a value.
        /// </summary>
        /// <value>
        /// A System.String representing the signature data.
        /// </value>
        public string SignatureVerificationHash { get; set; }

        /// <summary>
        /// The email address that was used to send the completion receipt.
        /// </summary>
        /// <value>
        /// A System.String representing the signed by email address.
        /// </value>
        public string SignedByEmail { get; set; }

        /// <summary>
        /// Gets or sets the signed by person alias identifier.
        /// </summary>
        /// <value>
        /// The signed by person alias identifier.
        /// </value>
        public int? SignedByPersonAliasId { get; set; }

        /// <summary>
        /// The observed IP address of the client system of the individual who signed the document.
        /// </summary>
        /// <value>
        /// A System.String representing the signed client IP address.
        /// </value>
        public string SignedClientIp { get; set; }

        /// <summary>
        /// The observed 'user agent' of the client system of the individual who signed the document.
        /// </summary>
        /// <value>
        /// A System.String representing the signed client user agent.
        /// </value>
        public string SignedClientUserAgent { get; set; }

        /// <summary>
        /// The date and time the document was signed.
        /// </summary>
        /// <value>
        /// The signed date and time.
        /// </value>
        public DateTime? SignedDateTime { get; set; }

        /// <summary>
        /// The resulting text/document using the Lava template from the Rock.Model.SignatureDocumentTemplate at the time the document was signed.
        /// Does not include the signature data. It would be what they saw just prior to signing.
        /// </summary>
        /// <value>
        /// The signed document text.
        /// </value>
        public string SignedDocumentText { get; set; }

        /// <summary>
        /// The name of the individual who signed the document.
        /// </summary>
        /// <value>
        /// The signed name.
        /// </value>
        public string SignedName { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created by person alias identifier.
        /// </summary>
        /// <value>
        /// The created by person alias identifier.
        /// </value>
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the modified by person alias identifier.
        /// </summary>
        /// <value>
        /// The modified by person alias identifier.
        /// </value>
        public int? ModifiedByPersonAliasId { get; set; }

    }
}
