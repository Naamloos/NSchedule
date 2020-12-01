using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;
using System.Reflection;
using System.IO;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Color = SixLabors.ImageSharp.Color;
using System.Threading.Tasks;
using Xamarin.Essentials;
using NSchedule.Entities;
using Xamarin.Forms;
using System.Linq;
using SixLabors.ImageSharp.Drawing;

namespace NSchedule.Helpers
{
    public class ImageHelper
    {
        // TODO: share full day
        public async Task ShareDayImageAsync()
        {

        }

        // TODO: share full week
        public async Task ShareWeekImageAsync()
        {

        }

        public async Task ShareAppointmentImageAsync(CalendarEvent c)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "NSchedule.Fonts.font.ttf";

            FontFamily font;

            FontCollection fonts = new FontCollection();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                font = fonts.Install(stream);
            }

            var Data = DependencyService.Get<DataHelper>();
            var att = c.Appointment.Attendees;

            var rooms = Data.Schedulables.Where(x => x.GetType() == typeof(Room) && att.Contains(x.Id)).Cast<Room>().Select(x => x.Code);
            var teachers = Data.Schedulables.Where(x => x.GetType() == typeof(Teacher) && att.Contains(x.Id)).Cast<Teacher>().Select(x => x.Code);
            var groups = Data.Schedulables.Where(x => x.GetType() == typeof(Group) && att.Contains(x.Id)).Cast<Group>().Select(x => x.Code);

            string yourText = $"{c.Name}\n{c.ScheduleableCode} ({c.Times})\n\n\nRooms:\n{JoinPerTwo(rooms)}\n\nTeachers:\n{JoinPerTwo(teachers)}\n\nGroups:\n{JoinPerTwo(groups)}\n\n{Constants.SHARED_WITH}";
            
            var f = font.CreateFont(25f);
            var measures = TextMeasurer.Measure(yourText, new RendererOptions(f));
            var img = new Image<Rgba32>((int)measures.Width + 50, (int)measures.Height + 50);

            img.Mutate(x => x.Fill(Color.White));

            IPath rect = new RectangularPolygon(15, 15, measures.Width + 20, measures.Height + 20);

            img.Mutate(x => x.Draw(Color.Gray, 5f, rect));

            img.Mutate(x => x.DrawText(yourText, f, Color.Black, new PointF(25f, 25f)));

            var basePath = System.IO.Path.GetTempPath();
            var path = System.IO.Path.Combine(basePath, "share.png");
            await img.SaveAsPngAsync(path);
            await Share.RequestAsync(new ShareFileRequest(c.Name, new ShareFile(path)));
            // cleanup after share..
            //File.Delete(path);
        }

        private string JoinPerTwo(IEnumerable<string> input)
        {
            var returns = "";
            bool newline = false;

            foreach(var s in input)
            {
                returns += s;
                if (s != input.Last())
                    returns += ", ";
                if (newline)
                    returns += "\n";
                newline = !newline;
            }

            return returns;
        }
    }
}
