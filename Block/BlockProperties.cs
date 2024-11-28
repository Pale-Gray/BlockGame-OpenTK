using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blockgame_OpenTK.BlockUtil
{
    internal struct BlockProperties
    {

        private Dictionary<string, object> _properties;
        public BlockProperties() { _properties = new Dictionary<string, object>(); }
        public BlockProperties(Dictionary<string, object> properties) { _properties = properties; }

        public void AddProperty<T>(string propertyName, T value)
        {

            if (!_properties.TryAdd(propertyName, value))
            {

                _properties[propertyName] = value;

            }

        }

        public void SetProperty<T>(string propertyName, T value)
        {

            _properties[propertyName] = value;

        }

        public bool TryGetProperty<T>(string propertyName, out T value)
        {

            if (_properties.TryGetValue(propertyName, out object val) && val != null)
            {

                value = (T) val;
                return true;

            }

            value = default;
            return false;

        }

        public BlockProperties Extract()
        {

            BlockProperties properties = new BlockProperties();

            foreach (KeyValuePair<string, object> property in _properties)
            {

                properties.AddProperty(property.Key, property.Value);

            }

            return properties;

        }

    }
}
