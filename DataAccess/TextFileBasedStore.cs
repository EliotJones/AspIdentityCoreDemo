namespace EliotJones.AspIdentity.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public class TextFileBasedStore : IDataContext
    {
        private readonly Lazy<string> dataFolderPath = new Lazy<string>(() => Path.Combine(AppContext.BaseDirectory, "DataStore")); 

        public async Task CreateAsync<T>(T item) where T : class
        {
            EnsureDirectoryPresent();

            var location = GetFileLocation(item);

            using (var sw = File.CreateText(location))
            {
                var content = GetFileContent(item);

                await sw.WriteAsync(content);
            }
        }

        public Task DeleteAsync<T>(T item) where T : class
        {
            EnsureDirectoryPresent();

            var location = GetFileLocation(item);

            if (File.Exists(location))
            {
                File.Delete(location);
            }

            return Task.CompletedTask;
        }

        public async Task<T> GetByIdAsync<T>(Guid id) where T : class
        {
            EnsureDirectoryPresent();

            var location = Path.Combine(dataFolderPath.Value, $"{id}.usr");

            if (!File.Exists(location))
            {
                return null;
            }

            string text;
            
            using (var read = File.OpenRead(location))
            {
                var result = new byte[read.Length];
                await read.ReadAsync(result, 0, (int)read.Length);
            
                text = Encoding.UTF8.GetString(result);
            }

            return ReadFileContent<T>(text);
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync<T>() where T : class
        {
            EnsureDirectoryPresent();

            var files = Directory.EnumerateFiles(dataFolderPath.Value, "*.usr");

            var results = new List<T>();

            foreach (var file in files)
            {
                string text;

                using (var reader = File.OpenRead(file))
                {
                    var result = new byte[reader.Length];
                    await reader.ReadAsync(result, 0, (int)reader.Length);

                    text = Encoding.UTF8.GetString(result);
                }

                results.Add(ReadFileContent<T>(text));
            }

            return results;
        }

        public async Task UpdateAsync<T>(T item) where T : class
        {
            EnsureDirectoryPresent();

            var location = GetFileLocation(item);

            if (!File.Exists(location))
            {
                await CreateAsync(item);
            }

            var text = GetFileContent(item);

            using (var writer = File.OpenWrite(location))
            {
                var bytes = Encoding.UTF8.GetBytes(text);

                await writer.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        private static string GetFileContent<T>(T item)
        {
            var properties = typeof(T).GetTypeInfo().GetProperties();

            StringBuilder builder = new StringBuilder();

            foreach (var property in properties)
            {
                var value = property.GetMethod.Invoke(item, null);

                builder.Append(value?.ToString() ?? "NULL").Append('\t');
            }

            return builder.ToString();
        }

        private static T ReadFileContent<T>(string content) where T : class
        {
            var parts = content.Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);

            var properties = typeof(T).GetTypeInfo().GetProperties();

            var instance = Activator.CreateInstance(typeof(T));

            if (parts.Length != properties.Length)
            {
                throw new InvalidOperationException("The numbers of properties and parts in the string did not match.");
            }

            for (int i = 0; i < properties.Length; i++)
            {
                var str = parts[i];

                object value = str == "NULL" ? null : Parse(str, properties[i]);

                properties[i].GetSetMethod().Invoke(instance, new[] {value});
            }

            return (T) instance;
        }

        private static object Parse(string str, PropertyInfo property)
        {
            if (property.PropertyType == typeof(string))
            {
                return str;
            }

            if (property.PropertyType == typeof(Guid))
            {
                return Guid.Parse(str);
            }

            if (property.PropertyType == typeof(int))
            {
                return int.Parse(str);
            }

            throw new NotSupportedException("Cannot convert the type: " + property.PropertyType);
        }

        private string GetFileLocation(dynamic item)
        {
            return Path.Combine(dataFolderPath.Value, $"{item.Id}.usr");
        }

        private void EnsureDirectoryPresent()
        {
            if (!Directory.Exists(dataFolderPath.Value))
            {
                Directory.CreateDirectory(dataFolderPath.Value);
            }
        }
    }
}