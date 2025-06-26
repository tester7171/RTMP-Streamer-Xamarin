using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using System.Threading.Tasks;

namespace RTMPStreamerApp.Services
{
    [Service]
    public class RTMPService : Service
    {
        const string CHANNEL_ID = "RTMPStreamChannel";
        const int NOTIFICATION_ID = 1;
        const string TAG = "RTMPService";

        string rtmpUrl;
        bool isStreaming = false;

        public override void OnCreate()
        {
            base.OnCreate();
            CreateNotificationChannel();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            rtmpUrl = intent.GetStringExtra("url");
            Log.Info(TAG, $"Starting RTMP streaming to {rtmpUrl}");

            if (!isStreaming)
            {
                Task.Run(() => StartStreaming());
                isStreaming = true;
            }

            StartForeground(NOTIFICATION_ID, BuildNotification());
            return StartCommandResult.Sticky;
        }

        void StartStreaming()
        {
            // TODO: Тут підключити RTMP-бібліотеку для захоплення відео та стрімінгу по rtmpUrl
            // Поки що заглушка:
            Log.Info(TAG, "Streaming started (stub).");

            // Симуляція роботи:
            while (isStreaming)
            {
                Task.Delay(1000).Wait();
            }
        }

        public override void OnDestroy()
        {
            Log.Info(TAG, "Stopping RTMP streaming");
            isStreaming = false;
            base.OnDestroy();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        Notification BuildNotification()
        {
            var notificationIntent = new Intent(this, typeof(MainActivity));
            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.Immutable);

            var builder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle("RTMP Streamer")
                .SetContentText($"Streaming to {rtmpUrl}")
                .SetSmallIcon(Android.Resource.Drawable.IcMediaPlay)
                .SetContentIntent(pendingIntent)
                .SetOngoing(true);

            return builder.Build();
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(CHANNEL_ID, "RTMP Streaming", NotificationImportance.Low);
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }
    }
}
