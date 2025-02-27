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

/** WorkflowFormBuilderTemplate View Model */
export type WorkflowFormBuilderTemplateBag = {
    /** Gets or sets a value indicating whether a new person (and spouse) can be added */
    allowPersonEntry: boolean;

    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;

    /** Gets or sets the completion settings json. */
    completionSettingsJson?: string | null;

    /** Gets or sets the confirmation email settings json. */
    confirmationEmailSettingsJson?: string | null;

    /** Gets or sets the created by person alias identifier. */
    createdByPersonAliasId?: number | null;

    /** Gets or sets the created date time. */
    createdDateTime?: string | null;

    /** Gets or sets the description or summary about this WorkflowFormBuilderTemplate. */
    description?: string | null;

    /** Gets or sets the footer. */
    formFooter?: string | null;

    /** Gets or sets the form header. */
    formHeader?: string | null;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;

    /** Gets or sets a value indicating whether this instance is active. */
    isActive: boolean;

    /** Gets or sets a value indicating whether [is login required]. */
    isLoginRequired: boolean;

    /** Gets or sets the modified by person alias identifier. */
    modifiedByPersonAliasId?: number | null;

    /** Gets or sets the modified date time. */
    modifiedDateTime?: string | null;

    /** Gets or sets the friendly Name of this WorkflowFormBuilderTemplate. This property is required. */
    name?: string | null;

    /** Gets or sets the person entry settings json. */
    personEntrySettingsJson?: string | null;
};
