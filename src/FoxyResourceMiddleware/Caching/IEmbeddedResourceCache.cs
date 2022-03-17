namespace FoxyResourceMiddleware.Caching;

public interface IEmbeddedResourceCache
{
	byte[] GetResourceStream(string resourceName);
}