using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using FractalPainting.App.Fractals;
using FractalPainting.Infrastructure.Common;
using FractalPainting.Infrastructure.UiActions;

namespace FractalPainting.App
{
    public class ImageSettingsAction : IUiAction
    {
        private readonly ImageSettings _imageSettings;
        private readonly IImageHolder _imageHolder;
        
        public ImageSettingsAction(ImageSettings imageSettings, IImageHolder imageHolder)
        {
            _imageSettings = imageSettings;
            _imageHolder = imageHolder;
        }

        public MenuCategory Category => MenuCategory.Settings;
        public string Name => "Изображение...";
        public string Description => "Размеры изображения";

        public void Perform()
        {
            SettingsForm.For(_imageSettings).ShowDialog();
            _imageHolder.RecreateImage(_imageSettings);
        }
    }

    public class SaveImageAction : IUiAction
    {
        private readonly IImageHolder _imageHolder;
        private readonly AppSettings _appSettings;

        public SaveImageAction(IImageHolder imageHolder, AppSettings appSettings)
        {
            _imageHolder = imageHolder;
            _appSettings = appSettings;
        }
        
        public MenuCategory Category => MenuCategory.File;
        public string Name => "Сохранить...";
        public string Description => "Сохранить изображение в файл";

        public void Perform()
        {
            var dialog = new SaveFileDialog
            {
                CheckFileExists = false,
                InitialDirectory = Path.GetFullPath(_appSettings.ImagesDirectory),
                DefaultExt = "bmp",
                FileName = "image.bmp",
                Filter = "Изображения (*.bmp)|*.bmp"
            };
            var res = dialog.ShowDialog();
            if (res == DialogResult.OK)
                _imageHolder.SaveImage(dialog.FileName);
        }
    }

    public class PaletteSettingsAction : IUiAction
    {
        private readonly Palette _palette;

        public PaletteSettingsAction(Palette palette)
        {
            _palette = palette;
        }

        public MenuCategory Category => MenuCategory.Settings;
        public string Name => "Палитра...";
        public string Description => "Цвета для рисования фракталов";

        public void Perform()
        {
            SettingsForm.For(_palette).ShowDialog();
        }
    }

    public class MainForm : Form
    {
        public MainForm(IUiAction[] actions, PictureBoxImageHolder pictureBox)
        {
            var imageSettings = CreateSettingsManager().Load().ImageSettings;
            ClientSize = new Size(imageSettings.Width, imageSettings.Height);

            pictureBox.RecreateImage(imageSettings);
            pictureBox.Dock = DockStyle.Fill;
            Controls.Add(pictureBox);

            var mainMenu = new MenuStrip();
            mainMenu.Items.AddRange(actions.ToMenuItems());
            mainMenu.Dock = DockStyle.Top;
            Controls.Add(mainMenu);
        }

        private static SettingsManager CreateSettingsManager()
        {
            return new SettingsManager(new XmlObjectSerializer(), new FileBlobStorage());
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Text = "Fractal Painter";
        }
    }
}