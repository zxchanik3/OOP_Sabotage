using System.Windows.Media;

namespace lab
{
    public class AudioManager
    {
        private readonly MediaPlayer _menuMusic = new();
        private readonly MediaPlayer _raceMusic = new();

        public AudioManager()
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                _menuMusic.Open(new Uri(Path.Combine(basePath, "Music", "MenuSong.mp3")));
                _raceMusic.Open(new Uri(Path.Combine(basePath, "Music", "RaceSong.mp3")));
                
                _menuMusic.Volume = 0.2;
                _raceMusic.Volume = 0.2;
                
                _menuMusic.MediaEnded += (_, _) => _menuMusic.Position = TimeSpan.Zero;
                _raceMusic.MediaEnded += (_, _) => _raceMusic.Position = TimeSpan.Zero;
            }
            catch { /* Ігноруємо або логуємо за потреби */ }
        }

        public void PlayMenuMusic() { _raceMusic.Stop(); _menuMusic.Play(); }
        public void PlayRaceMusic() { _menuMusic.Stop(); _raceMusic.Play(); }
        public void StopAll() { _menuMusic.Stop(); _raceMusic.Stop(); }
    }
}