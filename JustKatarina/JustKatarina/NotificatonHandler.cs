using JustKatarina;
using LeagueSharp.Common;
using SharpDX;

namespace JustKatarina
{
    class NotificationHandler
    {
        private static Notification _modeNotificationHandler;

        public static void Update()
        {
            var text = "Don't forget the upvote in AssemblyDB";

            if (_modeNotificationHandler == null)
            {
                _modeNotificationHandler = new Notification(text)
                {
                    TextColor = new ColorBGRA(124, 252, 0, 255)
                };
                Notifications.AddNotification("By Justy | JustKatarina Beta Version", 8000);
                Notifications.AddNotification(text, 12000);
            }

            _modeNotificationHandler.Text = text;
        }
    }
}