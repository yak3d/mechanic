namespace Mechanic.Core.Tests.Utils;

public static class VdfTestUtils
{
    public static string BuildSimpleLibraryFolder(string path)
    {
	    var escapedPath = path.Replace("\\", "\\\\");
        return $@"""libraryfolders""
                     {{
                     	""0""
                     	{{
                     		""path""		""{escapedPath}""
                     		""label""		""""
                     		""contentid""		""6217296288220088616""
                     		""totalsize""		""0""
                     		""update_clean_bytes_tally""		""2147846680""
                     		""time_last_update_verified""		""1750703811""
                     		""apps""
                     		{{
                     			""12345""		""505189673""
                     		}}
                     	}}
                     }}
                     """"";
    }
    
    public static string BuildFullAppState(
        string appId,
        string name,
        string installDir
        )
    {
        return $@"""AppState""
                 {{
                 	""appid""		""{appId}""
                 	""name""		""{name}""
                 	""installdir""		""{installDir}""
                 	""LastUpdated""		""1640995200""
                 }}
                 """"";
    }
}