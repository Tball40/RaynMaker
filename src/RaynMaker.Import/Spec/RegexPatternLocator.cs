﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using Plainion.Collections;

namespace RaynMaker.Import.Spec
{
    public class RegexPatternLocator : ICellLocator
    {
        public RegexPatternLocator( int seriesToScan, string value )
            : this( seriesToScan, new Regex( value ) )
        {
        }

        public RegexPatternLocator( int seriesToScan, Regex value )
        {
            SeriesToScan = seriesToScan;
            Pattern = value;
        }

        public int SeriesToScan { get; private set; }

        public Regex Pattern { get; private set; }

        public int GetLocation( IEnumerable<string> list )
        {
            return list.IndexOf( item => Pattern.IsMatch( item ) );
        }
    }
}
