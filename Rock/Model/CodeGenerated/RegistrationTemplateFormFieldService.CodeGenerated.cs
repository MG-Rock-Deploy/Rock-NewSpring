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

using System;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.ViewModels;
using Rock.ViewModels.Entities;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// RegistrationTemplateFormField Service class
    /// </summary>
    public partial class RegistrationTemplateFormFieldService : Service<RegistrationTemplateFormField>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationTemplateFormFieldService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public RegistrationTemplateFormFieldService(RockContext context) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( RegistrationTemplateFormField item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// RegistrationTemplateFormField View Model Helper
    /// </summary>
    [DefaultViewModelHelper( typeof( RegistrationTemplateFormField ) )]
    public partial class RegistrationTemplateFormFieldViewModelHelper : ViewModelHelper<RegistrationTemplateFormField, RegistrationTemplateFormFieldBag>
    {
        /// <summary>
        /// Converts the model to a view model.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public override RegistrationTemplateFormFieldBag CreateViewModel( RegistrationTemplateFormField model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = new RegistrationTemplateFormFieldBag
            {
                IdKey = model.IdKey,
                AttributeId = model.AttributeId,
                FieldSource = ( int ) model.FieldSource,
                FieldVisibilityRulesJSON = model.FieldVisibilityRulesJSON,
                IsGridField = model.IsGridField,
                IsInternal = model.IsInternal,
                IsRequired = model.IsRequired,
                IsSharedValue = model.IsSharedValue,
                Order = model.Order,
                PersonFieldType = ( int ) model.PersonFieldType,
                PostText = model.PostText,
                PreText = model.PreText,
                RegistrationTemplateFormId = model.RegistrationTemplateFormId,
                ShowCurrentValue = model.ShowCurrentValue,
                ShowOnWaitlist = model.ShowOnWaitlist,
                CreatedDateTime = model.CreatedDateTime,
                ModifiedDateTime = model.ModifiedDateTime,
                CreatedByPersonAliasId = model.CreatedByPersonAliasId,
                ModifiedByPersonAliasId = model.ModifiedByPersonAliasId,
            };

            AddAttributesToViewModel( model, viewModel, currentPerson, loadAttributes );
            ApplyAdditionalPropertiesAndSecurityToViewModel( model, viewModel, currentPerson, loadAttributes );
            return viewModel;
        }
    }


    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class RegistrationTemplateFormFieldExtensionMethods
    {
        /// <summary>
        /// Clones this RegistrationTemplateFormField object to a new RegistrationTemplateFormField object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static RegistrationTemplateFormField Clone( this RegistrationTemplateFormField source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as RegistrationTemplateFormField;
            }
            else
            {
                var target = new RegistrationTemplateFormField();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this RegistrationTemplateFormField object to a new RegistrationTemplateFormField object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static RegistrationTemplateFormField CloneWithoutIdentity( this RegistrationTemplateFormField source )
        {
            var target = new RegistrationTemplateFormField();
            target.CopyPropertiesFrom( source );

            target.Id = 0;
            target.Guid = Guid.NewGuid();
            target.ForeignKey = null;
            target.ForeignId = null;
            target.ForeignGuid = null;
            target.CreatedByPersonAliasId = null;
            target.CreatedDateTime = RockDateTime.Now;
            target.ModifiedByPersonAliasId = null;
            target.ModifiedDateTime = RockDateTime.Now;

            return target;
        }

        /// <summary>
        /// Copies the properties from another RegistrationTemplateFormField object to this RegistrationTemplateFormField object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this RegistrationTemplateFormField target, RegistrationTemplateFormField source )
        {
            target.Id = source.Id;
            target.AttributeId = source.AttributeId;
            target.FieldSource = source.FieldSource;
            target.FieldVisibilityRulesJSON = source.FieldVisibilityRulesJSON;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.IsGridField = source.IsGridField;
            target.IsInternal = source.IsInternal;
            target.IsRequired = source.IsRequired;
            target.IsSharedValue = source.IsSharedValue;
            target.Order = source.Order;
            target.PersonFieldType = source.PersonFieldType;
            target.PostText = source.PostText;
            target.PreText = source.PreText;
            target.RegistrationTemplateFormId = source.RegistrationTemplateFormId;
            target.ShowCurrentValue = source.ShowCurrentValue;
            target.ShowOnWaitlist = source.ShowOnWaitlist;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }

        /// <summary>
        /// Creates a view model from this entity
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson" >The currentPerson.</param>
        /// <param name="loadAttributes" >Load attributes?</param>
        public static RegistrationTemplateFormFieldBag ToViewModel( this RegistrationTemplateFormField model, Person currentPerson = null, bool loadAttributes = false )
        {
            var helper = new RegistrationTemplateFormFieldViewModelHelper();
            var viewModel = helper.CreateViewModel( model, currentPerson, loadAttributes );
            return viewModel;
        }

    }

}
