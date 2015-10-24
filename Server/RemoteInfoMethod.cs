using System;
using System.Collections.Generic;
using System.IO;
using NetworkTypes;

namespace Server
{
    public class RemoteInvokeMethod
    {
        public string ServiceClassName { get; set; }
        public string MethodName { get; set; }
        public List<SerializableType> Parameters { get; set; }
        public static readonly string AssemblyName = "NetworkTypes";

        public RemoteInvokeMethod(List<SerializableType> parameters)
        {
            ServiceClassName = "Handler";
            Parameters = parameters;
        }

        public RemoteInvokeMethod(string serviceClassName, Command methodName, List<SerializableType> parameters)
        {
            ServiceClassName = serviceClassName;
            MethodName = methodName.ToString();
            Parameters = parameters;
        }

        public RemoteInvokeMethod(string serviceClassName, string methodName, List<SerializableType> parameters)
        {
            ServiceClassName = serviceClassName;
            MethodName = methodName;
            Parameters = parameters;
        }

        public RemoteInvokeMethod(Command methodName, List<SerializableType> parameters)
        {
            ServiceClassName = "Handler";
            MethodName = methodName.ToString();
            Parameters = parameters;
        }

        public static byte[] WriteToStream(RemoteInvokeMethod toWrite)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(toWrite.ServiceClassName);
            writer.Write(toWrite.MethodName);

            writer.Write(toWrite.Parameters.Count.ToString());
            foreach (var parameter in toWrite.Parameters)
            {
                var type = parameter.GetType();
                writer.Write(type.ToString());
                parameter.Serialize(writer, type, parameter);
            }
            return stream.GetBuffer();
        }

        public static RemoteInvokeMethod ReadFromStream(MemoryStream from)
        {
            var reader = new BinaryReader(from);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var className = reader.ReadString();
            var methodName = reader.ReadString();
            var count = reader.ReadString();
            var paramsCount = Convert.ToInt32(count);
            var parameters = new List<SerializableType>();
            for (var i = 0; i < paramsCount; i++)
            {
                var type = reader.ReadString() + ", " + AssemblyName;
                var objectType = Type.GetType(type);
                var item = Activator.CreateInstance(objectType) as SerializableType;
                item.Deserialize(reader, objectType, item);
                parameters.Add(item);
            }
            var instance = new RemoteInvokeMethod(className, methodName, parameters);
            return instance;
        }
    }
}
