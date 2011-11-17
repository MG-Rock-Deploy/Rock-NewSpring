﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

using Rock.Repository;

//using Rock.Models;
//using Rock.Models.Core;
//using Rock.Services.Core;

namespace Rock.Services
{
    public class Service<T> where T : Rock.Models.Model<T>
    {
        private IRepository<T> _repository;
        protected IRepository<T> Repository 
        {
            get { return _repository; }
        }

        public Service()
			: this( new EntityRepository<T>() )
        { }

        public Service( IRepository<T> Repository )
        {
            _repository = Repository;
        }

        public IQueryable<T> Queryable()
        {
            return _repository.AsQueryable();
        }

        public T Get( int id )
        {
            return _repository.FirstOrDefault( t => t.Id == id );
        }

        public bool Add( T item, int? personId )
        {
            if ( item.Guid == Guid.Empty )
                item.Guid = Guid.NewGuid();

            bool cancel = false;
            item.RaiseAddingEvent( out cancel, personId );
            if ( !cancel )
            {
                _repository.Add( item );
                return true;
            }
            else
                return false;
        }

        public void Attach( T item )
        {
            _repository.Attach( item );
        }

        public bool Delete( T item, int? personId  )
        {
            bool cancel = false;
            item.RaiseAddingEvent( out cancel, personId );
            if ( !cancel )
            {
                _repository.Delete( item );
                return true;
            }
            else
                return false;
        }

        public void Save( T item, int? personId )
        {
            List<Rock.Models.Core.EntityChange> entityChanges = _repository.Save( personId );

            if ( entityChanges != null && entityChanges.Count > 0 )
            {
                Rock.Services.Core.EntityChangeService entityChangeService = new Rock.Services.Core.EntityChangeService();
                foreach ( Rock.Models.Core.EntityChange entityChange in entityChanges )
                {
                    entityChangeService.Add( entityChange, personId );
                    entityChangeService.Save( entityChange, personId );
                }
            }
        }

        public void Reorder( List<T> items, int oldIndex, int newIndex, int? personId )
        {
            T movedItem = items[oldIndex];
            items.RemoveAt( oldIndex );
            if ( newIndex >= items.Count )
                items.Add( movedItem );
            else
                items.Insert( newIndex, movedItem );

            int order = 0;
            foreach ( T item in items )
            {
                Rock.Models.IOrdered orderedItem = item as Rock.Models.IOrdered;
                if ( orderedItem != null )
                {
                    if ( orderedItem.Order != order )
                    {
                        orderedItem.Order = order;
                        Save( item, personId );
                    }
                }
                order++;
            }
        }

        #region Attributes

        /// <summary>
        /// Load the attributes associated with a specific instance of a model
        /// </summary>
        /// <param name="model"></param>
        public void LoadAttributes( T item )
        {
            var model = item as Rock.Models.ModelWithAttributes<T>;
            if ( model != null )
            {
                List<Rock.Models.Core.Attribute> attributes = new List<Rock.Models.Core.Attribute>();
                Dictionary<string, KeyValuePair<string, string>> attributeValues = new Dictionary<string, KeyValuePair<string, string>>();

                Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

                Type modelType = model.GetType().BaseType;
                string entityType = modelType.FullName;

                foreach ( PropertyInfo propertyInfo in modelType.GetProperties() )
                    properties.Add( propertyInfo.Name.ToLower(), propertyInfo );

                Rock.Services.Core.AttributeService attributeService = new Rock.Services.Core.AttributeService();

                foreach ( Rock.Models.Core.Attribute attribute in attributeService.GetByEntity( entityType ) )
                {
                    if ( string.IsNullOrEmpty( attribute.EntityQualifier ) ||
                        ( properties.ContainsKey( attribute.EntityQualifier.ToLower() ) &&
                        ( !attribute.EntityQualifierId.HasValue ||
                        ( int )properties[attribute.EntityQualifier.ToLower()].GetValue( model, null ) == attribute.EntityQualifierId.Value ) ) )
                    {
                        attributes.Add( attribute );
                        attributeValues.Add( attribute.Key, new KeyValuePair<string, string>( attribute.Name, attribute.GetValue( model.Id ) ) );
                    }
                }

                model.Attributes = attributes;
                model.AttributeValues = attributeValues;
            }
        }

        /// <summary>
        /// Save new values for a particular attribute and model instance
        /// </summary>
        /// <param name="model">Any ModelWithAttributes model</param>
        /// <param name="attribute">Attribute to update values for</param>
        /// <param name="values">A Dictionary of updated values.  The attribute's fieldtype object's ReadEdit() method returns this dictionary of values</param>
        public void SaveAttributeValue( Rock.Models.ModelWithAttributes<T> model, Rock.Models.Core.Attribute attribute, string value, int? personId )
        {
            Rock.Services.Core.AttributeValueService attributeValueService = new Rock.Services.Core.AttributeValueService();

            Rock.Models.Core.AttributeValue attributeValue = attribute.AttributeValues.Where( v => v.EntityId == model.Id ).FirstOrDefault();

            if ( attributeValue == null )
            {
                attributeValue = new Rock.Models.Core.AttributeValue();
                attributeValueService.Add( attributeValue, personId );
            }

            attributeValue.AttributeId = attribute.Id;
            attributeValue.EntityId = model.Id;
            attributeValue.Value = value;

            attributeValueService.Save( attributeValue, personId );

            model.AttributeValues[attribute.Key] = new KeyValuePair<string, string>( attribute.Name, value );
        }

        #endregion

    }

}