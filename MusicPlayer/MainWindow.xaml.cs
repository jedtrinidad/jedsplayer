using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ATL;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Song> song;
        MediaPlayer mp = new MediaPlayer();
        DispatcherTimer dt = new DispatcherTimer();
        Track trackTags;
        IList<TagData.PictureInfo> _albumArt;


        private bool IsSliderDragging = false;
        public double CurrentPos = 0.0;

        public MainWindow()
        {
            InitializeComponent();

            dt.Tick += Dt_Tick;
            dt.Interval = new TimeSpan(0, 0, 1);

            song = new ObservableCollection<Song>();

        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            try
            {
                var pos_min = mp.Position.Minutes;
                var pos_sec = mp.Position.Seconds;
                var dur_min = mp.NaturalDuration.TimeSpan.Minutes;
                var dur_sec = mp.NaturalDuration.TimeSpan.Seconds;
                tx_Position.Text = $"{pos_min}m{pos_sec}s";
                tx_Duration.Text = $"{dur_min}m{dur_sec}s";

                if (!IsSliderDragging)
                {
                    slider_Position.Minimum = 0;
                    slider_Position.Maximum = mp.NaturalDuration.TimeSpan.TotalSeconds;
                    slider_Position.Value = mp.Position.TotalSeconds;
                    CurrentPos = slider_Position.Value;
                }
            }
            catch
            {

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            fill_ListView(@"C:\Users\jed cyril\Music");

            bt_PlayPause.IsEnabled = false;
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            mp.Close();
        }

        private void fill_ListView(string folder)
        {

            string[] file = Directory.GetFiles(folder, "*.mp3", SearchOption.AllDirectories);
            
            for (int i = 0; i < file.GetLength(0); i++)
            {
                trackTags = new Track(file[i]);
                song.Add(new Song() {
                    Title = trackTags.Title, Album = trackTags.Album, Artist = trackTags.Artist, Path = file[i], albumArt = trackTags.EmbeddedPictures }
                );
            }
            lv_Songs.ItemsSource = song;
        }

        private void lv_Songs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mp.Open(new Uri(song[lv_Songs.SelectedIndex].Path));
            mp.Play();

            dt.Start();
            bt_PlayPause.IsEnabled = true;
            bt_PlayPause.IsChecked = true;

            tx_TrackTitle.Text = song[lv_Songs.SelectedIndex].Title;
            tx_TrackAlbum.Text = song[lv_Songs.SelectedIndex].Album;
            tx_TrackArtist.Text = song[lv_Songs.SelectedIndex].Artist;

            _albumArt = song[lv_Songs.SelectedIndex].albumArt;

            try
            {
                img_AlbumArt.Source = LoadImage(_albumArt[0].PictureData);
            }
            catch
            {

            }
        }

        private void bt_PlayPause_Click(object sender, RoutedEventArgs e)
        { 
           if(bt_PlayPause.IsChecked == true)
            {
                mp.Play();
            }
           else if(bt_PlayPause.IsChecked == false)
            {
                mp.Pause();
            }
        }

        private void bt_Fwd_Click(object sender, RoutedEventArgs e)
        {
            lv_Songs.SelectedIndex++;
        }

        private void bt_Prev_Click(object sender, RoutedEventArgs e)
        {
            lv_Songs.SelectedIndex--;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mp.Volume = slider_Volume.Value;
        }

        private void slider_Position_DragStarted(object sender, RoutedEventArgs e)
        {
            IsSliderDragging = true;
        }

        private void slider_Position_DragCompleted(object sender, RoutedEventArgs e)
        {
            IsSliderDragging = false;
            mp.Position = TimeSpan.FromSeconds(slider_Position.Value);
        }

        private void slider_Position_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(mp.Position == mp.NaturalDuration)
            {
                lv_Songs.SelectedIndex++;
            }
        }

        /// <summary>
        /// Loads Images embedded in mp3s
        /// </summary>
        /// <param name="imageData">ByteArray</param>
        /// <returns></returns>
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using(var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void MusicFolderPath_DialogHost_OnDialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            if (!Equals(eventArgs.Parameter, true)) return;
            else
            {
                Properties.Settings.Default.MusicFolderPath = tx_FolderPath.Text;
                Properties.Settings.Default.Save();
                song.Clear();
                fill_ListView(Properties.Settings.Default.MusicFolderPath);
            }
        }
    }
    /// <summary>
    /// Represents an audio file and metadata to be collected and then played
    /// </summary>
    public class Song
    {
        public string Title { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string Path { get; set; }

        public IList<TagData.PictureInfo> albumArt { get; set; }
    }
}
