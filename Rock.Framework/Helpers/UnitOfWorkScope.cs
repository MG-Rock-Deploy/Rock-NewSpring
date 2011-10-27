﻿using System;
using System.Data.Objects;
using System.Data.Entity;
using System.Threading;

using Rock.Models;
using Rock.EntityFramework;

namespace Rock.Helpers
{
    /// <summary>
    /// Class used when services need to share the same DbContext
    /// </summary>
    public class UnitOfWorkScope : IDisposable
    {
        [ThreadStatic]
        private static UnitOfWorkScope currentScope;
        public readonly DbContext objectContext;
        private bool isDisposed;

        public bool SaveAllChangesAtScopeEnd { get; set; }

        internal static DbContext CurrentObjectContext
        {
            get { return currentScope != null ? currentScope.objectContext : null; }
        }

        public UnitOfWorkScope() : this( false ) { }

        public UnitOfWorkScope( bool saveAllChangesAtScopeEnd )
        {
            if ( currentScope != null && !currentScope.isDisposed )
                throw new InvalidOperationException( "ObjectContextScope instances can not be nested" );

            SaveAllChangesAtScopeEnd = saveAllChangesAtScopeEnd;
            objectContext = new Rock.EntityFramework.RockContext();
            isDisposed = false;
            //Thread.BeginThreadAffinity();  --Not supported with Medium Trust
            currentScope = this;
        }

        public void Dispose()
        {
            if ( !isDisposed )
            {
                currentScope = null;
                //Thread.EndThreadAffinity();  -- Not supported with Medium Trust

                if ( SaveAllChangesAtScopeEnd )
                {
                    objectContext.SaveChanges();
                }

                objectContext.Dispose();
                isDisposed = true;
            }
        }
    }
}
