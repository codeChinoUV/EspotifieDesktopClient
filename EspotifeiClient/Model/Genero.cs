using System;
using Newtonsoft.Json;

namespace Model
{
    [JsonObject]
    [Serializable]
    public class Genero
    {
        public int id { get; set; }
        public string genero { get; set; }
        public bool seleccionado { get; set; }
    }
}