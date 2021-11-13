﻿using Encountify.Models;
using Encountify.Services;
using System;
using System.Diagnostics;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Encountify.ViewModels
{
    class HomePageViewModel : BaseViewModel
    {
        private ImageSource _downloadedImageSource;
        public Command OnImageChangeCommand { get; }

        public HomePageViewModel()
        {
            OnImageChangeCommand = new Command(OnChangeImageButtonClicked);

        }

        public ImageSource DownloadedImageSource
        {
            get => _downloadedImageSource;
            set => SetProperty(ref _downloadedImageSource, value);
        }

        private Image _imageOpenClose;
        public Image ImageOpenClose
        {
            get
            {
                return _imageOpenClose;
            }
            set
            {
                _imageOpenClose = value;
                OnPropertyChanged();
            }
        }

        private async void OnChangeImageButtonClicked()
        {
            byte[] newPicture = null;
            try
            {
                var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = "Please pick a photo" });
                var newFile = Path.Combine(FileSystem.CacheDirectory, result.FileName);
                try
                {
                    // documentation says that this using FullPath might cause errors
                    File.Copy(result.FullPath, newFile, true);
                }
                catch (FileNotFoundException fnfe)
                {
                    newFile = null;
                    Debug.WriteLine(fnfe.ToString());
                }
                catch (IOException iox)
                {
                    Debug.WriteLine(iox.Message);
                }
                ImageOpenClose.Source = ImageSource.FromFile(newFile);
                newPicture = File.ReadAllBytes(newFile);
            }
            catch (PermissionException pEx)
            {
                // Permissions not granted
                Debug.WriteLine("you don't have permissions");
                Debug.WriteLine(pEx.ToString());
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature is not supported on the device
                Debug.WriteLine("feature is not supported");
                Debug.WriteLine(fnsEx.ToString());
            }
            catch (NullReferenceException nrE)
            {
                // No image has been selected
                Debug.WriteLine("no image has been selected");
                Debug.WriteLine(nrE.ToString());
            }
            if (newPicture != null)
            {
                var users = new DatabaseAccess<User>();
                var newData = await users.GetAsync(App.UserID);
                newData.Picture = newPicture;
                await users.UpdateAsync(newData);
            }
        }
    }
}