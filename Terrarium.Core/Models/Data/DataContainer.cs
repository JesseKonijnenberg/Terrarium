using System;

namespace Terrarium.Core.Models.Data
{
    public class DataContainer<T>
    {
        public int Version { get; set; } = 1;

        public DateTime LastSaved { get; set; } = DateTime.Now;

        public T Data { get; set; }

        public DataContainer() { }

        public DataContainer(T data, int version)
        {
            Data = data;
            Version = version;
        }
    }
}