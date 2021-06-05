using System.IO;
using YamlDotNet.Serialization;

namespace ClipInputCLI
{
    public static class YamlManager
    {
        public static T Read<T>(Stream stream)
        {
            using var r = new StreamReader(stream);
            var yaml = new Deserializer();
            return yaml.Deserialize<T>(r);
        }

        public static T Read<T>(string fileName)
        {
            using var fs = File.OpenRead(fileName);
            return Read<T>(fs);
        }

        public static void Write(Stream stream, object graph)
        {
            using var w = new StreamWriter(stream);
            var yaml = new Serializer();
            yaml.Serialize(w, graph);
        }

        public static void Write(string fileName, object graph, bool replace = false)
        {
            if (replace) File.Delete(fileName);
            using var fs = File.OpenWrite(fileName);
            Write(fs, graph);
        }
    }
}
