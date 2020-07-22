using System.Windows;
using System.Windows.Controls;

namespace EspotifeiClient.Util.ManejadorDePaginas
{
    public class UIWindow : Window, IPageManager
    {

        public T ChangePage<T>() where T : UIPage, IPageListener, new()
        {
            var page = new T();
            page.SetPageManager(this);
            Content = page;
            return page;
        }
        
    }
}