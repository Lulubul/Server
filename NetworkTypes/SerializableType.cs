using System;
using System.IO;

namespace NetworkTypes
{
    public class SerializableType
    {
        private BinaryWriter _binaryWriter;

        public void Serialize(BinaryWriter writer, Type type)
        {
            _binaryWriter = writer;
            foreach (var propertyInfo in type.GetProperties())
            {
                WriteList(propertyInfo.GetValue(this, null));
            }
        }

        public void Deserialize(BinaryReader reader, Type type)
        {
            foreach (var propertyInfo in type.GetProperties())
            {
                var methodName = "Read" + propertyInfo.PropertyType.ToString().Split('.')[1];
                var value = typeof(BinaryReader).GetMethod(methodName).Invoke(reader, null);
                propertyInfo.SetValue(this, value, null);
            }
        }

        public void WriteList (object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    _binaryWriter.Write((bool)(object)value);
                    break;
                case TypeCode.Byte:
                    _binaryWriter.Write((byte)(object)value);
                    break;
                case TypeCode.Char:
                    _binaryWriter.Write((char)(object)value);
                    break;
                case TypeCode.Decimal:
                    _binaryWriter.Write((decimal)(object)value);
                    break;
                case TypeCode.Double:
                    _binaryWriter.Write((double)(object)value);
                    break;
                case TypeCode.Single:
                    _binaryWriter.Write((float)(object)value);
                    break;
                case TypeCode.Int16:
                    _binaryWriter.Write((short)(object)value);
                    break;
                case TypeCode.Int32:
                    _binaryWriter.Write((int)(object)value);
                    break;
                case TypeCode.Int64:
                    _binaryWriter.Write((short)(object)value);
                    break;
                case TypeCode.String:
                    _binaryWriter.Write((string)(object)value);
                    break;
                case TypeCode.SByte:
                    _binaryWriter.Write((sbyte)(object)value);
                    break;
                default:
                    throw new ArgumentException("List type not supported");
            }
        }
    }
}
