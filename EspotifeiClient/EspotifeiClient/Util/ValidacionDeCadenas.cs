using System;
using System.Text.RegularExpressions;

namespace EspotifeiClient.Util
{
    public class ValidacionDeCadenas
    {

        /// <summary>
        /// Valida que la longitud de la cadena se encuentre entre el tamaño minimo y el tamaño maximo    
        /// </summary>
        /// <param name="cadena">La cadena a validar</param>
        /// <param name="tamañoMinimo">El tamaño minimo que debe de tener la cadena</param>
        /// <param name="tamañoMaximo">El tamaño maximo que debe de tener la cadena</param>
        /// <returns>True si la cadena se encuentra entre los limites o False si no</returns>
        public static bool ValidarTamañoDeCadena(string cadena, int tamañoMinimo, int tamañoMaximo)
        {
            return cadena.Length >= tamañoMinimo && cadena.Length <= tamañoMaximo;
        }

        /// <summary>
        /// Valida si una cadena es alfanumerica
        /// </summary>
        /// <param name="cadena">La cadena a validar</param>
        /// <returns>True si la cadena es alfanumerica o false si no</returns>
        public static bool ValidarCadenaEsAlfanumerica(string cadena)
        {
            var esAlfanumerica = true;
            foreach (var caracter in cadena)
            {
                if (!Char.IsLetterOrDigit(caracter))
                {
                    esAlfanumerica = false;
                    break;
                }
            }

            return esAlfanumerica;
        }

        /// <summary>
        /// Valida si la cadena cumple con la expresion regular para las contraseñas
        /// </summary>
        /// <param name="cadena">La cadena a validar</param>
        /// <returns>True si la cadena cumple con la expresion regular para contraseñas o false si no</returns>
        public static bool ValidarContraseña(string cadena)
        {
            return Regex.IsMatch(cadena, @"^(?=\w*\d)(?=\w*[A-Z])(?=\w*[a-z])\S{8,16}$");
        }

        /// <summary>
        /// Valida si la cadena cumple con la expresion regular para correos electronicos
        /// </summary>
        /// <param name="cadena">La cadena a validar</param>
        /// <returns>True si la cadena cumple con la expresion regular para correos o false si no</returns>
        public static bool ValidarCorreoElectronico(string cadena)
        {
            return Regex.IsMatch(cadena, @"^[a-zA-Z0-9.!#$%&\'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$");
        }
    }
}