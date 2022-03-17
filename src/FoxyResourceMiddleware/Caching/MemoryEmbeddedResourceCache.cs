using System.Reflection;

namespace FoxyResourceMiddleware.Caching;

public class MemoryEmbeddedResourceCache : IEmbeddedResourceCache
{
	private static object _lock = new object();
	private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();
	private readonly string _name;
	private readonly Assembly _assembly;

	public MemoryEmbeddedResourceCache()
	{
		_assembly = Assembly.GetEntryAssembly();
		_name = _assembly.GetName().Name;
	}

	public byte[] GetResourceStream(string resourceName)
	{
		resourceName = $"{_name}.wwwroot.{resourceName.Replace("/", ".")}";
		if (_cache.ContainsKey(resourceName))
		{
			return _cache[resourceName];
		}


		var resourceStream = _assembly.GetManifestResourceStream(resourceName);
		if (resourceStream == null)
		{
			return null;
		}

		var resourceBytes = new byte[resourceStream.Length];
		resourceStream.Read(resourceBytes, 0, resourceBytes.Length);
		_cache.Add(resourceName, resourceBytes);

		return resourceBytes;
	}
}