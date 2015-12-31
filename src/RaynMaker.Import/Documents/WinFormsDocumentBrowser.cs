using System;
using System.Windows.Forms;
using Plainion;
using RaynMaker.Import.Documents.WinForms;
using RaynMaker.Import.Spec;
using RaynMaker.Import.Spec.v2;
using RaynMaker.Import.Spec.v2.Locating;
using RaynMaker.Import.WinForms;

namespace RaynMaker.Import.Documents
{
    class WinFormsDocumentBrowser : IDisposable, IDocumentBrowser
    {
        private INavigator myNavigator;

        public WinFormsDocumentBrowser( INavigator navigator, SafeWebBrowser webBrowser )
        {
            myNavigator = navigator;
            // we have to send Navigating event also if user interacts with the real WebBrowser.
            // in order to not send event twice we ignore the events from Navigator
            //myNavigator.Navigating += OnNavigating;

            Browser = webBrowser;
            Browser.Navigating += WebBrowser_Navigating;
            Browser.DocumentCompleted += WebBrowser_DocumentCompleted;
        }

        public void Dispose()
        {
            if( myNavigator != null )
            {
                var disposable = myNavigator as IDisposable;
                if( disposable != null )
                {
                    disposable.Dispose();
                }

                myNavigator = null;
            }
            
            if( Browser != null )
            {
                Browser.Navigating -= WebBrowser_Navigating;
                Browser.DocumentCompleted -= WebBrowser_DocumentCompleted;

                Browser = null;
            }
        }

        public SafeWebBrowser Browser { get; private set; }

        public IDocument Document { get; private set; }

        public void Navigate( DocumentType docType, Uri url )
        {
            Contract.Requires( docType != DocumentType.None, "DocumentType must not be None" );
            Contract.RequiresNotNull( url, "url" );

            var loader = DocumentLoaderFactory.CreateLoader( docType, Browser );
            Document = loader.Load( url );
        }

        public void Navigate( DocumentType docType, DocumentLocator navi )
        {
            Contract.RequiresNotNull( navi, "navi" );

            var url = myNavigator.Navigate( navi );
            if( url == null )
            {
                return;
            }

            var loader = DocumentLoaderFactory.CreateLoader( docType, Browser );
            Document = loader.Load( url );
        }

        private void WebBrowser_Navigating( object sender, WebBrowserNavigatingEventArgs e )
        {
            if( Browser == null )
            {
                // already disposed
                return;
            }

            if( Navigating != null )
            {
                Navigating( e.Url );
            }
        }
        
        private void WebBrowser_DocumentCompleted( object sender, WebBrowserDocumentCompletedEventArgs e )
        {
            Document = new HtmlDocumentAdapter( Browser.Document );

            if( DocumentCompleted != null )
            {
                DocumentCompleted( Document );
            }
        }

        public event Action<Uri> Navigating;

        public event Action<IDocument> DocumentCompleted;

        public void ClearCache()
        {
            var cache = myNavigator as ICache;
            if( cache != null )
            {
                cache.Clear();
            }
        }
    }
}
