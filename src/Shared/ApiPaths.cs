namespace Phrazy.Shared;

public static class ApiPaths
{
	public static class Puzzle
	{
		public const string GetWithIdentifier = "/api/puzzle/{id}";
		public const string PutResult = "/api/result";
		public const string GetLastResult = "/api/getlastresult/{id}";
	}
}