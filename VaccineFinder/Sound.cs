using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder
{
    class Sound
    {
        public static void PlayBeep(int count, int freq, int dur)
        {
            //Play Beep sound
            for (int i = 0; i < count; i++)
            {
                Console.Beep(freq, dur);
            }
        }

        public static void PlayAsterisk(int count)
        {
            //Play Asterisk sound
            for (int i = 0; i < count; i++)
            {
                SystemSounds.Asterisk.Play();
            }
        }
    }
}
