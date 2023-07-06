using System;
using System.Drawing;
using System.Linq;
using FractalPainting.App.Fractals;
using FractalPainting.Infrastructure.Common;
using FractalPainting.Infrastructure.UiActions;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Factory;
 
namespace FractalPainting.App
{
    public static class DIContainerTask
    {
        public static MainForm CreateMainForm() => ConfigureContainer().Get<MainForm>();
 
        public static StandardKernel ConfigureContainer()
        {
            var container = new StandardKernel();
            container.Bind(configure =>
                configure.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IUiAction>()
                    .BindAllInterfaces());

            container.Bind<MainForm>().ToSelf();
            
            container.Bind<Palette>().ToSelf().InSingletonScope();
 
            container.Bind<IDragonPainterFactory>().ToFactory();
            container.Bind<IImageHolder, PictureBoxImageHolder>().To<PictureBoxImageHolder>().InSingletonScope();
            
            container.Bind<IObjectSerializer>().To<XmlObjectSerializer>().WhenInjectedInto<SettingsManager>();
            container.Bind<IBlobStorage>().To<FileBlobStorage>().WhenInjectedInto<SettingsManager>();
            container.Bind<SettingsManager>().ToSelf()
                .WithConstructorArgument(new XmlObjectSerializer())
                .WithConstructorArgument(new FileBlobStorage());
 
            container.Bind<AppSettings>().ToMethod(context => context.Kernel.Get<SettingsManager>().Load())
                .InSingletonScope();
            container.Bind<ImageSettings>().ToMethod(context => context.Kernel.Get<AppSettings>().ImageSettings)
                .InSingletonScope();
            return container;
        }
    }
 
    public interface IDragonPainterFactory
    {
        DragonPainter CreatePainter(DragonSettings dragonSettings);
    }
 
    public class DragonFractalAction : IUiAction
    {
        public MenuCategory Category => MenuCategory.Fractals;
        
        public string Name => "Дракон";
 
        public string Description => "Дракон Хартера-Хейтуэя";

        private readonly Func<DragonSettings, DragonPainter> dragonPainterFactory;
 
        public DragonFractalAction(Func<DragonSettings, DragonPainter> dragonPainterFactory)
        {
            this.dragonPainterFactory = dragonPainterFactory;
        }
 
        public void Perform()
        {
            var dragonSettings = CreateRandomSettings();
            // редактируем настройки:
            SettingsForm.For(dragonSettings).ShowDialog();
            // создаём painter с такими настройками
 
            dragonPainterFactory(dragonSettings).Paint();
        }
 
        private static DragonSettings CreateRandomSettings()
        {
            return new DragonSettingsGenerator(new Random()).Generate();
        }
    }
 
    public class KochFractalAction : IUiAction
    {
        public MenuCategory Category => MenuCategory.Fractals;
        public string Name => "Кривая Коха";
        public string Description => "Кривая Коха";
 
        private readonly Lazy<KochPainter> _painter;
 
        public KochFractalAction(Lazy<KochPainter> painter)
        {
            _painter = painter;
        }
 
        public void Perform()
        {
            _painter.Value.Paint();
        }
    }
 
    public class DragonPainter
    {
        private readonly IImageHolder _imageHolder;
        private readonly DragonSettings _settings;
        private readonly Palette _palette;
 
        public DragonPainter(IImageHolder imageHolder, DragonSettings settings, Palette palette)
        {
            _imageHolder = imageHolder;
            _settings = settings;
            _palette = palette;
        }
 
        public void Paint()
        {
            var imageSize = _imageHolder.GetImageSize();
            var size = Math.Min(imageSize.Width, imageSize.Height) / 2.1f;

            using (var paintingBrush = new SolidBrush(_palette.BackgroundColor))
            using (var backgroundBrush = new SolidBrush(_palette.PrimaryColor))
            using (var graphics = _imageHolder.StartDrawing())
            {
                graphics.FillRectangle(paintingBrush, 0, 0, imageSize.Width, imageSize.Height);
                var r = new Random();
                var cosa = (float) Math.Cos(_settings.Angle1);
                var sina = (float) Math.Sin(_settings.Angle1);
                var cosb = (float) Math.Cos(_settings.Angle2);
                var sinb = (float) Math.Sin(_settings.Angle2);
                var shiftX = _settings.ShiftX * size * 0.8f;
                var shiftY = _settings.ShiftY * size * 0.8f;
                var scale = _settings.Scale;
                var p = new PointF(0, 0);
                foreach (var i in Enumerable.Range(0, _settings.IterationsCount))
                {
                    graphics.FillRectangle(backgroundBrush, imageSize.Width / 3f + p.X, imageSize.Height / 2f + p.Y, 1, 1);
                    if (r.Next(0, 2) == 0)
                        p = new PointF(scale * (p.X * cosa - p.Y * sina), scale * (p.X * sina + p.Y * cosa));
                    else
                        p = new PointF(scale * (p.X * cosb - p.Y * sinb) + shiftX,
                            scale * (p.X * sinb + p.Y * cosb) + shiftY);
                    if (i % 100 == 0) _imageHolder.UpdateUi();
                }
            }
 
            _imageHolder.UpdateUi();
        }
    }
}