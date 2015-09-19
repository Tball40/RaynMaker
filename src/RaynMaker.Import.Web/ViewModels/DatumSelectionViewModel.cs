﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Prism.Mvvm;
using Plainion;
using RaynMaker.Entities;
using RaynMaker.Entities.Datums;
using RaynMaker.Import.Spec;
using RaynMaker.Import.Web.Model;

namespace RaynMaker.Import.Web.ViewModels
{
    // TODO: we do not support add, remove and edit of datums as they are currently fixed by entities model.
    // TODO: "Standing" datums also exists
    class DatumSelectionViewModel : BindableBase
    {
        private Session mySession;
        private Type mySelectedDatum;

        public DatumSelectionViewModel( Session session )
        {
            Contract.RequiresNotNull( session, "session" );

            mySession = session;

            PropertyChangedEventManager.AddHandler( mySession, OnCurrentLocatorChanged, PropertySupport.ExtractPropertyName( () => mySession.CurrentLocator ) );

            Datums = Dynamics.AllDatums
                .OrderBy( d => d.Name )
                .ToList();

            OnCurrentLocatorChanged( null, null );
        }

        private void OnCurrentLocatorChanged( object sender, PropertyChangedEventArgs e )
        {
            if( mySession.CurrentLocator != null )
            {
                SelectedDatum = Datums.FirstOrDefault( d => d.Name == mySession.CurrentLocator.Datum );
            }
            else
            {
                SelectedDatum = typeof( Price );
            }
        }
        
        public IEnumerable<Type> Datums { get; private set; }

        public Type SelectedDatum
        {
            get { return mySelectedDatum; }
            set
            {
                if( SetProperty( ref mySelectedDatum, value ) )
                {
                    var locator = mySession.Locators.SingleOrDefault( l => l.Datum == mySelectedDatum.Name );
                    if( locator == null )
                    {
                        locator = new DatumLocator( mySelectedDatum.Name );
                        mySession.Locators.Add( locator );
                    }
                    mySession.CurrentLocator = locator;
                }
            }
        }
    }
}
