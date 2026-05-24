using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace lab
{
    public class ParticleManager
    {
        private readonly Canvas _canvas;
        private readonly List<Ellipse> _particles = new();

        public ParticleManager(Canvas canvas) => _canvas = canvas;

        public void SpawnDust(Vector2 position, string trackName)
        {
            var dust = new Ellipse
            {
                Width = 10, Height = 10,
                Fill = trackName == "Winter" ? Brushes.White : Brushes.SaddleBrown,
                Opacity = 0.8
            };
            Canvas.SetLeft(dust, position.X - 5);
            Canvas.SetTop(dust, position.Y - 5);
            _canvas.Children.Add(dust);
            _particles.Add(dust);
        }

        public void UpdateParticles()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Opacity -= 0.05;
                p.Width += 0.5; p.Height += 0.5;
                Canvas.SetLeft(p, Canvas.GetLeft(p) - 0.25);
                Canvas.SetTop(p, Canvas.GetTop(p) - 0.25);

                if (p.Opacity <= 0)
                {
                    _canvas.Children.Remove(p);
                    _particles.RemoveAt(i);
                }
            }
        }

        public void Clear()
        {
            foreach (var p in _particles) _canvas.Children.Remove(p);
            _particles.Clear();
        }
    }
}