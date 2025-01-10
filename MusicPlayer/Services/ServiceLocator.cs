using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Services
{
    public static class ServiceLocator // To allow access to the singleton even from the classes in /Android
    {
        public static AudioService AudioServiceInstance { get; } = new AudioService();
    }
}
