using Android.Content;

namespace RTMPStreamerApp.Helpers
{
    public static class SettingsHelper
    {
        const string PREFS_NAME = "RTMPStreamerPrefs";
        const string KEY_CAMERA = "selected_camera";
        const string KEY_ZOOM = "zoom_level";
        const string KEY_RTMP_URL = "rtmp_url";

        public static void SaveCamera(Context context, string camera)
        {
            var prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            var editor = prefs.Edit();
            editor.PutString(KEY_CAMERA, camera);
            editor.Apply();
        }

        public static string GetSavedCamera(Context context)
        {
            var prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            return prefs.GetString(KEY_CAMERA, "Back");
        }

        public static void SaveZoom(Context context, float zoom)
        {
            var prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            var editor = prefs.Edit();
            editor.PutFloat(KEY_ZOOM, zoom);
            editor.Apply();
        }

        public static float GetSavedZoom(Context context)
        {
            var prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            return prefs.GetFloat(KEY_ZOOM, 1.0f);
        }

        public static void SaveUrl(Context context, string url)
        {
            var prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            var editor = prefs.Edit();
            editor.PutString(KEY_RTMP_URL, url);
            editor.Apply();
        }

        public static string GetSavedUrl(Context context)
        {
            var prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            return prefs.GetString(KEY_RTMP_URL, null);
        }
    }
}
