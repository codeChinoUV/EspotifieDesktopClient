﻿ namespace EspotifeiClient.Util.ManejadorDePaginas
{
    /// <summary>
    ///     Interfaz que habilita la navegabilidad entre páginas. Es obligatorio declarar un atributo IPageManager en
    ///     las pantallas que implementen esta interfaz.
    /// </summary>
    public interface IPageListener
    {
        /// <summary>
        ///     Se encarga de proveer la interfaz IPageManager a todas las pantallas para que puedan realizar
        ///     navegabilidad en la aplicación.
        /// </summary>
        /// <param name="pageManager">Interfaz de PageManager</param>
        void SetPageManager(IPageManager pageManager);
    }
}