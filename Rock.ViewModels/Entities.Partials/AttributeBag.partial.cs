﻿// <copyright>
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
using System.Collections.Generic;

namespace Rock.ViewModels.Entities
{
    /// <summary>
    /// AttributeViewModel
    /// </summary>
    public partial class AttributeBag
    {
        /// <summary>
        /// Gets or sets the category Guids.
        /// </summary>
        /// <value>
        /// The category Guids.
        /// </value>
        public List<Guid> CategoryGuids { get; set; }

        /// <summary>
        /// Gets or sets the field type unique identifier.
        /// </summary>
        /// <value>
        /// The field type unique identifier.
        /// </value>
        public Guid FieldTypeGuid { get; set; }

        /// <summary>
        /// Gets the configuration values that define the behavior of the attribute.
        /// </summary>
        /// <value>The configuration values.</value>
        public Dictionary<string, string> ConfigurationValues { get; set; }

        /// <summary>
        /// Gets the qualifier values.
        /// </summary>
        /// <value>
        /// The qualifier values.
        /// </value>
        public Dictionary<string, string> QualifierValues { get; set; }
    }
}
