﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaynMaker.Import.Spec;
using RaynMaker.Import.Spec.v2.Extraction;

namespace RaynMaker.Import.Parsers
{
    class TableFormatter
    {
        /// <summary>
        /// Tries to format the given DataTable as descripbed in the 
        /// FormatColumns. That means: types are converted into the 
        /// required types, only the described part of the raw table 
        /// is extracted.
        /// Empty rows will be removed.
        /// </summary>
        public static DataTable ToFormattedTable( AbstractTableDescriptor format, DataTable rawTable )
        {
            DataTable table = new DataTable();
            foreach( var col in format.Columns )
            {
                table.Columns.Add( col.Name, col.Type );
            }
            ToFormattedTable( format, rawTable, table );

            return table;
        }

        public static void ToFormattedTable( AbstractTableDescriptor format, DataTable rawTable, DataTable targetTable )
        {
            for( int r = 0; r < rawTable.Rows.Count; ++r )
            {
                if( format.SkipRows.Contains( r ) )
                {
                    continue;
                }

                DataRow rawRow = rawTable.Rows[ r ];
                DataRow row = targetTable.NewRow();
                int targetCol = 0;
                bool isEmpty = true;
                for( int c = 0; c < rawRow.ItemArray.Length; ++c )
                {
                    if( format.SkipColumns.Contains( c ) )
                    {
                        continue;
                    }

                    if( targetCol == format.Columns.Length )
                    {
                        break;
                    }

                    FormatColumn formatCol = format.Columns[ targetCol ];
                    object value = formatCol.Convert( rawRow[ c ].ToString() );
                    row[ formatCol.Name ] = ( value != null ? value : DBNull.Value );
                    if( row[ formatCol.Name ] != DBNull.Value )
                    {
                        isEmpty = false;
                    }

                    targetCol++;
                }

                if( !isEmpty )
                {
                    targetTable.Rows.Add( row );
                }
            }
        }

        /// <summary>
        /// Tries to format the given DataTable as descripbed in 
        /// ValueFormat and TimeAxisFormat. That means: types are 
        /// converted into the required types, the first column
        /// is treated as value column the second as timeaxis column.
        /// <remarks>Usually this method is called with a table
        /// which has been tailored using DataTable.ExtractSeries()
        /// </remarks>
        /// </summary>
        public static DataTable ToFormattedTable( AbstractSeriesDescriptor format, DataTable table_in )
        {
            if( table_in == null )
            {
                throw new ArgumentNullException( "table_in" );
            }

            DataTable rawTable = table_in;

            rawTable = ExtractSeries( format, table_in );
            if( rawTable == null )
            {
                return null;
            }

            if( format.ValueFormat == null && format.TimeFormat == null )
            {
                return rawTable;
            }

            DataTable table = new DataTable();

            if( format.ValueFormat != null )
            {
                table.Columns.Add( new DataColumn( format.ValueFormat.Name, format.ValueFormat.Type ) );
            }
            else
            {
                throw new InvalidOperationException( "No value format specified" );
            }

            if( format.TimeFormat != null )
            {
                table.Columns.Add( new DataColumn( format.TimeFormat.Name, format.TimeFormat.Type ) );
            }

            foreach( DataRow rawRow in rawTable.Rows )
            {
                DataRow row = table.NewRow();

                Action<int, FormatColumn> Convert = ( idx, col ) =>
                {
                    object value = ( col == null ? rawRow[ idx ] : col.Convert( rawRow[ idx ].ToString() ) );
                    row[ idx ] = ( value != null ? value : DBNull.Value );
                };

                Convert( 0, format.ValueFormat );
                if( format.TimeFormat != null )
                {
                    Convert( 1, format.TimeFormat );
                }

                table.Rows.Add( row );
            }

            return table;
        }

        /// <summary>
        /// SeriesName validation not enabled here.
        /// </summary>
        public static TableExtractionSettings ToExtractionSettings( AbstractSeriesDescriptor format )
        {
            TableExtractionSettings settings = new TableExtractionSettings();
            settings.Dimension = format.Orientation;
            if( settings.Dimension == SeriesOrientation.Column )
            {
                settings.ColumnHeaderRow = format.ValuesLocator.SeriesToScan;
                settings.RowHeaderColumn = format.TimesLocator.SeriesToScan;

                settings.SkipColumns = null;
                settings.SkipRows = format.Excludes;
            }
            else
            {
                settings.ColumnHeaderRow = format.TimesLocator.SeriesToScan;
                settings.RowHeaderColumn = format.ValuesLocator.SeriesToScan;

                settings.SkipColumns = format.Excludes;
                settings.SkipRows = null;
            }

            // do not enable validation here
            //settings.SeriesName = format.SeriesNameContains;

            return settings;
        }

        private static DataTable ExtractSeries( AbstractSeriesDescriptor format, DataTable rawTable )
        {
            // calculate the anchor
            Point anchor = new Point( 0, 0 );

            if( format.Orientation == SeriesOrientation.Column )
            {
                int rowToScan = format.ValuesLocator.SeriesToScan;
                if( rawTable.Rows.Count <= rowToScan )
                {
                    throw new InvalidExpressionException( "Anchor points outside table" );
                }

                anchor.X = format.ValuesLocator.GetLocation( rawTable.Rows[ rowToScan ].ItemArray.Select( item => item.ToString() ) );
                if( anchor.X == -1 )
                {
                    throw new InvalidExpressionException( "Anchor condition failed: column not found" );
                }
            }

            if( format.Orientation == SeriesOrientation.Row )
            {
                int colToScan = format.ValuesLocator.SeriesToScan;
                if( rawTable.Columns.Count <= colToScan )
                {
                    throw new InvalidExpressionException( "Anchor points outside table" );
                }

                anchor.Y = format.ValuesLocator.GetLocation( rawTable.Rows.ToSet().Select( row => row[ colToScan ].ToString() ) );
                if( anchor.Y == -1 )
                {
                    throw new InvalidExpressionException( "Anchor condition failed: row not found" );
                }
            }

            // How to get the value
            Func<object, object> GetValue = value => value;

            return rawTable.ExtractSeries( anchor, GetValue, ToExtractionSettings( format ) );
        }
    }
}
