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

/** Location View Model */
export type LocationBag = {
    /** Gets or sets the Local Assessor's parcel identification value that is linked to the location. */
    assessorParcelId?: string | null;

    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;

    /** Gets or sets the barcode. */
    barcode?: string | null;

    /** Gets or sets the city component of the Location's Street/Mailing Address. */
    city?: string | null;

    /** Gets or sets the country component of the Location's Street/Mailing Address.  */
    country?: string | null;

    /** Gets or sets the county. */
    county?: string | null;

    /** Gets or sets the created by person alias identifier. */
    createdByPersonAliasId?: number | null;

    /** Gets or sets the created date time. */
    createdDateTime?: string | null;

    /** Gets or sets threshold that will prevent checkin (no option to override) */
    firmRoomThreshold?: number | null;

    /** Gets and sets the date and time that an attempt was made to geocode the Location's address. */
    geocodeAttemptedDateTime?: string | null;

    /** Gets or sets the result code returned by geocoding service during the last geocode attempt. */
    geocodeAttemptedResult?: string | null;

    /** Gets or sets the component name of the Geocoding service that attempted the most recent address Geocode attempt. */
    geocodeAttemptedServiceType?: string | null;

    /** Gets or sets date and time that this Location's  address has been successfully geocoded.  */
    geocodedDateTime?: string | null;

    /**
     * Gets or sets the geographic parameter around the a Location's GeoPoint. This can also be used to define a large area
     * like a neighborhood.  
     */
    geoFence?: unknown;

    /** Gets or sets the GeoPoint (GeoLocation) for the location */
    geoPoint?: unknown;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;

    /** Gets or sets the image identifier. */
    imageId?: number | null;

    /** Gets or sets a value indicating whether this instance is active. */
    isActive: boolean;

    /** Gets or sets flag indicating if GeoPoint is locked (shouldn't be geocoded again) */
    isGeoPointLocked?: boolean | null;

    /**
     * Gets or sets the Id of the LocationType Rock.Model.DefinedValue that is used to identify the type of Rock.Model.Location
     * that this is. Examples: Campus, Building, Room, etc
     */
    locationTypeValueId?: number | null;

    /** Gets or sets the modified by person alias identifier. */
    modifiedByPersonAliasId?: number | null;

    /** Gets or sets the modified date time. */
    modifiedDateTime?: string | null;

    /** Gets or sets the Location's Name. */
    name?: string | null;

    /** Gets or sets the if the location's parent Location.  */
    parentLocationId?: number | null;

    /** Gets or sets the Zip/Postal Code component of the Location's Street/Mailing Address. */
    postalCode?: string | null;

    /** Gets or sets the Rock.Model.Device Id of the printer (if any) associated with the location. */
    printerDeviceId?: number | null;

    /** Gets or sets a threshold that will prevent checkin unless a manager overrides */
    softRoomThreshold?: number | null;

    /** Gets or sets the date and time of the last address standardization attempt. */
    standardizeAttemptedDateTime?: string | null;

    /** Gets or sets the result code returned from the address standardization service. */
    standardizeAttemptedResult?: string | null;

    /** Gets or set the component name of the service that attempted the most recent address standardization attempt. */
    standardizeAttemptedServiceType?: string | null;

    /** Gets or sets the date and time that the Location's address was successfully standardized. */
    standardizedDateTime?: string | null;

    /** Gets or sets the State component of the Location's Street/Mailing Address. */
    state?: string | null;

    /** Gets or sets the first line of the Location's Street/Mailing Address. */
    street1?: string | null;

    /** Gets or sets the second line of the Location's Street/Mailing Address.  */
    street2?: string | null;
};
