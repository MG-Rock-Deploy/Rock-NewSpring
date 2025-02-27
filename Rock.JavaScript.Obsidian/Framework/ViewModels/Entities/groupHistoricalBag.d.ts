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

/** GroupHistorical View Model */
export type GroupHistoricalBag = {
    /** Gets or sets the PersonAliasId that archived (soft deleted) this group at this point in history */
    archivedByPersonAliasId?: number | null;

    /** Gets or sets the archived date time value of this group at this point in history */
    archivedDateTime?: string | null;

    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;

    /** Gets or sets the Rock.Model.Campus identifier for this group at this point in history */
    campusId?: number | null;

    /** Gets or sets the created by person alias identifier. */
    createdByPersonAliasId?: number | null;

    /** Gets or sets the created date time. */
    createdDateTime?: string | null;

    /**
     * Gets or sets a value indicating whether [current row indicator].
     * This will be True if this represents the same values as the current tracked record for this
     */
    currentRowIndicator: boolean;

    /** Gets or sets the description for this group at this point in history */
    description?: string | null;

    /**
     * Gets or sets the effective date.
     * This is the starting date that the tracked record had the values reflected in this record
     */
    effectiveDateTime?: string | null;

    /**
     * Gets or sets the expire date time
     * This is the last date that the tracked record had the values reflected in this record
     * For example, if a tracked record's Name property changed on '2016-07-14', the ExpireDate of the previously current record will be '2016-07-13', and the EffectiveDate of the current record will be '2016-07-14'
     * If this is most current record, the ExpireDate will be '9999-01-01'
     */
    expireDateTime?: string | null;

    /** Gets or sets the group id of the group for this group historical record */
    groupId: number;

    /** Gets or sets the name of the group at this point in history */
    groupName?: string | null;

    /** Gets or sets the group type identifier. Normally, a GroupTypeId can't be changed, but just in case, this will be the group type at this point in history */
    groupTypeId: number;

    /** Gets or sets the name of the Rock.Model.GroupType at this point in history */
    groupTypeName?: string | null;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;

    /** Gets or sets the InactiveDateTime value of the group at this point in history */
    inactiveDateTime?: string | null;

    /** Gets or sets a value indicating whether this group had IsActive==True at this point in history */
    isActive: boolean;

    /** Gets or sets a value indicating whether this group was archived at this point in history */
    isArchived: boolean;

    /** Gets or sets the modified by person alias identifier. */
    modifiedByPersonAliasId?: number | null;

    /** Gets or sets the modified date time. */
    modifiedDateTime?: string | null;

    /** Gets or sets the parent Rock.Model.Group identifier at this point in history */
    parentGroupId?: number | null;

    /**
     * If this group's group type supports a schedule for a group, this is the schedule id for that group at this point in history
     * NOTE: If this Group has Schedules at it's Locations, those will be in GroupLocationHistorical.GroupLocationHistoricalSchedules
     */
    scheduleId?: number | null;

    /** Gets or sets the Schedule's ModifiedDateTime. This is used internally to detect if the group's schedule has changed */
    scheduleModifiedDateTime?: string | null;

    /**
     * If this group's group type supports a schedule for a group, this is the schedule text (Schedule.ToString()) for that group at this point in history
     * NOTE: If this Group has Schedules at it's Locations, those will be in GroupLocationHistorical.GroupLocationHistoricalSchedules
     */
    scheduleName?: string | null;

    /** Gets or sets the Group Status Id.  DefinedType depends on this group's Rock.Model.GroupType.GroupStatusDefinedType */
    statusValueId?: number | null;
};
