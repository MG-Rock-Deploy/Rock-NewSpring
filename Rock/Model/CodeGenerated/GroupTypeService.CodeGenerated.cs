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
    /// GroupType Service class
    /// </summary>
    public partial class GroupTypeService : Service<GroupType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public GroupTypeService(RockContext context) : base(context)
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
        public bool CanDelete( GroupType item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new Service<ConnectionOpportunityGroupConfig>( Context ).Queryable().Any( a => a.GroupTypeId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", GroupType.FriendlyTypeName, ConnectionOpportunityGroupConfig.FriendlyTypeName );
                return false;
            }

            if ( new Service<Group>( Context ).Queryable().Any( a => a.GroupTypeId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", GroupType.FriendlyTypeName, Group.FriendlyTypeName );
                return false;
            }

            if ( new Service<GroupHistorical>( Context ).Queryable().Any( a => a.GroupTypeId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", GroupType.FriendlyTypeName, GroupHistorical.FriendlyTypeName );
                return false;
            }

            if ( new Service<GroupMember>( Context ).Queryable().Any( a => a.GroupTypeId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", GroupType.FriendlyTypeName, GroupMember.FriendlyTypeName );
                return false;
            }

            if ( new Service<GroupMemberScheduleTemplate>( Context ).Queryable().Any( a => a.GroupTypeId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", GroupType.FriendlyTypeName, GroupMemberScheduleTemplate.FriendlyTypeName );
                return false;
            }

            // ignoring GroupRequirement,GroupTypeId

            if ( new Service<GroupType>( Context ).Queryable().Any( a => a.InheritedGroupTypeId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", GroupType.FriendlyTypeName, GroupType.FriendlyTypeName );
                return false;
            }

            if ( new Service<RegistrationTemplate>( Context ).Queryable().Any( a => a.GroupTypeId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", GroupType.FriendlyTypeName, RegistrationTemplate.FriendlyTypeName );
                return false;
            }

            if ( new Service<RegistrationTemplatePlacement>( Context ).Queryable().Any( a => a.GroupTypeId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", GroupType.FriendlyTypeName, RegistrationTemplatePlacement.FriendlyTypeName );
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// GroupType View Model Helper
    /// </summary>
    [DefaultViewModelHelper( typeof( GroupType ) )]
    public partial class GroupTypeViewModelHelper : ViewModelHelper<GroupType, GroupTypeBag>
    {
        /// <summary>
        /// Converts the model to a view model.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public override GroupTypeBag CreateViewModel( GroupType model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = new GroupTypeBag
            {
                IdKey = model.IdKey,
                AdministratorTerm = model.AdministratorTerm,
                AllowAnyChildGroupType = model.AllowAnyChildGroupType,
                AllowedScheduleTypes = ( int ) model.AllowedScheduleTypes,
                AllowGroupSync = model.AllowGroupSync,
                AllowMultipleLocations = model.AllowMultipleLocations,
                AllowSpecificGroupMemberAttributes = model.AllowSpecificGroupMemberAttributes,
                AllowSpecificGroupMemberWorkflows = model.AllowSpecificGroupMemberWorkflows,
                AttendanceCountsAsWeekendService = model.AttendanceCountsAsWeekendService,
                AttendancePrintTo = ( int ) model.AttendancePrintTo,
                AttendanceRule = ( int ) model.AttendanceRule,
                DefaultGroupRoleId = model.DefaultGroupRoleId,
                Description = model.Description,
                EnableGroupHistory = model.EnableGroupHistory,
                EnableGroupTag = model.EnableGroupTag,
                EnableInactiveReason = model.EnableInactiveReason,
                EnableLocationSchedules = model.EnableLocationSchedules,
                EnableRSVP = model.EnableRSVP,
                EnableSpecificGroupRequirements = model.EnableSpecificGroupRequirements,
                GroupAttendanceRequiresLocation = model.GroupAttendanceRequiresLocation,
                GroupAttendanceRequiresSchedule = model.GroupAttendanceRequiresSchedule,
                GroupCapacityRule = ( int ) model.GroupCapacityRule,
                GroupMemberTerm = model.GroupMemberTerm,
                GroupsRequireCampus = model.GroupsRequireCampus,
                GroupStatusDefinedTypeId = model.GroupStatusDefinedTypeId,
                GroupTerm = model.GroupTerm,
                GroupTypeColor = model.GroupTypeColor,
                GroupTypePurposeValueId = model.GroupTypePurposeValueId,
                GroupViewLavaTemplate = model.GroupViewLavaTemplate,
                IconCssClass = model.IconCssClass,
                IgnorePersonInactivated = model.IgnorePersonInactivated,
                InheritedGroupTypeId = model.InheritedGroupTypeId,
                IsCapacityRequired = model.IsCapacityRequired,
                IsIndexEnabled = model.IsIndexEnabled,
                IsSchedulingEnabled = model.IsSchedulingEnabled,
                IsSystem = model.IsSystem,
                LocationSelectionMode = ( int ) model.LocationSelectionMode,
                Name = model.Name,
                Order = model.Order,
                RequiresInactiveReason = model.RequiresInactiveReason,
                RequiresReasonIfDeclineSchedule = model.RequiresReasonIfDeclineSchedule,
                RSVPReminderOffsetDays = model.RSVPReminderOffsetDays,
                RSVPReminderSystemCommunicationId = model.RSVPReminderSystemCommunicationId,
                ScheduleCancellationWorkflowTypeId = model.ScheduleCancellationWorkflowTypeId,
                ScheduleConfirmationEmailOffsetDays = model.ScheduleConfirmationEmailOffsetDays,
                ScheduleConfirmationSystemCommunicationId = model.ScheduleConfirmationSystemCommunicationId,
                ScheduleReminderEmailOffsetDays = model.ScheduleReminderEmailOffsetDays,
                ScheduleReminderSystemCommunicationId = model.ScheduleReminderSystemCommunicationId,
                SendAttendanceReminder = model.SendAttendanceReminder,
                ShowAdministrator = model.ShowAdministrator,
                ShowConnectionStatus = model.ShowConnectionStatus,
                ShowInGroupList = model.ShowInGroupList,
                ShowInNavigation = model.ShowInNavigation,
                ShowMaritalStatus = model.ShowMaritalStatus,
                TakesAttendance = model.TakesAttendance,
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
    public static partial class GroupTypeExtensionMethods
    {
        /// <summary>
        /// Clones this GroupType object to a new GroupType object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static GroupType Clone( this GroupType source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as GroupType;
            }
            else
            {
                var target = new GroupType();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Clones this GroupType object to a new GroupType object with default values for the properties in the Entity and Model base classes.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static GroupType CloneWithoutIdentity( this GroupType source )
        {
            var target = new GroupType();
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
        /// Copies the properties from another GroupType object to this GroupType object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this GroupType target, GroupType source )
        {
            target.Id = source.Id;
            target.AdministratorTerm = source.AdministratorTerm;
            target.AllowAnyChildGroupType = source.AllowAnyChildGroupType;
            target.AllowedScheduleTypes = source.AllowedScheduleTypes;
            target.AllowGroupSync = source.AllowGroupSync;
            target.AllowMultipleLocations = source.AllowMultipleLocations;
            target.AllowSpecificGroupMemberAttributes = source.AllowSpecificGroupMemberAttributes;
            target.AllowSpecificGroupMemberWorkflows = source.AllowSpecificGroupMemberWorkflows;
            target.AttendanceCountsAsWeekendService = source.AttendanceCountsAsWeekendService;
            target.AttendancePrintTo = source.AttendancePrintTo;
            target.AttendanceRule = source.AttendanceRule;
            target.DefaultGroupRoleId = source.DefaultGroupRoleId;
            target.Description = source.Description;
            target.EnableGroupHistory = source.EnableGroupHistory;
            target.EnableGroupTag = source.EnableGroupTag;
            target.EnableInactiveReason = source.EnableInactiveReason;
            target.EnableLocationSchedules = source.EnableLocationSchedules;
            target.EnableRSVP = source.EnableRSVP;
            target.EnableSpecificGroupRequirements = source.EnableSpecificGroupRequirements;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.GroupAttendanceRequiresLocation = source.GroupAttendanceRequiresLocation;
            target.GroupAttendanceRequiresSchedule = source.GroupAttendanceRequiresSchedule;
            target.GroupCapacityRule = source.GroupCapacityRule;
            target.GroupMemberTerm = source.GroupMemberTerm;
            target.GroupsRequireCampus = source.GroupsRequireCampus;
            target.GroupStatusDefinedTypeId = source.GroupStatusDefinedTypeId;
            target.GroupTerm = source.GroupTerm;
            target.GroupTypeColor = source.GroupTypeColor;
            target.GroupTypePurposeValueId = source.GroupTypePurposeValueId;
            target.GroupViewLavaTemplate = source.GroupViewLavaTemplate;
            target.IconCssClass = source.IconCssClass;
            target.IgnorePersonInactivated = source.IgnorePersonInactivated;
            target.InheritedGroupTypeId = source.InheritedGroupTypeId;
            target.IsCapacityRequired = source.IsCapacityRequired;
            target.IsIndexEnabled = source.IsIndexEnabled;
            target.IsSchedulingEnabled = source.IsSchedulingEnabled;
            target.IsSystem = source.IsSystem;
            target.LocationSelectionMode = source.LocationSelectionMode;
            target.Name = source.Name;
            target.Order = source.Order;
            target.RequiresInactiveReason = source.RequiresInactiveReason;
            target.RequiresReasonIfDeclineSchedule = source.RequiresReasonIfDeclineSchedule;
            target.RSVPReminderOffsetDays = source.RSVPReminderOffsetDays;
            target.RSVPReminderSystemCommunicationId = source.RSVPReminderSystemCommunicationId;
            target.ScheduleCancellationWorkflowTypeId = source.ScheduleCancellationWorkflowTypeId;
            target.ScheduleConfirmationEmailOffsetDays = source.ScheduleConfirmationEmailOffsetDays;
            target.ScheduleConfirmationSystemCommunicationId = source.ScheduleConfirmationSystemCommunicationId;
            target.ScheduleReminderEmailOffsetDays = source.ScheduleReminderEmailOffsetDays;
            target.ScheduleReminderSystemCommunicationId = source.ScheduleReminderSystemCommunicationId;
            target.SendAttendanceReminder = source.SendAttendanceReminder;
            target.ShowAdministrator = source.ShowAdministrator;
            target.ShowConnectionStatus = source.ShowConnectionStatus;
            target.ShowInGroupList = source.ShowInGroupList;
            target.ShowInNavigation = source.ShowInNavigation;
            target.ShowMaritalStatus = source.ShowMaritalStatus;
            target.TakesAttendance = source.TakesAttendance;
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
        public static GroupTypeBag ToViewModel( this GroupType model, Person currentPerson = null, bool loadAttributes = false )
        {
            var helper = new GroupTypeViewModelHelper();
            var viewModel = helper.CreateViewModel( model, currentPerson, loadAttributes );
            return viewModel;
        }

    }

}
