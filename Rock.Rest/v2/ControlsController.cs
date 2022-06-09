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
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using Rock.Badge;
using Rock.ClientService.Core.Category;
using Rock.ClientService.Core.Category.Options;
using Rock.Communication;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.CRM;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Rest.v2
{
    /// <summary>
    /// Provides API endpoints for the Controls controller.
    /// </summary>
    [RoutePrefix( "api/v2/Controls" )]
    [Rock.SystemGuid.RestControllerGuid( "815B51F0-B552-47FD-8915-C653EEDD5B67")]
    public class ControlsController : ApiControllerBase
    {

        #region Achievement Type Picker

        /// <summary>
        /// Gets the achievement types that can be displayed in the achievement type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AchievementTypePickerGetAchievementTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "F98E3033-C652-4031-94B3-E7C44ECA51AA" )]
        public IHttpActionResult AchievementTypePickerGetEntityTypes( [FromBody] AchievementTypePickerGetAchievementTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = AchievementTypeCache.All( rockContext )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.Name,
                        Category = t.Category?.Name
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Assessment Type Picker

        /// <summary>
        /// Gets the achievement types that can be displayed in the achievement type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AssessmentTypePickerGetAssessmentTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "B47DCE1B-89D7-4DD5-88A7-B3C393D49A7C  " )]
        public IHttpActionResult AssessmentTypePickerGetEntityTypes( [FromBody] AssessmentTypePickerGetAssessmentTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = AssessmentTypeCache.All( rockContext )
                    .Where( at => options.isInactiveIncluded == true || at.IsActive )
                    .OrderBy( at => at.Title )
                    .ThenBy( at => at.Id )
                    .Select( at => new ListItemBag
                    {
                        Value = at.Guid.ToString(),
                        Text = at.Title
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Asset Storage Provider Picker

        /// <summary>
        /// Gets the asset storage providers that can be displayed in the asset storage provider picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "AssetStorageProviderPickerGetAssetStorageProviders" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "665EDE0C-1FEA-4421-B355-4D4F72B7E26E" )]
        public IHttpActionResult AssetStorageProviderPickerGetAssetStorageProviders( [FromBody] AssetStorageProviderPickerGetAssetStorageProvidersOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = new AssetStorageProviderService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g => g.EntityTypeId.HasValue && g.IsActive )
                    .OrderBy( g => g.Name )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.Name
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Audit Detail

        /// <summary>
        /// Gets the audit details about the entity.
        /// </summary>
        /// <param name="options">The options that describe which entity to be audited.</param>
        /// <returns>A <see cref="EntityAuditBag"/> that contains the requested information.</returns>
        [HttpPost]
        [Authenticate]
        [System.Web.Http.Route( "AuditDetailGetAuditDetails" )]
        [Rock.SystemGuid.RestActionGuid( "714d83c9-96e4-49d7-81af-2eed7d5ccd56" )]
        public IHttpActionResult AuditDetailGetAuditDetails( [FromBody] AuditDetailGetAuditDetailsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                // Get the entity type identifier to use to lookup the entity.
                var entityType = EntityTypeCache.Get( options.EntityTypeGuid )?.GetEntityType();

                if ( entityType == null )
                {
                    return NotFound();
                }

                var entity = Reflection.GetIEntityForEntityType( entityType, options.EntityKey, rockContext ) as IModel;

                if ( entity == null )
                {
                    return NotFound();
                }

                // If the entity can be secured, ensure the person has access to it.
                if ( entity is ISecured securedEntity )
                {
                    var isAuthorized = securedEntity.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( entity, Authorization.VIEW ) == true;

                    if ( !isAuthorized )
                    {
                        return Unauthorized();
                    }
                }

                return Ok( entity.GetEntityAuditBag() );
            }
        }

        #endregion

        #region Badge Component Picker

        /// <summary>
        /// Gets the badge components that can be displayed in the badge component picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of list items that represent the badge components.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BadgeComponentPickerGetBadgeComponents" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "ABDFC10F-BCCC-4AF1-8DB3-88A26862485D" )]
        public IHttpActionResult BadgeComponentPickerGetEntityTypes( [FromBody] BadgeComponentPickerGetBadgeComponentsOptionsBag options )
        {
            var componentsList = GetComponentListItems( "Rock.Badge.BadgeContainer, Rock", ( Component component ) =>
            {
                var badgeComponent = component as BadgeComponent;
                var entityType = EntityTypeCache.Get( options.EntityTypeGuid.GetValueOrDefault() )?.Name;

                return badgeComponent != null && badgeComponent.DoesApplyToEntityType( entityType );
            } );

            return Ok( componentsList );
        }

        #endregion

        #region Badge List

        /// <summary>
        /// Get the rendered badge information for a specific entity.
        /// </summary>
        /// <param name="options">The options that describe which badges to render.</param>
        /// <returns>A collection of <see cref="RenderedBadgeBag"/> objects.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BadgeListGetBadges" )]
        [Rock.SystemGuid.RestActionGuid( "34387b98-bf7e-4000-a28a-24ea08605285" )]
        public IHttpActionResult BadgeListGetBadges( [FromBody] BadgeListGetBadgesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeCache = EntityTypeCache.Get( options.EntityTypeGuid, rockContext );
                var entityType = entityTypeCache?.GetEntityType();
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                // Verify that we found the entity type.
                if ( entityType == null )
                {
                    return BadRequest( "Unknown entity type." );
                }

                // Load the entity and verify we got one.
                var entity = Rock.Reflection.GetIEntityForEntityType( entityType, options.EntityKey );

                if ( entity == null )
                {
                    return NotFound();
                }

                // If the entity can be secured, ensure the person has access to it.
                if ( entity is ISecured securedEntity )
                {
                    var isAuthorized = securedEntity.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( entity, Authorization.VIEW ) == true;

                    if ( !isAuthorized )
                    {
                        return Unauthorized();
                    }
                }

                List<BadgeCache> badges;

                // Load the list of badges that were requested or all badges
                // if no specific badges were requested.
                if ( options.BadgeTypeGuids != null && options.BadgeTypeGuids.Any() )
                {
                    badges = options.BadgeTypeGuids
                        .Select( g => BadgeCache.Get( g ) )
                        .Where( b => b != null )
                        .ToList();
                }
                else
                {
                    badges = BadgeCache.All()
                        .Where( b => !b.EntityTypeId.HasValue || b.EntityTypeId.Value == entityTypeCache.Id )
                        .ToList();
                }

                // Filter out any badges that don't apply to the entity or are not
                // authorized by the person to be viewed.
                badges = badges.Where( b => b.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson )
                        || grant?.IsAccessGranted( b, Authorization.VIEW ) == true )
                    .ToList();

                // Render all the badges and then filter out any that are empty.
                var badgeResults = badges.Select( b => b.RenderBadge( entity ) )
                    .Where( b => b.Html.IsNotNullOrWhiteSpace() || b.JavaScript.IsNotNullOrWhiteSpace() )
                    .ToList();

                return Ok( badgeResults );
            }
        }

        #endregion

        #region Binary File Picker

        /// <summary>
        /// Gets the asset storage providers that can be displayed in the asset storage provider picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BinaryFilePickerGetBinaryFiles" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "9E5F190E-91FD-4E50-9F00-8B4F9DBD874C" )]
        public IHttpActionResult BinaryFilePickerGetBinaryFiles( [FromBody] BinaryFilePickerGetBinaryFilesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = new BinaryFileService( new RockContext() )
                    .Queryable()
                    .Where( f => f.BinaryFileType.Guid == options.BinaryFileTypeGuid && !f.IsTemporary )
                    .OrderBy( f => f.FileName )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.FileName
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Binary File Type Picker

        /// <summary>
        /// Gets the binary file types that can be displayed in the binary file type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "BinaryFileTypePickerGetBinaryFileTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "C93E5A06-82DE-4475-88B8-B173C03BFB50" )]
        public IHttpActionResult BinaryFileTypePickerGetBinaryFileTypes( [FromBody] BinaryFileTypePickerGetBinaryFileTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = new BinaryFileTypeService( rockContext )
                    .Queryable()
                    .OrderBy( f => f.Name )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.Name
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Category Picker

        private static readonly Regex QualifierValueLookupRegex = new Regex( "^{EL:((?:[a-f\\d]{8})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{12})):((?:[a-f\\d]{8})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{4})-(?:[a-f\\d]{12}))}$", RegexOptions.IgnoreCase );

        /// <summary>
        /// Gets the child items that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "CategoryPickerChildTreeItems" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "A1D07211-6C50-463B-98ED-1622DC4D73DD" )]
        public IHttpActionResult CategoryPickerChildTreeItems( [FromBody] CategoryPickerChildTreeItemsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = new CategoryClientService( rockContext, GetPerson( rockContext ) );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                var items = clientService.GetCategorizedTreeItems( new CategoryItemTreeOptions
                {
                    ParentGuid = options.ParentGuid,
                    GetCategorizedItems = options.GetCategorizedItems,
                    EntityTypeGuid = options.EntityTypeGuid,
                    EntityTypeQualifierColumn = options.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = GetQualifierValueLookupResult( options.EntityTypeQualifierValue, rockContext ),
                    IncludeUnnamedEntityItems = options.IncludeUnnamedEntityItems,
                    IncludeCategoriesWithoutChildren = options.IncludeCategoriesWithoutChildren,
                    DefaultIconCssClass = options.DefaultIconCssClass,
                    IncludeInactiveItems = options.IncludeInactiveItems,
                    LazyLoad = options.LazyLoad,
                    SecurityGrant = grant
                } );

                return Ok( items );
            }
        }

        /// <summary>
        /// Checks if the qualifier value is a lookup and if so translate it to the
        /// identifier from the unique identifier. Otherwise returns the original
        /// value.
        /// </summary>
        /// <remarks>
        /// At some point this needs to be moved into a ClientService layer, but
        /// I'm not sure where since it isn't related to any one service.
        /// </remarks>
        /// <param name="value">The value to be translated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The qualifier value to use.</returns>
        private static string GetQualifierValueLookupResult( string value, RockContext rockContext )
        {
            if ( value == null )
            {
                return null;
            }

            var m = QualifierValueLookupRegex.Match( value );

            if ( m.Success )
            {
                int? id = null;

                if ( Guid.TryParse( m.Groups[1].Value, out var g1 ) && Guid.TryParse( m.Groups[2].Value, out var g2 ) )
                {
                    id = Rock.Reflection.GetEntityIdForEntityType( g1, g2, rockContext );
                }

                return id?.ToString() ?? "0";
            }
            else
            {
                return value;
            }
        }

        #endregion

        #region Defined Value Picker

        /// <summary>
        /// Gets the child items that match the options sent in the request body.
        /// This endpoint returns items formatted for use in a tree view control.
        /// </summary>
        /// <param name="options">The options that describe which defined values to load.</param>
        /// <returns>A collection of view models that represent the defined values.</returns>
        [HttpPost]
        [System.Web.Http.Route( "DefinedValuePickerGetDefinedValues" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "1E4A1812-8A2C-4266-8F39-3004C1DEBC9F" )]
        public IHttpActionResult DefinedValuePickerGetDefinedValues( DefinedValuePickerGetDefinedValuesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedType = DefinedTypeCache.Get( options.DefinedTypeGuid );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( definedType == null || !definedType.IsAuthorized( Rock.Security.Authorization.VIEW, RockRequestContext.CurrentPerson ) )
                {
                    return NotFound();
                }

                var definedValues = definedType.DefinedValues
                    .Where( v => ( v.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( v, Authorization.VIEW ) == true )
                        && ( options.IncludeInactive || v.IsActive ) )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList();

                return Ok( definedValues );
            }
        }

        #endregion

        #region Entity Tag List

        /// <summary>
        /// Gets the tags that are currently specified for the given entity.
        /// </summary>
        /// <param name="options">The options that describe which tags to load.</param>
        /// <returns>A collection of <see cref="EntityTagListTagBag"/> that represent the tags.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListGetEntityTags" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "7542d4b3-17dc-4640-acbd-f02784130401" )]
        public IHttpActionResult EntityTagListGetEntityTags( [FromBody] EntityTagListGetEntityTagsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var taggedItemService = new TaggedItemService( rockContext );
                var items = taggedItemService.Get( entityTypeId.Value, string.Empty, string.Empty, RockRequestContext.CurrentPerson?.Id, entityGuid.Value, options.CategoryGuid, false )
                    .Include( ti => ti.Tag.Category )
                    .Select( ti => ti.Tag )
                    .ToList()
                    .Where( t => t.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( t, Authorization.VIEW ) == true )
                    .Select( t => GetTagBagFromTag( t ) )
                    .ToList();

                return Ok( items );
            }
        }

        /// <summary>
        /// Gets the tags that are available for the given entity.
        /// </summary>
        /// <param name="options">The options that describe which tags to load.</param>
        /// <returns>A collection of list item bags that represent the tags.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListGetAvailableTags" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "91890d39-6e3e-4623-aad7-f32e686c784e" )]
        public IHttpActionResult EntityTagListGetAvailableTags( [FromBody] EntityTagListGetAvailableTagsOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var tagService = new TagService( rockContext );
                var items = tagService.Get( entityTypeId.Value, string.Empty, string.Empty, RockRequestContext.CurrentPerson?.Id, options.CategoryGuid, false )
                    .Where( t => t.Name.StartsWith( options.Name )
                        && !t.TaggedItems.Any( i => i.EntityGuid == entityGuid ) )
                    .ToList()
                    .Where( t => t.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) || grant?.IsAccessGranted( t, Authorization.VIEW ) == true )
                    .Select( t => GetTagBagFromTag( t ) )
                    .ToList();

                return Ok( items );
            }
        }

        /// <summary>
        /// Create a new personal tag for the EntityTagList control.
        /// </summary>
        /// <param name="options">The options that describe the tag to be created.</param>
        /// <returns>An instance of <see cref="EntityTagListTagBag"/> that represents the tag.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListCreatePersonalTag" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "8ccb7b8d-5d5c-4aa6-a12c-ed062c7afa05" )]
        public IHttpActionResult EntityTagListCreatePersonalTag( [FromBody] EntityTagListCreatePersonalTagOptionsBag options )
        {
            if ( RockRequestContext.CurrentPerson == null )
            {
                return Unauthorized();
            }

            using ( var rockContext = new RockContext() )
            {
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );

                if ( !entityTypeId.HasValue )
                {
                    return NotFound();
                }

                var tagService = new TagService( rockContext );
                var tag = tagService.Get( entityTypeId.Value, string.Empty, string.Empty, RockRequestContext.CurrentPerson?.Id, options.Name, options.CategoryGuid, true );

                // If the personal tag already exists, use a 409 to indicate
                // it already exists and return the existing tag.
                if ( tag != null && tag.OwnerPersonAliasId.HasValue )
                {
                    // If the personal tag isn't active, make it active.
                    if ( !tag.IsActive )
                    {
                        tag.IsActive = true;
                        System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                        rockContext.SaveChanges();
                    }

                    return Content( System.Net.HttpStatusCode.Conflict, GetTagBagFromTag( tag ) );
                }

                // At this point tag either doesn't exist or was an organization
                // tag so we need to create a new personal tag.
                tag = new Tag
                {
                    EntityTypeId = entityTypeId,
                    OwnerPersonAliasId = new PersonAliasService( rockContext ).GetPrimaryAliasId( RockRequestContext.CurrentPerson.Id ),
                    Name = options.Name
                };

                if ( options.CategoryGuid.HasValue )
                {
                    var category = new CategoryService( rockContext ).Get( options.CategoryGuid.Value );

                    if ( category == null || ( !category.IsAuthorized( Authorization.VIEW, RockRequestContext.CurrentPerson ) && !grant?.IsAccessGranted( category, Authorization.VIEW ) != true ) )
                    {
                        return NotFound();
                    }

                    // Set the category as well so we can properly convert to a bag.
                    tag.Category = category;
                    tag.CategoryId = category.Id;
                }

                tagService.Add( tag );

                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                rockContext.SaveChanges();

                return Content( System.Net.HttpStatusCode.Created, GetTagBagFromTag( tag ) );
            }
        }

        /// <summary>
        /// Adds a tag on the given entity.
        /// </summary>
        /// <param name="options">The options that describe the tag and the entity to be tagged.</param>
        /// <returns>An instance of <see cref="EntityTagListTagBag"/> that represents the tag applied to the entity.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListAddEntityTag" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "c9cacc7f-68de-4765-8967-b50ee2949062" )]
        public IHttpActionResult EntityTagListAddEntityTag( [FromBody] EntityTagListAddEntityTagOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var tagService = new TagService( rockContext );
                var tag = tagService.Get( options.TagKey );

                if ( tag == null || ( !tag.IsAuthorized( Authorization.TAG, RockRequestContext.CurrentPerson ) && grant?.IsAccessGranted( tag, Authorization.VIEW ) != true ) )
                {
                    return NotFound();
                }

                // If the entity is not already tagged, then tag it.
                var taggedItem = tag.TaggedItems.FirstOrDefault( i => i.EntityGuid.Equals( entityGuid ) );

                if ( taggedItem == null )
                {
                    taggedItem = new TaggedItem
                    {
                        Tag = tag,
                        EntityTypeId = entityTypeId.Value,
                        EntityGuid = entityGuid.Value
                    };

                    tag.TaggedItems.Add( taggedItem );

                    System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                    rockContext.SaveChanges();
                }

                return Ok( GetTagBagFromTag( tag ) );
            }
        }

        /// <summary>
        /// Removes a tag from the given entity.
        /// </summary>
        /// <param name="options">The options that describe the tag and the entity to be untagged.</param>
        /// <returns>A response code that indicates success or failure.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTagListRemoveEntityTag" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "6a78d538-87db-43fe-9150-4e9a3f276afe" )]
        public IHttpActionResult EntityTagListRemoveEntityTag( [FromBody] EntityTagListRemoveEntityTagOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );
                var entityGuid = Reflection.GetEntityGuidForEntityType( options.EntityTypeGuid, options.EntityKey, false, rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );
                var tagService = new TagService( rockContext );
                var taggedItemService = new TaggedItemService( rockContext );

                if ( !entityTypeId.HasValue || !entityGuid.HasValue )
                {
                    return NotFound();
                }

                var tag = tagService.Get( options.TagKey );

                if ( tag == null || ( !tag.IsAuthorized( Authorization.TAG, RockRequestContext.CurrentPerson ) && grant?.IsAccessGranted( tag, Authorization.VIEW ) != true ) )
                {
                    return NotFound();
                }

                // If the entity is tagged, then untag it.
                var taggedItem = taggedItemService.Queryable()
                    .FirstOrDefault( ti => ti.TagId == tag.Id && ti.EntityGuid == entityGuid.Value );

                if ( taggedItem != null )
                {
                    taggedItemService.Delete( taggedItem );

                    System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                    rockContext.SaveChanges();
                }

                return Ok();
            }
        }

        private static EntityTagListTagBag GetTagBagFromTag( Tag tag )
        {
            return new EntityTagListTagBag
            {
                IdKey = tag.IdKey,
                BackgroundColor = tag.BackgroundColor,
                Category = tag.Category != null
                    ? new ListItemBag
                    {
                        Value = tag.Category.Guid.ToString(),
                        Text = tag.Category.ToString()
                    }
                    : null,
                EntityTypeGuid = tag.EntityTypeId.HasValue ? EntityTypeCache.Get( tag.EntityTypeId.Value ).Guid : Guid.Empty,
                IconCssClass = tag.IconCssClass,
                IsPersonal = tag.OwnerPersonAliasId.HasValue,
                Name = tag.Name
            };
        }

        #endregion

        #region Entity Type Picker

        /// <summary>
        /// Gets the entity types that can be displayed in the entity type picker.
        /// </summary>
        /// <param name="options">The options that describe which items to load.</param>
        /// <returns>A collection of view models that represent the tree items.</returns>
        [HttpPost]
        [System.Web.Http.Route( "EntityTypePickerGetEntityTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "AFDD3D40-5856-478B-A41A-0539127F0631" )]
        public IHttpActionResult EntityTypePickerGetEntityTypes( [FromBody] EntityTypePickerGetEntityTypesOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var items = EntityTypeCache.All( rockContext )
                    .Where( t => t.IsEntity )
                    .OrderByDescending( t => t.IsCommon )
                    .ThenBy( t => t.FriendlyName )
                    .Select( t => new ListItemBag
                    {
                        Value = t.Guid.ToString(),
                        Text = t.FriendlyName,
                        Category = t.IsCommon ? "Common" : "All Entities"
                    } )
                    .ToList();

                return Ok( items );
            }
        }

        #endregion

        #region Field Type Editor

        /// <summary>
        /// Gets the available field types for the current person.
        /// </summary>
        /// <param name="options">The options that provide details about the request.</param>
        /// <returns>A collection <see cref="ListItemBag"/> that represents the field types that are available.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FieldTypeEditorGetAvailableFieldTypes" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "FEDEF3F7-FCB0-4538-9629-177C7D2AE06F" )]
        public IHttpActionResult FieldTypeEditorGetAvailableFieldTypes( [FromBody] FieldTypeEditorGetAvailableFieldTypesOptionsBag options )
        {
            var fieldTypes = FieldTypeCache.All()
                .Where( f => f.Platform.HasFlag( Rock.Utility.RockPlatform.Obsidian ) )
                .ToList();

            var fieldTypeItems = fieldTypes
                .Select( f => new ListItemBag
                {
                    Text = f.Name,
                    Value = f.Guid.ToString()
                } )
                .ToList();

            return Ok( fieldTypeItems );
        }

        /// <summary>
        /// Gets the attribute configuration information provided and returns a new
        /// set of configuration data. This is used by the attribute editor control
        /// when a field type makes a change that requires new data to be retrieved
        /// in order for it to continue editing the attribute.
        /// </summary>
        /// <param name="options">The view model that contains the update request.</param>
        /// <returns>An instance of <see cref="FieldTypeEditorUpdateAttributeConfigurationResultBag"/> that represents the state of the attribute configuration.</returns>
        [HttpPost]
        [System.Web.Http.Route( "FieldTypeEditorUpdateAttributeConfiguration" )]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "AFDF0EC4-5D17-4278-9FA6-3F859F38E3B5" )]
        public IHttpActionResult FieldTypeEditorUpdateAttributeConfiguration( [FromBody] FieldTypeEditorUpdateAttributeConfigurationOptionsBag options )
        {
            var fieldType = Rock.Web.Cache.FieldTypeCache.Get( options.FieldTypeGuid )?.Field;

            if ( fieldType == null )
            {
                return BadRequest( "Unknown field type." );
            }

            // Convert the public configuration options into our private
            // configuration options (values).
            var configurationValues = fieldType.GetPrivateConfigurationValues( options.ConfigurationValues );

            // Convert the default value from the public value into our
            // private internal value.
            var privateDefaultValue = fieldType.GetPrivateEditValue( options.DefaultValue, configurationValues );

            // Get the new configuration properties from the currently selected
            // options.
            var configurationProperties = fieldType.GetPublicEditConfigurationProperties( configurationValues );

            // Get the public configuration options from the internal options (values).
            var publicConfigurationValues = fieldType.GetPublicConfigurationValues( configurationValues, Field.ConfigurationValueUsage.Configure, null );

            return Ok( new FieldTypeEditorUpdateAttributeConfigurationResultBag
            {
                ConfigurationProperties = configurationProperties,
                ConfigurationValues = publicConfigurationValues,
                DefaultValue = fieldType.GetPublicEditValue( privateDefaultValue, configurationValues )
            } );
        }

        #endregion

        #region Following

        /// <summary>
        /// Determines if the entity is currently being followed by the logged in person.
        /// </summary>
        /// <param name="options">The options that describe which entity to be checked.</param>
        /// <returns>A <see cref="FollowingGetFollowingResponseBag"/> that contains the followed state of the entity.</returns>
        [HttpPost]
        [Authenticate]
        [System.Web.Http.Route( "FollowingGetFollowing" )]
        [Rock.SystemGuid.RestActionGuid( "fa1cc136-a994-4870-9507-818ea7a70f01" )]
        public IHttpActionResult FollowingGetFollowing( [FromBody] FollowingGetFollowingOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( RockRequestContext.CurrentPerson == null )
                {
                    return Unauthorized();
                }

                // Get the entity type identifier to use to lookup the entity.
                int? entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );

                if ( !entityTypeId.HasValue )
                {
                    return NotFound();
                }

                int? entityId = null;

                // Special handling for a person record, need to translate it to
                // a person alias record.
                if ( entityTypeId.Value == EntityTypeCache.GetId<Person>() )
                {
                    entityTypeId = EntityTypeCache.GetId<PersonAlias>();
                    entityId = new PersonService( rockContext ).Get( options.EntityKey, true )?.PrimaryAliasId;
                }
                else
                {
                    // Get the entity identifier to use for the following query.
                    entityId = Reflection.GetEntityIdForEntityType( entityTypeId.Value, options.EntityKey, true, rockContext );
                }

                if ( !entityId.HasValue )
                {
                    return NotFound();
                }

                var purposeKey = options.PurposeKey ?? string.Empty;

                // Look for any following objects that match the criteria.
                var followings = new FollowingService( rockContext ).Queryable()
                    .Where( f =>
                        f.EntityTypeId == entityTypeId.Value &&
                        f.EntityId == entityId.Value &&
                        f.PersonAlias.PersonId == RockRequestContext.CurrentPerson.Id &&
                        ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) );

                return Ok( new FollowingGetFollowingResponseBag
                {
                    IsFollowing = followings.Any()
                } );
            }
        }

        /// <summary>
        /// Sets the following state of the entity for the logged in person.
        /// </summary>
        /// <param name="options">The options that describe which entity to be followed or unfollowed.</param>
        /// <returns>An HTTP status code that indicates if the request was successful.</returns>
        [HttpPost]
        [Authenticate]
        [System.Web.Http.Route( "FollowingSetFollowing" )]
        [Rock.SystemGuid.RestActionGuid( "8ca2eafb-e577-4f65-8d96-f42d8d5aae7a" )]
        public IHttpActionResult FollowingSetFollowing( [FromBody] FollowingSetFollowingOptionsBag options )
        {
            using ( var rockContext = new RockContext() )
            {
                var followingService = new FollowingService( rockContext );

                if ( RockRequestContext.CurrentPerson == null )
                {
                    return Unauthorized();
                }

                // Get the entity type identifier to use to lookup the entity.
                int? entityTypeId = EntityTypeCache.GetId( options.EntityTypeGuid );

                if ( !entityTypeId.HasValue )
                {
                    return NotFound();
                }

                int? entityId = null;

                // Special handling for a person record, need to translate it to
                // a person alias record.
                if ( entityTypeId.Value == EntityTypeCache.GetId<Person>() )
                {
                    entityTypeId = EntityTypeCache.GetId<PersonAlias>();
                    entityId = new PersonService( rockContext ).Get( options.EntityKey, true )?.PrimaryAliasId;
                }
                else
                {
                    // Get the entity identifier to use for the following query.
                    entityId = Reflection.GetEntityIdForEntityType( entityTypeId.Value, options.EntityKey, true, rockContext );
                }

                if ( !entityId.HasValue )
                {
                    return NotFound();
                }

                var purposeKey = options.PurposeKey ?? string.Empty;

                // Look for any following objects that match the criteria.
                var followings = followingService.Queryable()
                    .Where( f =>
                        f.EntityTypeId == entityTypeId.Value &&
                        f.EntityId == entityId.Value &&
                        f.PersonAlias.PersonId == RockRequestContext.CurrentPerson.Id &&
                        ( ( f.PurposeKey == null && purposeKey == "" ) || f.PurposeKey == purposeKey ) );

                if ( options.IsFollowing )
                {
                    // Already following, don't need to add a new record.
                    if ( followings.Any() )
                    {
                        return Ok();
                    }

                    var following = new Following
                    {
                        EntityTypeId = entityTypeId.Value,
                        EntityId = entityId.Value,
                        PersonAliasId = RockRequestContext.CurrentPerson.PrimaryAliasId.Value,
                        PurposeKey = purposeKey
                    };

                    followingService.Add( following );

                    if ( !following.IsValid )
                    {
                        return BadRequest( string.Join( ", ", following.ValidationResults.Select( r => r.ErrorMessage ) ) );
                    }
                }
                else
                {
                    foreach ( var following in followings )
                    {
                        // Don't check security here because a person is allowed
                        // to un-follow/delete something they previously followed.
                        followingService.Delete( following );
                    }
                }

                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );

                rockContext.SaveChanges();

                return Ok();
            }
        }

        #endregion

        #region Location Picker

        /// <summary>
        /// Gets the child locations, excluding inactive items.
        /// </summary>
        /// <param name="options">The options that describe which child locations to retrieve.</param>
        /// <returns>A collection of <see cref="TreeItemBag"/> objects that represent the child locations.</returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "LocationPickerGetActiveChildren" )]
        [Rock.SystemGuid.RestActionGuid( "E57312EC-92A7-464C-AA7E-5320DDFAEF3D" )]
        public IHttpActionResult LocationPickerGetActiveChildren( [FromBody] LocationPickerGetActiveChildrenOptionsBag options )
        {
            IQueryable<Location> qry;

            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );
                var grant = SecurityGrant.FromToken( options.SecurityGrantToken );

                if ( options.Guid == Guid.Empty )
                {
                    qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocationId == null );
                    if ( options.RootLocationGuid != Guid.Empty )
                    {
                        qry = qry.Where( a => a.Guid == options.RootLocationGuid );
                    }
                }
                else
                {
                    qry = locationService.Queryable().AsNoTracking().Where( a => a.ParentLocation.Guid == options.Guid );
                }

                // limit to only active locations.
                qry = qry.Where( a => a.IsActive );

                // limit to only Named Locations (don't show home addresses, etc)
                qry = qry.Where( a => a.Name != null && a.Name != string.Empty );

                List<Location> locationList = new List<Location>();
                List<TreeItemBag> locationNameList = new List<TreeItemBag>();

                var person = GetPerson();

                foreach ( var location in qry.OrderBy( l => l.Name ) )
                {
                    if ( location.IsAuthorized( Authorization.VIEW, person ) || grant?.IsAccessGranted( location, Authorization.VIEW ) == true )
                    {
                        locationList.Add( location );
                        var treeViewItem = new TreeItemBag();
                        treeViewItem.Value = location.Guid.ToString();
                        treeViewItem.Text = location.Name;
                        locationNameList.Add( treeViewItem );
                    }
                }

                // try to quickly figure out which items have Children
                List<int> resultIds = locationList.Select( a => a.Id ).ToList();

                var qryHasChildren = locationService.Queryable().AsNoTracking()
                    .Where( l =>
                        l.ParentLocationId.HasValue &&
                        resultIds.Contains( l.ParentLocationId.Value ) &&
                        l.IsActive
                    )
                    .Select( l => l.ParentLocation.Guid )
                    .Distinct()
                    .ToList();

                var qryHasChildrenList = qryHasChildren.ToList();

                foreach ( var item in locationNameList )
                {
                    var locationGuid = item.Value.AsGuid();
                    item.IsFolder = qryHasChildrenList.Any( a => a == locationGuid );
                    item.HasChildren = item.IsFolder;
                }

                return Ok( locationNameList );
            }
        }

        #endregion

        #region Person Picker

        /// <summary>
        /// Searches for people that match the given search options and returns
        /// those matches.
        /// </summary>
        /// <param name="options">The options that describe how the search should be performed.</param>
        /// <returns>A collection of <see cref="Rock.Rest.Controllers.PersonSearchResult"/> objects.</returns>
        [Authenticate]
        [Secured]
        [HttpPost]
        [System.Web.Http.Route( "PersonPickerSearch" )]
        [Rock.SystemGuid.RestActionGuid( "1947578D-B28F-4956-8666-DCC8C0F2B945" )]
        public IQueryable<Rock.Rest.Controllers.PersonSearchResult> PersonPickerSearch( [FromBody] PersonPickerSearchOptionsBag options )
        {
            var rockContext = new RockContext();

            // Chain to the v1 controller.
            return Rock.Rest.Controllers.PeopleController.SearchForPeople( rockContext, options.Name, options.Address, options.Phone, options.Email, options.IncludeDetails, options.IncludeBusinesses, options.IncludeDeceased, false );
        }

        #endregion

        #region Save Financial Account Form

        /// <summary>
        /// Saves the financial account.
        /// </summary>
        /// <param name="options">The options that describe what account should be saved.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "SaveFinancialAccountFormSaveAccount" )]
        [Rock.SystemGuid.RestActionGuid( "544B6302-A9E0-430E-A1C1-7BCBC4A6230C" )]
        public SaveFinancialAccountFormSaveAccountResultBag SaveFinancialAccountFormSaveAccount( [FromBody] SaveFinancialAccountFormSaveAccountOptionsBag options )
        {
            // Validate the arguments
            if ( options?.TransactionCode.IsNullOrWhiteSpace() != false )
            {
                return new SaveFinancialAccountFormSaveAccountResultBag
                {
                    Title = "Sorry",
                    Detail = "The account information cannot be saved as there's not a valid transaction code to reference",
                    IsSuccess = false
                };
            }

            if ( options.SavedAccountName.IsNullOrWhiteSpace() )
            {
                return new SaveFinancialAccountFormSaveAccountResultBag
                {
                    Title = "Missing Account Name",
                    Detail = "Please enter a name to use for this account",
                    IsSuccess = false
                };
            }

            var currentPerson = GetPerson();
            var isAnonymous = currentPerson == null;

            using ( var rockContext = new RockContext() )
            {
                if ( isAnonymous )
                {
                    if ( options.Username.IsNullOrWhiteSpace() || options.Password.IsNullOrWhiteSpace() )
                    {
                        return new SaveFinancialAccountFormSaveAccountResultBag
                        {
                            Title = "Missing Information",
                            Detail = "A username and password are required when saving an account",
                            IsSuccess = false
                        };
                    }

                    var userLoginService = new UserLoginService( rockContext );

                    if ( userLoginService.GetByUserName( options.Username ) != null )
                    {
                        return new SaveFinancialAccountFormSaveAccountResultBag
                        {
                            Title = "Invalid Username",
                            Detail = "The selected Username is already being used. Please select a different Username",
                            IsSuccess = false
                        };
                    }

                    if ( !UserLoginService.IsPasswordValid( options.Password ) )
                    {
                        return new SaveFinancialAccountFormSaveAccountResultBag
                        {
                            Title = "Invalid Password",
                            Detail = UserLoginService.FriendlyPasswordRules(),
                            IsSuccess = false
                        };
                    }
                }

                // Load the gateway from the database
                var financialGatewayService = new FinancialGatewayService( rockContext );
                var financialGateway = financialGatewayService.Get( options.GatewayGuid );
                var gateway = financialGateway?.GetGatewayComponent();

                if ( gateway is null )
                {
                    return new SaveFinancialAccountFormSaveAccountResultBag
                    {
                        Title = "Invalid Gateway",
                        Detail = "Sorry, the financial gateway information is not valid.",
                        IsSuccess = false
                    };
                }

                // Load the transaction from the database
                var financialTransactionService = new FinancialTransactionService( rockContext );
                var transaction = financialTransactionService.GetByTransactionCode( financialGateway.Id, options.TransactionCode );
                var transactionPersonAlias = transaction?.AuthorizedPersonAlias;
                var transactionPerson = transactionPersonAlias?.Person;
                var paymentDetail = transaction?.FinancialPaymentDetail;

                if ( transactionPerson is null || paymentDetail is null )
                {
                    return new SaveFinancialAccountFormSaveAccountResultBag
                    {
                        Title = "Invalid Transaction",
                        Detail = "Sorry, the account information cannot be saved as there's not a valid transaction to reference",
                        IsSuccess = false
                    };
                }

                // Create the login if needed
                if ( isAnonymous )
                {
                    var user = UserLoginService.Create(
                        rockContext,
                        transactionPerson,
                        AuthenticationServiceType.Internal,
                        EntityTypeCache.Get( SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                        options.Username,
                        options.Password,
                        false );

                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );
                    // TODO mergeFields.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );
                    mergeFields.Add( "Person", transactionPerson );
                    mergeFields.Add( "User", user );

                    var emailMessage = new RockEmailMessage( SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT.AsGuid() );
                    emailMessage.AddRecipient( new RockEmailMessageRecipient( transactionPerson, mergeFields ) );
                    // TODO emailMessage.AppRoot = ResolveRockUrl( "~/" );
                    // TODO emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                    emailMessage.CreateCommunicationRecord = false;
                    emailMessage.Send();
                }

                var savedAccount = new FinancialPersonSavedAccount
                {
                    PersonAliasId = transactionPersonAlias.Id,
                    ReferenceNumber = options.TransactionCode,
                    GatewayPersonIdentifier = options.GatewayPersonIdentifier,
                    Name = options.SavedAccountName,
                    TransactionCode = options.TransactionCode,
                    FinancialGatewayId = financialGateway.Id,
                    FinancialPaymentDetail = new FinancialPaymentDetail
                    {
                        AccountNumberMasked = paymentDetail.AccountNumberMasked,
                        CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId,
                        CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId,
                        NameOnCard = paymentDetail.NameOnCard,
                        ExpirationMonth = paymentDetail.ExpirationMonth,
                        ExpirationYear = paymentDetail.ExpirationYear,
                        BillingLocationId = paymentDetail.BillingLocationId
                    }
                };

                var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
                financialPersonSavedAccountService.Add( savedAccount );

                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", RockRequestContext.CurrentPerson );
                rockContext.SaveChanges();

                return new SaveFinancialAccountFormSaveAccountResultBag
                {
                    Title = "Success",
                    Detail = "The account has been saved for future use",
                    IsSuccess = true
                };
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Retrieve a list of ListItems representing components for the given container type
        /// </summary>
        /// <param name="containerType"></param>
        /// <returns>A list of ListItems representing components</returns>
        private List<ListItemBag> GetComponentListItems( string containerType )
        {
            return GetComponentListItems( containerType, (x) => true );
        }

        /// <summary>
        /// Retrieve a list of ListItems representing components for the given container type. Filters any components
        /// out that don't pass the given validator
        /// </summary>
        /// <param name="containerType"></param>
        /// <param name="isValidComponentChecker"></param>
        /// <returns>A list of ListItems representing components</returns>
        private List<ListItemBag> GetComponentListItems( string containerType, Func<Component, bool> isValidComponentChecker )
        {
            if ( containerType.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var resolvedContainerType = Type.GetType( containerType );

            if ( resolvedContainerType == null )
            {
                return null;
            }

            var instanceProperty = resolvedContainerType.GetProperty( "Instance" );

            if ( instanceProperty == null )
            {
                return null;
            }

            var container = instanceProperty.GetValue( null, null ) as IContainer;
            var componentDictionary = container?.Dictionary;

            var items = new List<ListItemBag>();

            foreach ( var component in componentDictionary )
            {
                var componentValue = component.Value.Value;
                var entityType = EntityTypeCache.Get( componentValue.GetType() );

                if ( !componentValue.IsActive || entityType == null || !isValidComponentChecker(componentValue) )
                {
                    continue;
                }

                var componentName = component.Value.Key;

                // If the component name already has a space then trust
                // that they are using the exact name formatting they want.
                if ( !componentName.Contains( ' ' ) )
                {
                    componentName = componentName.SplitCase();
                }

                items.Add( new ListItemBag
                {
                    Text = componentName,
                    Value = entityType.Guid.ToString().ToUpper()
                } );
            }


            return items;
        }

        #endregion
    }
}