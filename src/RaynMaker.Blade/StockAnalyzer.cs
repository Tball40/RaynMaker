﻿using System;
using System.IO;
using RaynMaker.Blade.AnalysisSpec;
using RaynMaker.Blade.DataSheetSpec;
using RaynMaker.Blade.Engine;

namespace RaynMaker.Blade
{
    class StockAnalyzer
    {
        private Analysis myAnalysis;
        private TextWriter myWriter;

        public StockAnalyzer( Analysis analysis, TextWriter writer )
        {
            myAnalysis = analysis;
            myWriter = writer;
        }

        public void Execute( Stock stock )
        {
            Console.WriteLine( "Analyzing: {0} - Isin: {1}", stock.Name, stock.Isin );
            Console.WriteLine();

            var context = new ReportContext( stock, Console.Out );
            foreach( var element in myAnalysis.Elements )
            {
                element.Report( context );
                context.Out.WriteLine();
            }
        }
    }
}
