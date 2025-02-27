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

/** Authorization for a special group of users not defined by a specific role or person */
export const enum SpecialRole {
    /** No special role */
    None = 0,

    /** Authorize all users */
    AllUsers = 1,

    /** Authorize all authenticated users */
    AllAuthenticatedUsers = 2,

    /** Authorize all un-authenticated users */
    AllUnAuthenticatedUsers = 3
}
