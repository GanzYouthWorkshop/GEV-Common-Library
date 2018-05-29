using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GEV.Common
{
    /// <summary>
    /// Provides auxiliary interface for configuration classes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConfiguration<T>
    {
        void Save(string file);
    }

    /// <summary>
    /// Provides basic mechanisms for loading and saving XML-based configuration files.<br/>
    /// <b>Usage:</b> <code>public class ImplementedConfig : Configuration&lt;ImplementedConfig&gt;</code>
    /// </summary>
    public abstract class Configuration<T> : IConfiguration<T> where T : IConfiguration<T>, new()
    {
        /// <summary>
        /// Loads a configuration file.
        /// </summary>
        /// <param name="file">Path fo the xml file.</param>
        /// <returns>A loaded configuration object.</returns>
        public static T Load(string file)
        {
            if (!File.Exists(file))
            {
                T config = new T();
                config.Save(file);
                return config;
            }
            else
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(T));
                    return (T)ser.Deserialize(sr);
                }
            }
        }

        /// <summary>
        /// Saves a configauration object to a xml file.
        /// </summary>
        /// <param name="file">Path of the xml file.</param>
        public void Save(string file)
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                ser.Serialize(sw, this);
            }
        }
    }

}
