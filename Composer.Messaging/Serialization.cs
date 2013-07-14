using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Composer.Messaging
{
    /// <summary>
    ///   Serialization class.
    /// </summary>
    public static class Serialization
    {
        /// <summary>
        ///   From json.
        /// </summary>
        /// <typeparam name="T">Type of object serialized.</typeparam>
        /// <param name="json">The json.</param>
        /// <returns>My new object with all its teef.</returns>
        public static T FromJson<T>(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var myObject = default(T);
            using (MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                myObject = (T)serializer.ReadObject(memStream);
            }
            return myObject;
        }

        /// <summary>
        ///   To json.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="myObject">The object.</param>
        /// <returns>Serialized information.</returns>
        public static string ToJson<T>(T myObject)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            string data = string.Empty;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, myObject);
                data = Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);
            }

            return data;
        }
    }
}
