/*NOTE FROM TWITCH:
 An application must request only the scopes required by the APIs that their app calls.
 If you request more scopes than is required to support your app�s functionality, Twitch may suspend your application�s access to the Twitch API.

 To see what each scope does, visit:
 https://dev.twitch.tv/docs/authentication/scopes
 If the list of scopes for some reason is outdated you can add more scopes manually by following the same syntax.
 Should you notice the list being outdated, please, do contact VonRiddarn.
 */

using System.Collections.Generic;

namespace SotnKhaosTools.Services.TwitchImplicitOAuth
{
	public static class Scopes
	{
		public static string[] GetScopes()
		{
			List<string> s = new List<string>();

			s.Add("channel:manage:redemptions");
			s.Add("channel:read:redemptions");
			s.Add("channel:read:subscriptions");
			s.Add("bits:read");

			return s.ToArray();
		}
	}
}