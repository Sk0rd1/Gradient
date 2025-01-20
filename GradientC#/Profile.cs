using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradientC_
{
    internal static class Profile
    {
        public static async Task RemoveAnotherProfiles()
        {
            string userDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google\\Chrome\\User Data");

            var directories = Directory.GetDirectories(userDataPath);

            foreach (var directory in directories)
            {
                string dirName = Path.GetFileName(directory);
                if (dirName.StartsWith("ProfileChrome"))
                {
                    try
                    {
                        Directory.Delete(directory, true);
                        Console.WriteLine($" # Profile successfully deleted: {dirName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" # Error deleting profile {dirName}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine(" # Removal work completed");
        }
    }
}
