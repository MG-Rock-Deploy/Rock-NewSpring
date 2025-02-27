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
    /// DocumentType View Model
    /// </summary>
    public partial class DocumentTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the id of the Rock.Model.BinaryFileType that this document type belongs to.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the Rock.Model.BinaryFileType.
        /// </value>
        public int BinaryFileTypeId { get; set; }

        /// <summary>
        /// Gets or sets the default document name template.
        /// </summary>
        /// <value>
        /// The default document name template.
        /// </value>
        public string DefaultDocumentNameTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Rock.Model.EntityType that this DocumentType is used for.  A DocumentType can only be associated with a single Rock.Model.EntityType and will 
        /// only contain notes for entities of this type. This property is required.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the Id of the Rock.Model.EntityType
        /// </value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the qualifier column/property on the Rock.Model.EntityType that this Docuement Type applies to. If this is not 
        /// provided, the document type can be used on all entities of the provided Rock.Model.EntityType.
        /// </summary>
        /// <value>
        /// A System.String representing the name of the qualifier column that this DocumentType applies to.
        /// </value>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value in the qualifier column that this document type applies to.  For instance this note type and related notes will only be applicable to entity 
        /// if the value in the EntityTypeQualiferColumn matches this value. This property should not be populated without also populating the EntityTypeQualifierColumn property.
        /// </summary>
        /// <value>
        /// Entity Type Qualifier Value.
        /// </value>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the CSS class that is used for a vector/CSS icon.
        /// </summary>
        /// <value>
        /// A System.String representing the CSS class that is used for a vector/CSS based icon.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the IsImage flag for the Rock.Model.DocumentType.
        /// </summary>
        /// <value>
        /// A System.Boolean for the IsImage flag.
        /// </value>
        public bool IsImage { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this DocumentType is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A System.Boolean value that is true if this is part of the core system/framework; otherwise false.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the maximum documents per entity.  This would limit the documents of that type per entity. A blank value means no limit.
        /// </summary>
        /// <value>
        /// A System.Int32 that represents the maximum documents per entity.
        /// </value>
        public int? MaxDocumentsPerEntity { get; set; }

        /// <summary>
        /// Gets or sets the given Name of the DocumentType.
        /// </summary>
        /// <value>
        /// A System.String representing the given Name of the BinaryFileType. 
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order of this DocumentType.  The lower the number the higher the display priority.  This property is required.
        /// </summary>
        /// <value>
        /// A System.Int32 that represents the display order of this DocumentType.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the type is user selectable.
        /// </summary>
        /// <value>
        ///   true if [user selectable]; otherwise, false.
        /// </value>
        public bool UserSelectable { get; set; }

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
