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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.StatementGenerator.Rest
{
    /// <summary>
    /// NOTE: WebApi doesn't support Controllers with the Same Name, even if they have different namespaces, so can't call this FinancialTransactionsController
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
    [RockObsolete( "1.12.4" )]
    [Rock.SystemGuid.RestControllerGuid( "745A5C57-6091-4563-A447-566BC649C776")]
    public class StatementGeneratorFinancialTransactionsController : Rock.Rest.ApiControllerBase
    {
        /// <summary>
        /// Gets the statement generator templates.
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorTemplates" )]
        [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
        [RockObsolete( "1.12.4" )]
        public List<DefinedValue> GetStatementGeneratorTemplates()
        {
            List<DefinedValue> result = null;
            using ( var rockContext = new RockContext() )
            {
                result = new Model.DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( Rock.StatementGenerator.SystemGuid.DefinedType.STATEMENT_GENERATOR_LAVA_TEMPLATE.AsGuid() ).AsNoTracking()
                    .ToList();

                result.ForEach( a => a.LoadAttributes( rockContext ) );
            }

            return result;
        }

        /// <summary>
        /// Gets the statement generator recipients. This will be sorted based on the StatementGeneratorOptions
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorRecipients" )]
        [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
        [RockObsolete( "1.12.4" )]
        public List<StatementGeneratorRecipient> GetStatementGeneratorRecipients( [FromBody] StatementGeneratorOptions options )
        {
            if ( options == null )
            {
                throw new Exception( "StatementGenerationOption options must be specified" );
            }

            using ( var rockContext = new RockContext() )
            {
                var financialTransactionQry = GetFinancialTransactionQuery( options, rockContext, true );
                var financialPledgeQry = GetFinancialPledgeQuery( options, rockContext, true );

                // Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
                // These are Persons that give as part of a Group.For example, Husband and Wife
                var qryGivingGroupIdsThatHaveTransactions = financialTransactionQry.Select( a => a.AuthorizedPersonAlias.Person.GivingGroupId ).Where( a => a.HasValue )
                    .Select( a => new
                    {
                        PersonId = ( int? ) null,
                        GroupId = a.Value
                    } ).Distinct();

                var qryGivingGroupIdsThatHavePledges = financialPledgeQry.Select( a => a.PersonAlias.Person.GivingGroupId ).Where( a => a.HasValue )
                    .Select( a => new
                    {
                        PersonId = ( int? ) null,
                        GroupId = a.Value
                    } ).Distinct();

                // Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.
                // These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order
                // to determine which address(es) the statements need to be mailed to
                var groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
                var groupMembersQry = new GroupMemberService( rockContext ).Queryable( true ).Where( m => m.Group.GroupTypeId == groupTypeIdFamily );

                var qryIndividualGiversThatHaveTransactions = financialTransactionQry
                    .Where( a => !a.AuthorizedPersonAlias.Person.GivingGroupId.HasValue )
                    .Select( a => a.AuthorizedPersonAlias.PersonId ).Distinct()
                    .Join( groupMembersQry, p => p, m => m.PersonId, ( p, m ) => new { PersonId = ( int? ) p, GroupId = m.GroupId } );

                var qryIndividualGiversThatHavePledges = financialPledgeQry
                    .Where( a => !a.PersonAlias.Person.GivingGroupId.HasValue )
                    .Select( a => a.PersonAlias.PersonId ).Distinct()
                    .Join( groupMembersQry, p => p, m => m.PersonId, ( p, m ) => new { PersonId = ( int? ) p, GroupId = m.GroupId } );

                var unionQry = qryGivingGroupIdsThatHaveTransactions.Union( qryIndividualGiversThatHaveTransactions );

                if ( options.PledgesAccountIds.Any() )
                {
                    unionQry = unionQry.Union( qryGivingGroupIdsThatHavePledges ).Union( qryIndividualGiversThatHavePledges );
                }

                /*  Limit to Mailing Address and sort by ZipCode */
                IQueryable<GroupLocation> groupLocationsQry = GetGroupLocationQuery( rockContext );

                // Do an outer join on location so we can include people that don't have an address (if options.IncludeIndividualsWithNoAddress) //
                var unionJoinLocationQry = from pg in unionQry
                                           join l in groupLocationsQry on pg.GroupId equals l.GroupId into u
                                           from l in u.DefaultIfEmpty()
                                           select new
                                           {
                                               pg.PersonId,
                                               pg.GroupId,
                                               LocationGuid = ( Guid? ) l.Location.Guid,
                                               l.Location.PostalCode
                                           };

                // Require that LocationId has a value unless this is for a specific person, a dataview, or the IncludeIndividualsWithNoAddress option is enabled
                if ( options.PersonId == null && options.DataViewId == null && !options.IncludeIndividualsWithNoAddress )
                {
                    unionJoinLocationQry = unionJoinLocationQry.Where( a => a.LocationGuid.HasValue );
                }

                if ( options.OrderBy == OrderBy.PostalCode )
                {
                    unionJoinLocationQry = unionJoinLocationQry.OrderBy( a => a.PostalCode );
                }
                else if ( options.OrderBy == OrderBy.LastName )
                {
                    // get a query to look up LastName for recipients that give as a group
                    var qryLastNameAsGroup = new PersonService( rockContext ).Queryable( true, true )
                        .Where( a => a.GivingLeaderId == a.Id && a.GivingGroupId.HasValue )
                        .Select( a => new
                        {
                            a.GivingGroupId,
                            a.LastName,
                            a.FirstName
                        } );

                    // get a query to look up LastName for recipients that give as individuals
                    var qryLastNameAsIndividual = new PersonService( rockContext ).Queryable( true, true );

                    unionJoinLocationQry = unionJoinLocationQry.Select( a => new
                    {
                        a.PersonId,
                        a.GroupId,
                        a.LocationGuid,
                        a.PostalCode,
                        GivingLeader = a.PersonId.HasValue ?
                            qryLastNameAsIndividual.Where( p => p.Id == a.PersonId ).Select( x => new { x.LastName, x.FirstName } ).FirstOrDefault()
                            : qryLastNameAsGroup.Where( gl => gl.GivingGroupId == a.GroupId ).Select( x => new { x.LastName, x.FirstName } ).FirstOrDefault()
                    } ).OrderBy( a => a.GivingLeader.LastName ).ThenBy( a => a.GivingLeader.FirstName )
                    .Select( a => new
                    {
                        a.PersonId,
                        a.GroupId,
                        a.LocationGuid,
                        a.PostalCode
                    } );
                }

                var givingIdsQry = unionJoinLocationQry.Select( a => new { a.PersonId, a.GroupId, a.LocationGuid } );

                var recipientList = givingIdsQry.ToList().Select( a => new StatementGeneratorRecipient { GroupId = a.GroupId, PersonId = a.PersonId, LocationGuid = a.LocationGuid } ).ToList();

                if ( options.DataViewId.HasValue )
                {
                    var dataView = new DataViewService( new RockContext() ).Get( options.DataViewId.Value );
                    if ( dataView != null )
                    {
                        var dataViewGetQueryArgs = new DataViewGetQueryArgs();
                        var personList = dataView.GetQuery( dataViewGetQueryArgs ).OfType<Rock.Model.Person>().Select( a => new { a.Id, a.GivingGroupId } ).ToList();
                        HashSet<int> personIds = new HashSet<int>( personList.Select( a => a.Id ) );
                        HashSet<int> groupsIds = new HashSet<int>( personList.Where( a => a.GivingGroupId.HasValue ).Select( a => a.GivingGroupId.Value ).Distinct() );

                        foreach ( var recipient in recipientList.ToList() )
                        {
                            if ( recipient.PersonId.HasValue )
                            {
                                if ( !personIds.Contains( recipient.PersonId.Value ) )
                                {
                                    recipientList.Remove( recipient );
                                }
                            }
                            else
                            {
                                if ( !groupsIds.Contains( recipient.GroupId ) )
                                {
                                    recipientList.Remove( recipient );
                                }
                            }
                        }
                    }
                }

                return recipientList;
            }
        }

        /// <summary>
        /// Gets the group location query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
        [RockObsolete( "1.12.4" )]
        private static IQueryable<GroupLocation> GetGroupLocationQuery( RockContext rockContext )
        {
            var groupLocationTypeIdHome = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() )?.Id;
            var groupLocationTypeIdWork = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() )?.Id;
            var groupLocationTypeIds = new List<int>();
            if ( groupLocationTypeIdHome.HasValue )
            {
                groupLocationTypeIds.Add( groupLocationTypeIdHome.Value );
            }

            if ( groupLocationTypeIdWork.HasValue )
            {
                groupLocationTypeIds.Add( groupLocationTypeIdWork.Value );
            }

            IQueryable<GroupLocation> groupLocationsQry = null;
            if ( groupLocationTypeIds.Count == 2 )
            {
                groupLocationsQry = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.IsMailingLocation && a.GroupLocationTypeValueId.HasValue )
                    .Where( a => a.GroupLocationTypeValueId == groupLocationTypeIdHome.Value || a.GroupLocationTypeValueId == groupLocationTypeIdWork.Value );
            }
            else
            {
                groupLocationsQry = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.IsMailingLocation && a.GroupLocationTypeValueId.HasValue && groupLocationTypeIds.Contains( a.GroupLocationTypeValueId.Value ) );
            }

            return groupLocationsQry;
        }

        /// <summary>
        /// Gets the statement generator recipient result for a GivingGroup that doesn't have an address
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorRecipientResult" )]
        [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
        [RockObsolete( "1.12.4" )]
        public StatementGeneratorRecipientResult GetStatementGeneratorRecipientResult( int groupId, [FromBody] StatementGeneratorOptions options )
        {
            return GetStatementGeneratorRecipientResult( groupId, ( int? ) null, ( Guid? ) null, options );
        }

        /// <summary>
        /// Gets the statement generator recipient result for an individual that doesn't have an address
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorRecipientResult" )]
        [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
        [RockObsolete( "1.12.4" )]
        public StatementGeneratorRecipientResult GetStatementGeneratorRecipientResult( int groupId, int? personId, [FromBody] StatementGeneratorOptions options )
        {
            return GetStatementGeneratorRecipientResult( groupId, personId, ( Guid? ) null, options );
        }

        /// <summary>
        /// Gets the statement generator recipient result for a GivingGroup with the specified address (locationGuid)
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationGuid">The location unique identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorRecipientResult" )]
        [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
        [RockObsolete( "1.12.4" )]
        public StatementGeneratorRecipientResult GetStatementGeneratorRecipientResult( int groupId, Guid? locationGuid, [FromBody] StatementGeneratorOptions options )
        {
            return GetStatementGeneratorRecipientResult( groupId, ( int? ) null, locationGuid, options );
        }

        /// <summary>
        /// Gets the statement generator recipient result for a specific person and associated group (family) with the specified address (locationGuid)
        /// NOTE: If a person is in multiple families, call this for each of the families so that the statement will go to the address of each family
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="locationGuid">The location unique identifier.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// StatementGenerationOption options must be specified
        /// or
        /// LayoutDefinedValueGuid option must be specified
        /// </exception>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/FinancialTransactions/GetStatementGeneratorRecipientResult" )]
        [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
        [RockObsolete( "1.12.4" )]
        public StatementGeneratorRecipientResult GetStatementGeneratorRecipientResult( int groupId, int? personId, Guid? locationGuid, [FromBody] StatementGeneratorOptions options )
        {
            return GenerateStatementGeneratorRecipientResult( groupId, personId, locationGuid, this.GetPerson(), options );
        }

        /// <summary>
        /// Generates the statement generator recipient result.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="locationGuid">The location unique identifier.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// StatementGenerationOption options must be specified
        /// or
        /// LayoutDefinedValueGuid option must be specified
        /// </exception>
        private static StatementGeneratorRecipientResult GenerateStatementGeneratorRecipientResult( int groupId, int? personId, Guid? locationGuid, Person currentPerson, StatementGeneratorOptions options )
        {
            if ( options == null )
            {
                throw new Exception( "StatementGenerationOption options must be specified" );
            }

            if ( options.LayoutDefinedValueGuid == null )
            {
                throw new Exception( "LayoutDefinedValueGuid option must be specified" );
            }

            var result = new StatementGeneratorRecipientResult();
            result.GroupId = groupId;
            result.PersonId = personId;

            using ( var rockContext = new RockContext() )
            {
                var financialTransactionQry = GetFinancialTransactionQuery( options, rockContext, false );
                var financialPledgeQry = GetFinancialPledgeQuery( options, rockContext, false );

                var personList = new List<Person>();
                Person person = null;
                if ( personId.HasValue )
                {
                    person = new PersonService( rockContext ).Queryable().Include( a => a.Aliases ).Where( a => a.Id == personId.Value ).FirstOrDefault();
                    personList.Add( person );
                }
                else
                {
                    // get transactions for all the persons in the specified group that have specified that group as their GivingGroup
                    GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                    personList = groupMemberService.GetByGroupId( groupId, true ).Where( a => a.Person.GivingGroupId == groupId ).Select( s => s.Person ).Include( a => a.Aliases ).ToList();
                    person = personList.FirstOrDefault();
                }

                if ( options.ExcludeOptedOutIndividuals == true && !options.DataViewId.HasValue )
                {
                    int? doNotSendGivingStatementAttributeId = AttributeCache.Get( Rock.StatementGenerator.SystemGuid.Attribute.PERSON_DO_NOT_SEND_GIVING_STATEMENT.AsGuid() )?.Id;
                    if ( doNotSendGivingStatementAttributeId.HasValue )
                    {
                        var personIds = personList.Select( a => a.Id ).ToList();
                        var optedOutPersonQry = new AttributeValueService( rockContext ).Queryable().Where( a => a.AttributeId == doNotSendGivingStatementAttributeId );
                        if ( personIds.Count == 1 )
                        {
                            int entityPersonId = personIds[0];
                            optedOutPersonQry = optedOutPersonQry.Where( a => a.EntityId == entityPersonId );
                        }
                        else
                        {
                            optedOutPersonQry = optedOutPersonQry.Where( a => personIds.Contains( a.EntityId.Value ) );
                        }

                        var optedOutPersonIds = optedOutPersonQry
                            .Select( a => new
                            {
                                PersonId = a.EntityId.Value,
                                a.Value
                            } ).ToList().Where( a => a.Value.AsBoolean() == true ).Select( a => a.PersonId ).ToList();

                        if ( optedOutPersonIds.Any() )
                        {
                            bool givingLeaderOptedOut = personList.Any( a => optedOutPersonIds.Contains( a.Id ) && a.GivingLeaderId == a.Id );

                            var remaingPersonIds = personList.Where( a => !optedOutPersonIds.Contains( a.Id ) ).ToList();

                            if ( givingLeaderOptedOut || !remaingPersonIds.Any() )
                            {
                                // If the giving leader opted out, or if there aren't any people in the giving statement that haven't opted out, return NULL and OptedOut = true
                                result.OptedOut = true;
                                result.Html = null;
                                return result;
                            }
                        }
                    }
                }

                var personAliasIds = personList.SelectMany( a => a.Aliases.Select( x => x.Id ) ).ToList();
                if ( personAliasIds.Count == 1 )
                {
                    var personAliasId = personAliasIds[0];
                    financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAliasId.Value == personAliasId );
                }
                else
                {
                    financialTransactionQry = financialTransactionQry.Where( a => personAliasIds.Contains( a.AuthorizedPersonAliasId.Value ) );
                }

                var financialTransactionsList = financialTransactionQry
                    .Include( a => a.FinancialPaymentDetail )
                    .Include( a => a.FinancialPaymentDetail.CurrencyTypeValue )
                    .Include( a => a.TransactionDetails )
                    .Include( a => a.TransactionDetails.Select( x => x.Account ) )
                    .OrderBy( a => a.TransactionDateTime ).ToList();

                foreach ( var financialTransaction in financialTransactionsList )
                {
                    if ( options.TransactionAccountIds != null )
                    {
                        // remove any Accounts that were not included (in case there was a mix of included and not included accounts in the transaction)
                        financialTransaction.TransactionDetails = financialTransaction.TransactionDetails.Where( a => options.TransactionAccountIds.Contains( a.AccountId ) ).ToList();
                    }

                    financialTransaction.TransactionDetails = financialTransaction.TransactionDetails.OrderBy( a => a.Account.Order ).ThenBy( a => a.Account.PublicName ).ToList();
                }

                var lavaTemplateValue = DefinedValueCache.Get( options.LayoutDefinedValueGuid.Value );
                var lavaTemplateLava = lavaTemplateValue.GetAttributeValue( "LavaTemplate" );
                var lavaTemplateFooterLava = lavaTemplateValue.GetAttributeValue( "FooterHtml" );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false, GetDeviceFamily = false, GetOSFamily = false, GetPageContext = false, GetPageParameters = false, GetCampuses = true, GetCurrentPerson = true } );
                mergeFields.Add( "LavaTemplate", lavaTemplateValue );

                mergeFields.Add( "PersonList", personList );
                mergeFields.Add( "StatementStartDate", options.StartDate );
                var humanFriendlyEndDate = options.EndDate.HasValue ? options.EndDate.Value.AddDays( -1 ) : RockDateTime.Now.Date;
                mergeFields.Add( "StatementEndDate", humanFriendlyEndDate );

                string salutation;
                if ( personId.HasValue )
                {
                    // if the person gives as an individual, the salutation should be just the person's name
                    salutation = person.FullName;
                }
                else
                {
                    // if giving as a group, the salutation is family title
                    string familyTitle;
                    if ( person != null && person.PrimaryFamilyId == groupId )
                    {
                        // this is how familyTitle should able to be determined in most cases
                        familyTitle = person.PrimaryFamily.GroupSalutation;
                    }
                    else
                    {
                        // This could happen if the person is from multiple families, and specified groupId is not their PrimaryFamily
                        familyTitle = new GroupService( rockContext ).GetSelect( groupId, s => s.GroupSalutation );
                    }

                    if ( familyTitle.IsNullOrWhiteSpace() )
                    {
                        // shouldn't happen, just in case the familyTitle is blank, just return the person's name
                        familyTitle = person.FullName;
                    }

                    salutation = familyTitle;
                }

                mergeFields.Add( "Salutation", salutation );

                Location mailingAddress;

                if ( locationGuid.HasValue )
                {
                    // get the location that was specified for the recipient
                    mailingAddress = new LocationService( rockContext ).Get( locationGuid.Value );
                }
                else
                {
                    // for backwards compatibility, get the first address
                    IQueryable<GroupLocation> groupLocationsQry = GetGroupLocationQuery( rockContext );
                    mailingAddress = groupLocationsQry.Where( a => a.GroupId == groupId ).Select( a => a.Location ).FirstOrDefault();
                }

                mergeFields.Add( "MailingAddress", mailingAddress );

                if ( mailingAddress != null )
                {
                    mergeFields.Add( "StreetAddress1", mailingAddress.Street1 );
                    mergeFields.Add( "StreetAddress2", mailingAddress.Street2 );
                    mergeFields.Add( "City", mailingAddress.City );
                    mergeFields.Add( "State", mailingAddress.State );
                    mergeFields.Add( "PostalCode", mailingAddress.PostalCode );
                    mergeFields.Add( "Country", mailingAddress.Country );
                }
                else
                {
                    mergeFields.Add( "StreetAddress1", string.Empty );
                    mergeFields.Add( "StreetAddress2", string.Empty );
                    mergeFields.Add( "City", string.Empty );
                    mergeFields.Add( "State", string.Empty );
                    mergeFields.Add( "PostalCode", string.Empty );
                    mergeFields.Add( "Country", string.Empty );
                }

                var transactionDetailListAll = financialTransactionsList.SelectMany( a => a.TransactionDetails ).ToList();

                if ( options.HideRefundedTransactions && transactionDetailListAll.Any( a => a.Amount < 0 ) )
                {
                    var allRefunds = transactionDetailListAll.SelectMany( a => a.Transaction.Refunds ).ToList();
                    foreach ( var refund in allRefunds )
                    {
                        foreach ( var refundedOriginalTransactionDetail in refund.OriginalTransaction.TransactionDetails )
                        {
                            // remove the refund's original TransactionDetails from the results
                            if ( transactionDetailListAll.Contains( refundedOriginalTransactionDetail ) )
                            {
                                transactionDetailListAll.Remove( refundedOriginalTransactionDetail );
                                foreach ( var refundDetailId in refund.FinancialTransaction.TransactionDetails.Select( a => a.Id ) )
                                {
                                    // remove the refund's transaction from the results
                                    var refundDetail = transactionDetailListAll.FirstOrDefault( a => a.Id == refundDetailId );
                                    if ( refundDetail != null )
                                    {
                                        transactionDetailListAll.Remove( refundDetail );
                                    }
                                }
                            }
                        }
                    }
                }

                if ( options.HideCorrectedTransactions && transactionDetailListAll.Any( a => a.Amount < 0 ) )
                {
                    // Hide transactions that are corrected on the same date. Transactions that have a matching negative dollar amount on the same date and same account will not be shown.

                    // get a list of dates that have at least one negative transaction
                    var transactionsByDateList = transactionDetailListAll.GroupBy( a => a.Transaction.TransactionDateTime.Value.Date ).Select( a => new
                    {
                        Date = a.Key,
                        TransactionDetails = a.ToList()
                    } )
                    .Where( a => a.TransactionDetails.Any( x => x.Amount < 0 ) )
                    .ToList();

                    foreach ( var transactionsByDate in transactionsByDateList )
                    {
                        foreach ( var negativeTransaction in transactionsByDate.TransactionDetails.Where( a => a.Amount < 0 ) )
                        {
                            // find the first transaction that has an amount that matches the negative amount (on the same day and same account)
                            // and make sure the matching transaction doesn't already have a refund associated with it
                            var correctedTransactionDetail = transactionsByDate.TransactionDetails
                                .Where( a => ( a.Amount == ( -negativeTransaction.Amount ) && a.AccountId == negativeTransaction.AccountId ) && !a.Transaction.Refunds.Any() )
                                .FirstOrDefault();
                            if ( correctedTransactionDetail != null )
                            {
                                // if the transaction was corrected, remove it, and also remove the associated correction (the negative one) transaction
                                transactionDetailListAll.Remove( correctedTransactionDetail );
                                transactionDetailListAll.Remove( negativeTransaction );
                            }
                        }
                    }
                }

                List<FinancialTransactionDetail> transactionDetailListCash = transactionDetailListAll;
                List<FinancialTransactionDetail> transactionDetailListNonCash = new List<FinancialTransactionDetail>();

                if ( options.CurrencyTypeIdsCash != null )
                {
                    // NOTE: if there isn't a FinancialPaymentDetail record, assume it is Cash
                    transactionDetailListCash = transactionDetailListCash.Where( a =>
                        ( a.Transaction.FinancialPaymentDetailId == null ) ||
                        ( a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue && options.CurrencyTypeIdsCash.Contains( a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) ) ).ToList();
                }

                if ( options.CurrencyTypeIdsNonCash != null )
                {
                    transactionDetailListNonCash = transactionDetailListAll.Where( a =>
                        a.Transaction.FinancialPaymentDetailId.HasValue &&
                        a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue
                        && options.CurrencyTypeIdsNonCash.Contains( a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) ).ToList();
                }

                // Add Merge Fields for Transactions for custom Statements that might want to organize the output by Transaction instead of TransactionDetail
                var transactionListCash = transactionDetailListCash.GroupBy( a => a.Transaction ).Select( a => a.Key ).ToList();
                var transactionListNonCash = transactionDetailListNonCash.GroupBy( a => a.Transaction ).Select( a => a.Key ).ToList();
                mergeFields.Add( "Transactions", transactionListCash );
                mergeFields.Add( "TransactionsNonCash", transactionListNonCash );

                // Add the standard TransactionDetails and TransactionDetailsNonCash that the default Rock templates use
                mergeFields.Add( "TransactionDetails", transactionDetailListCash );
                mergeFields.Add( "TransactionDetailsNonCash", transactionDetailListNonCash );

                mergeFields.Add( "TotalContributionAmount", transactionDetailListCash.Sum( a => a.Amount ) );
                mergeFields.Add( "TotalContributionCount", transactionDetailListCash.Count() );

                mergeFields.Add( "TotalContributionAmountNonCash", transactionDetailListNonCash.Sum( a => a.Amount ) );
                mergeFields.Add( "TotalContributionCountNonCash", transactionDetailListNonCash.Count() );

                mergeFields.Add(
                    "AccountSummary",
                    transactionDetailListCash
                        .GroupBy( t => t.Account.PublicName )
                        .Select( s => new
                        {
                            AccountName = s.FirstOrDefault().Account.PublicName ?? s.Key,
                            Total = s.Sum( a => a.Amount ),
                            Order = s.Max( a => a.Account.Order )
                        } )
                        .OrderBy( s => s.Order ) );

                mergeFields.Add(
                    "AccountSummaryNonCash",
                    transactionDetailListNonCash
                        .GroupBy( t => t.Account.PublicName )
                        .Select( s => new
                        {
                            AccountName = s.FirstOrDefault().Account.PublicName ?? s.Key,
                            Total = s.Sum( a => a.Amount ),
                            Order = s.Max( a => a.Account.Order )
                        } )
                        .OrderBy( s => s.Order ) );

                if ( options.PledgesAccountIds != null && options.PledgesAccountIds.Any() )
                {
                    var pledgeList = financialPledgeQry
                                        .Where( p => p.PersonAliasId.HasValue && personAliasIds.Contains( p.PersonAliasId.Value ) )
                                        .Include( a => a.Account )
                                        .OrderBy( a => a.Account.Order )
                                        .ThenBy( a => a.Account.PublicName )
                                        .ToList();

                    var pledgeSummaryByPledgeList = pledgeList
                                        .Select( p => new
                                        {
                                            p.Account,
                                            Pledge = p
                                        } )
                                        .ToList();

                    //// Pledges but organized by Account (in case more than one pledge goes to the same account)
                    //// NOTE: In the case of multiple pledges to the same account (just in case they accidently or intentionally had multiple pledges to the same account)
                    ////  -- Date Range
                    ////    -- StartDate: Earliest StartDate of all the pledges for that account
                    ////    -- EndDate: Lastest EndDate of all the pledges for that account
                    ////  -- Amount Pledged: Sum of all Pledges to that account
                    ////  -- Amount Given:
                    ////    --  The sum of transaction amounts to that account between
                    ////      -- Start Date: Earliest Start Date of all the pledges to that account
                    ////      -- End Date: Whatever is earlier (Statement End Date or Pledges' End Date)
                    var pledgeSummaryList = pledgeSummaryByPledgeList.GroupBy( a => a.Account ).Select( a => new PledgeSummary
                    {
                        Account = a.Key,
                        PledgeList = a.Select( x => x.Pledge ).ToList()
                    } ).ToList();

                    // add detailed pledge information
                    if ( pledgeSummaryList.Any() )
                    {
                        int statementPledgeYear = options.StartDate.Value.Year;

                        List<int> pledgeCurrencyTypeIds = null;
                        if ( options.CurrencyTypeIdsCash != null )
                        {
                            pledgeCurrencyTypeIds = options.CurrencyTypeIdsCash;
                            if ( options.PledgesIncludeNonCashGifts && options.CurrencyTypeIdsNonCash != null )
                            {
                                pledgeCurrencyTypeIds = options.CurrencyTypeIdsCash.Union( options.CurrencyTypeIdsNonCash ).ToList();
                            }
                        }

                        foreach ( var pledgeSummary in pledgeSummaryList )
                        {
                            DateTime adjustedPledgeEndDate = pledgeSummary.PledgeEndDate.Value.Date;
                            if ( pledgeSummary.PledgeEndDate.Value.Date < DateTime.MaxValue.Date )
                            {
                                adjustedPledgeEndDate = pledgeSummary.PledgeEndDate.Value.Date.AddDays( 1 );
                            }

                            if ( options.EndDate.HasValue )
                            {
                                if ( adjustedPledgeEndDate > options.EndDate.Value )
                                {
                                    adjustedPledgeEndDate = options.EndDate.Value;
                                }
                            }

                            var pledgeFinancialTransactionDetailQry = new FinancialTransactionDetailService( rockContext ).Queryable().Where( t =>
                                                             t.Transaction.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.Transaction.AuthorizedPersonAliasId.Value )
                                                             && t.Transaction.TransactionDateTime >= pledgeSummary.PledgeStartDate
                                                             && t.Transaction.TransactionDateTime < adjustedPledgeEndDate );

                            if ( options.PledgesIncludeChildAccounts )
                            {
                                // If PledgesIncludeChildAccounts = true, we'll include transactions to those child accounts as part of the pledge (but only one level deep)
                                pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t =>
                                    t.AccountId == pledgeSummary.AccountId
                                    ||
                                    ( t.Account.ParentAccountId.HasValue && t.Account.ParentAccountId == pledgeSummary.AccountId )
                                );
                            }
                            else
                            {
                                pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t => t.AccountId == pledgeSummary.AccountId );
                            }

                            if ( pledgeCurrencyTypeIds != null )
                            {
                                pledgeFinancialTransactionDetailQry = pledgeFinancialTransactionDetailQry.Where( t =>
                                    t.Transaction.FinancialPaymentDetailId.HasValue &&
                                    t.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.HasValue && pledgeCurrencyTypeIds.Contains( t.Transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value ) );
                            }

                            pledgeSummary.AmountGiven = pledgeFinancialTransactionDetailQry.Sum( t => ( decimal? ) t.Amount ) ?? 0;
                        }
                    }

                    // Pledges ( organized by each Account in case an account is used by more than one pledge )
                    mergeFields.Add( "Pledges", pledgeSummaryList );
                }

                mergeFields.Add( "Options", options );

                result.Html = lavaTemplateLava.ResolveMergeFields( mergeFields, currentPerson );
                if ( !string.IsNullOrEmpty( lavaTemplateFooterLava ) )
                {
                    result.FooterHtml = lavaTemplateFooterLava.ResolveMergeFields( mergeFields, currentPerson );
                }

                result.Html = result.Html.Trim();
            }

            return result;
        }

        /// <summary>
        /// Gets the giving statement HTML.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="year">The year.</param>
        /// <param name="templateDefinedValueId">The template defined value identifier.</param>
        /// <param name="hideRefundedTransactions">if set to <c>true</c> [hide refunded transactions].</param>
        /// <param name="currentPerson">The current person.</param>
        /// <returns></returns>
        [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
        [RockObsolete( "1.12.4" )]
        internal static string GetGivingStatementHTML( int personId, int? year, int? templateDefinedValueId, bool hideRefundedTransactions, Person currentPerson )
        {
            // Assume the current year if no year is specified
            var currentYear = RockDateTime.Now.Year;
            year = year ?? currentYear;
            var isCurrentYear = year == currentYear;
            var startDate = new DateTime( year.Value, 1, 1 );
            var endDate = isCurrentYear ? RockDateTime.Now : new DateTime( year.Value + 1, 1, 1 );

            // Load the statement lava defined type
            var definedTypeCache = DefinedTypeCache.Get( SystemGuid.DefinedType.STATEMENT_GENERATOR_LAVA_TEMPLATE );
            if ( definedTypeCache == null )
            {
                throw new Exception( "The defined type 'Statement Generator Lava Template' could not be found." );
            }

            // Get the specified defined value or the default if none is specified
            var templateValues = definedTypeCache.DefinedValues;
            var templateValue = templateDefinedValueId.HasValue ?
                templateValues.FirstOrDefault( dv => dv.Id == templateDefinedValueId.Value ) :
                templateValues.OrderBy( dv => dv.Order ).FirstOrDefault();

            if ( templateValue == null )
            {
                throw new Exception( string.Format(
                    "The defined value '{0}' within 'Statement Generator Lava Template' could not be found.",
                    templateDefinedValueId.HasValue ?
                        templateDefinedValueId.Value.ToString() :
                        "default" ) );
            }

            // Declare the necessary services
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            // Get the family ID
            var person = personService.Get( personId );
            if ( person == null )
            {
                throw new Exception( string.Format( "The person with ID {0} could not be found", personId ) );
            }

            if ( !person.PrimaryFamilyId.HasValue )
            {
                throw new Exception( string.Format( "The person with ID {0} does not have a primary family ID", personId ) );
            }

            // Build the options for the generator
            var options = new StatementGeneratorOptions
            {
                EndDate = endDate,
                LayoutDefinedValueGuid = templateValue.Guid,
                StartDate = startDate,
                ExcludeOptedOutIndividuals = false,
                HideRefundedTransactions = hideRefundedTransactions
            };

            // Get the generator result
            var result = GenerateStatementGeneratorRecipientResult( person.PrimaryFamilyId.Value, null, null, currentPerson, options );

            return result.Html;
        }

        /// <summary>
        /// Gets the financial pledge query.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="usePersonFilters">if set to <c>true</c> [use person filters].</param>
        /// <returns></returns>
        [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
        [RockObsolete( "1.12.4" )]
        private static IQueryable<FinancialPledge> GetFinancialPledgeQuery( StatementGeneratorOptions options, RockContext rockContext, bool usePersonFilters )
        {
            // pledge information
            var pledgeQry = new FinancialPledgeService( rockContext ).Queryable();

            // only include pledges that started *before* the enddate of the statement ( we don't want pledges that start AFTER the statement end date )
            if ( options.EndDate.HasValue )
            {
                pledgeQry = pledgeQry.Where( p => p.StartDate < options.EndDate.Value );
            }

            // also only include pledges that ended *after* the statement start date ( we don't want pledges that ended BEFORE the statement start date )
            pledgeQry = pledgeQry.Where( p => p.EndDate >= options.StartDate.Value );

            // Filter to specified AccountIds (if specified)
            if ( options.PledgesAccountIds == null || !options.PledgesAccountIds.Any() )
            {
                // if no PledgeAccountIds where specified, don't include any pledges
                pledgeQry = pledgeQry.Where( a => false );
            }
            else
            {
                // NOTE: Only get the Pledges that were specifically pledged to the selected accounts
                // If the PledgesIncludeChildAccounts = true, we'll include transactions to those child accounts as part of the pledge (but only one level deep)
                var selectedAccountIds = options.PledgesAccountIds;
                pledgeQry = pledgeQry.Where( a => a.AccountId.HasValue && selectedAccountIds.Contains( a.AccountId.Value ) );
            }

            if ( usePersonFilters )
            {
                if ( options.PersonId.HasValue )
                {
                    // If PersonId is specified, then this statement is for a specific person, so don't do any other filtering
                    string personGivingId = new PersonService( rockContext ).Queryable().Where( a => a.Id == options.PersonId.Value ).Select( a => a.GivingId ).FirstOrDefault();
                    if ( personGivingId != null )
                    {
                        pledgeQry = pledgeQry.Where( a => a.PersonAlias.Person.GivingId == personGivingId );
                    }
                    else
                    {
                        // shouldn't happen, but just in case person doesn't exist
                        pledgeQry = pledgeQry.Where( a => false );
                    }
                }
                else
                {
                    // unless we are using a DataView for filtering, filter based on the IncludeBusiness and ExcludeInActiveIndividuals options
                    if ( !options.DataViewId.HasValue )
                    {
                        if ( !options.IncludeBusinesses )
                        {
                            int recordTypeValueIdPerson = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                            pledgeQry = pledgeQry.Where( a => a.PersonAlias.Person.RecordTypeValueId == recordTypeValueIdPerson );
                        }

                        if ( options.ExcludeInActiveIndividuals )
                        {
                            int recordStatusValueIdActive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

                            // If the ExcludeInActiveIndividuals is enabled, don't include Pledges from Inactive Individuals, but only if they give as individuals.
                            // Pledges from Giving Groups should always be included regardless of the ExcludeInActiveIndividuals option.
                            // See https://app.asana.com/0/0/1200512694724254/f
                            pledgeQry = pledgeQry.Where( a => ( a.PersonAlias.Person.RecordStatusValueId == recordStatusValueIdActive ) || a.PersonAlias.Person.GivingGroupId.HasValue );
                        }

                        /* 06/23/2021 MDP
                         * Don't exclude pledges from Deceased. If the person pledged during the specified Date/Time range (probably while they weren't deceased), include them regardless of Deceased Status.
                         * 
                         * see https://app.asana.com/0/0/1200512694724244/f
                         */
                    }
                }
            }

            return pledgeQry;
        }

        /// <summary>
        /// Gets the financial transaction query.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="usePersonFilters">if set to <c>true</c> [use person filters].</param>
        /// <returns></returns>
        [Obsolete( "Use ~/api/FinancialGivingStatement/ endpoints instead " )]
        [RockObsolete( "1.12.4" )]
        private static IQueryable<FinancialTransaction> GetFinancialTransactionQuery( StatementGeneratorOptions options, RockContext rockContext, bool usePersonFilters )
        {
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var financialTransactionQry = financialTransactionService.Queryable();

            // filter to specified date range
            financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDateTime >= options.StartDate );

            if ( options.EndDate.HasValue )
            {
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDateTime < options.EndDate.Value );
            }

            // default to Contributions if nothing specified
            var transactionTypeContribution = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid() );
            if ( options.TransactionTypeIds == null || !options.TransactionTypeIds.Any() )
            {
                options.TransactionTypeIds = new List<int>();
                if ( transactionTypeContribution != null )
                {
                    options.TransactionTypeIds.Add( transactionTypeContribution.Id );
                }
            }

            if ( options.TransactionTypeIds.Count() == 1 )
            {
                int selectedTransactionTypeId = options.TransactionTypeIds[0];
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionTypeValueId == selectedTransactionTypeId );
            }
            else
            {
                financialTransactionQry = financialTransactionQry.Where( a => options.TransactionTypeIds.Contains( a.TransactionTypeValueId ) );
            }

            // Filter to specified AccountIds (if specified)
            if ( options.TransactionAccountIds == null )
            {
                // if TransactionAccountIds wasn't supplied, don't filter on AccountId
            }
            else
            {
                // narrow it down to recipients that have transactions involving any of the AccountIds
                var selectedAccountIds = options.TransactionAccountIds;
                financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDetails.Any( x => selectedAccountIds.Contains( x.AccountId ) ) );
            }

            if ( usePersonFilters )
            {
                if ( options.PersonId.HasValue )
                {
                    // If PersonId is specified, then this statement is for a specific person, so don't do any other filtering
                    string personGivingId = new PersonService( rockContext ).Queryable().Where( a => a.Id == options.PersonId.Value ).Select( a => a.GivingId ).FirstOrDefault();
                    if ( personGivingId != null )
                    {
                        financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.Person.GivingId == personGivingId );
                    }
                    else
                    {
                        // shouldn't happen, but just in case person doesn't exist
                        financialTransactionQry = financialTransactionQry.Where( a => false );
                    }
                }
                else
                {
                    // unless we are using a DataView for filtering, filter based on the IncludeBusiness and ExcludeInActiveIndividuals options
                    if ( !options.DataViewId.HasValue )
                    {
                        if ( !options.IncludeBusinesses )
                        {
                            int recordTypeValueIdPerson = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                            financialTransactionQry = financialTransactionQry.Where( a => a.AuthorizedPersonAlias.Person.RecordTypeValueId == recordTypeValueIdPerson );
                        }

                        if ( options.ExcludeInActiveIndividuals )
                        {
                            int recordStatusValueIdActive = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

                            // If the ExcludeInActiveIndividuals is enabled, don't include transactions from Inactive Individuals, but only if they give as individuals.
                            // Transactions from Giving Groups should always be included regardless of the ExcludeInActiveIndividuals option.
                            // See https://app.asana.com/0/0/1200512694724254/f
                            financialTransactionQry = financialTransactionQry.Where( a => ( a.AuthorizedPersonAlias.Person.RecordStatusValueId == recordStatusValueIdActive ) || a.AuthorizedPersonAlias.Person.GivingGroupId.HasValue );
                        }
                    }

                    /* 06/23/2021 MDP
                      Don't exclude transactions from Deceased. If the person gave doing the specified Date/Time
                      range (probably while they weren't deceased), include them regardless of Deceased Status.

                      see https://app.asana.com/0/0/1200512694724244/f
                    */
                }
            }

            return financialTransactionQry;
        }
    }
}
