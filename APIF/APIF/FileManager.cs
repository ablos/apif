using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/*
 * Try saving in binary
 * Try saving in XML
 * Try saving in JSON
 */

namespace APIF
{
    public static class FileManager
    {
        public static void SaveFileBinary<T>(string path, T objectToSave, bool append = false)
        {
            using (Stream stream = File.Open(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, objectToSave);
            }
        }

        public static T LoadFileBinary<T>(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
