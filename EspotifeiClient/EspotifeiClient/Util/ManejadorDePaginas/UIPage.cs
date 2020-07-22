using System.Windows.Controls;

namespace EspotifeiClient.Util.ManejadorDePaginas
{
    public abstract class UIPage : Page, IPageListener
    {
        protected IPageManager PageManager;
        
        public void SetPageManager(IPageManager pageManager)
        {
            PageManager = pageManager;
        }
        
        
    }
}