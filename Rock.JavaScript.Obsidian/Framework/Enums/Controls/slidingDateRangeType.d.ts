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

/** Also defined in Rock.Web.UI.Controls.SlidingDateRangePicker, so if changed, please update there as well */
export const enum SlidingDateRangeType {
    /** All */
    All = -1,

    /** The last X days,weeks,months, etc (inclusive of current day,week,month,...) but cuts off so it doesn't include future dates */
    Last = 0,

    /** The current day,week,month,year */
    Current = 1,

    /** The date range */
    DateRange = 2,

    /** The previous X days,weeks,months, etc (excludes current day,week,month,...) */
    Previous = 4,

    /** The next X days,weeks,months, etc (inclusive of current day,week,month,...), but cuts off so it doesn't include past dates */
    Next = 8,

    /** The upcoming X days,weeks,months, etc (excludes current day,week,month,...) */
    Upcoming = 16
}
