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

import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

/** Layout View Model */
export type LayoutBag = {
    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;

    /** Gets or sets the created by person alias identifier. */
    createdByPersonAliasId?: number | null;

    /** Gets or sets the created date time. */
    createdDateTime?: string | null;

    /** Gets or sets the user defined description of the Layout. */
    description?: string | null;

    /**
     * Gets or sets the file name portion of the associated .Net ASCX UserControl that provides the HTML Markup and code for this Layout.
     * Value should not include the extension.  And the path is relative to the theme folder.
     */
    fileName?: string | null;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;

    /** Gets or sets a flag indicating if this Layout was created by and is a part of the Rock core system/framework. This property is required. */
    isSystem: boolean;

    /** Gets or sets the layout mobile phone. */
    layoutMobilePhone?: string | null;

    /** Gets or sets the layout mobile tablet. */
    layoutMobileTablet?: string | null;

    /** Gets or sets the modified by person alias identifier. */
    modifiedByPersonAliasId?: number | null;

    /** Gets or sets the modified date time. */
    modifiedDateTime?: string | null;

    /** Gets or sets the logical name of the Layout. */
    name?: string | null;

    /** Gets or sets the Id of the Rock.Model.Site that this layout is associated with. */
    siteId: number;
};
