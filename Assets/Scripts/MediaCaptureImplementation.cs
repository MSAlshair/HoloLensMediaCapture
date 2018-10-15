#if WINDOWS_UWP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;

namespace UWPExample
{
    public class MediaCaptureImplementation
    {
        public enum PHOTO_RESOLUTION { HIGHEST, LOWEST };

        private bool _isInitialized;
        private byte[] _photoBytes;
        
        private DeviceInformation _selectedDeviceInformation;
        private ImageEncodingProperties _selectedPhotoResolution;
        private Action<MediaCapture, MediaCaptureFailedEventArgs> _processExtraTasksAfterWhenPhotoCaptureFailsMethod;

        public MediaCapture MyMediaCapture;
        
        public DateTime? Last_Photo_Captured;

        #region Constructors
        public MediaCaptureImplementation():this(null)
        {
        }

        public MediaCaptureImplementation(
            Action<MediaCapture, MediaCaptureFailedEventArgs> processExtraTasksAfterWhenPhotoCaptureFailsMethod)
        {
            _isInitialized = false;
            _processExtraTasksAfterWhenPhotoCaptureFailsMethod = processExtraTasksAfterWhenPhotoCaptureFailsMethod;
        }
        #endregion Constructors

        #region Initialization
        private async Task InitializeAsync()
        {
            MyMediaCapture = new MediaCapture();

            _selectedDeviceInformation = null;
            _selectedPhotoResolution = null;
            Last_Photo_Captured = null;

            var mediaCaptureInitializationSettings = await GetSelectedMediaCaptureInitializationSettingsAsync();

            if (mediaCaptureInitializationSettings != null)
            {
                await MyMediaCapture.InitializeAsync(mediaCaptureInitializationSettings);
                MyMediaCapture.Failed += MediaCapture_Failed;

                _isInitialized = true;
            }
        }

        #endregion Initialization

        #region Media Capture Events
        private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            _processExtraTasksAfterWhenPhotoCaptureFailsMethod?.Invoke(sender, errorEventArgs);
            _photoBytes = null;
            System.Diagnostics.Debug.WriteLine("Camera Failed");
        }
        #endregion Media Capture Events

        #region Camera Settings
        public async Task<DeviceInformationCollection> GetCameraDevicesAsync()
        {
            return await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync
                (DeviceClass.VideoCapture);
        }

        public async Task<int> GetNumberOfCameraDevicesAsync()
        {
            return (await GetCameraDevicesAsync()).Count;
        }

        public async Task<bool> IsCameraAvailableInDevice()
        {
            return (await GetNumberOfCameraDevicesAsync()) > 0;
        }

        public void SelectCameraDevice(DeviceInformation deviceInformation)
        {
            if (string.IsNullOrWhiteSpace(deviceInformation.Id))
            {
                _selectedDeviceInformation = deviceInformation;
            }
        }

        public async Task SelectDefaultCameraDeviceAsync()
        {
            if (await IsCameraAvailableInDevice())
            {
                var devices = await GetCameraDevicesAsync();

                _selectedDeviceInformation = devices.Count > 0 ? devices.First() : null;
            }
        }

        public async Task<MediaCaptureInitializationSettings> GetSelectedMediaCaptureInitializationSettingsAsync()
        {
            if (_selectedDeviceInformation == null)
                await SelectDefaultCameraDeviceAsync();

            if (_selectedDeviceInformation != null)
            {
                return new MediaCaptureInitializationSettings()
                {
                    PhotoCaptureSource = PhotoCaptureSource.Photo,
                    MemoryPreference = MediaCaptureMemoryPreference.Auto,
                    VideoDeviceId = _selectedDeviceInformation.Id,
                    AudioDeviceId = "",
                };
            }
            else
            {
                return null;
            }
        }
        #endregion Camera Settings

        #region Resolution Methods
        public async Task<IReadOnlyList<ImageEncodingProperties>> GetPhotoResolutionsAsync()
        {
            if (_selectedDeviceInformation == null)
                await SelectDefaultCameraDeviceAsync();

            if (_selectedDeviceInformation != null)
            {
                if (!_isInitialized)
                    await InitializeAsync();

                return MyMediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo)
                    .Select(item => item as ImageEncodingProperties).Where(item => item != null).ToList();
            }
            else
                return null;
        }

        public ImageEncodingProperties GetSelectedPhotoResolution()
        {
            return _selectedPhotoResolution;
        }

        public void SetPhotoResolutionTo(ImageEncodingProperties photoResolution)
        {
            if (photoResolution != null)
                _selectedPhotoResolution = photoResolution;
        }

        public async Task SetPhotoResolutionToAsync(PHOTO_RESOLUTION photoResolution)
        {
            var resolutions = await GetPhotoResolutionsAsync();

            if (resolutions != null)
            {
                var resolutionsDescending = resolutions.OrderByDescending(Resolution => Resolution.Height * Resolution.Width).ToList();

                if (PHOTO_RESOLUTION.HIGHEST == photoResolution)
                    _selectedPhotoResolution = resolutionsDescending.First();

                if (PHOTO_RESOLUTION.LOWEST == photoResolution)
                    _selectedPhotoResolution = resolutionsDescending.Last();
            }
            else
                _selectedPhotoResolution = null;
        }
        #endregion Resolution Methods

        #region Take Photo Methods
        public async Task<byte[]> TakePhotoBytesAsync()
        {
            await TakePhotoAsync();
            return _photoBytes;
        }

        public async Task<InMemoryRandomAccessStream> TakePhotoInMemoryRandomAccessStreamAsync()
        {
            await TakePhotoAsync();
            return await GetPhotoInMemoryRandomAccessStreamAsync();
        }

        public async Task<Texture2D> TakePhotoTexture2DAsync()
        {
            await TakePhotoAsync();

            return await GetPhotoTexture2D();
        }

        public async Task<bool> TakePhotoAndSavePhotoToPicturesLibraryAsync()
        {
            await TakePhotoAsync();
            return await SavePhotoToPicturesLibraryAsync(getDefaultFileName());
        }

        public async Task<bool> TakePhotoAndSavePhotoToPicturesLibraryAsync(string fileNameIncludinFileExtenstion)
        {
            await TakePhotoAsync();

            if (string.IsNullOrEmpty(fileNameIncludinFileExtenstion))
                fileNameIncludinFileExtenstion = getDefaultFileName();

            var picturesLibraryFolder = await StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            StorageFile file = await picturesLibraryFolder.SaveFolder.CreateFileAsync(fileNameIncludinFileExtenstion,
                CreationCollisionOption.GenerateUniqueName);

            return await SavePhotoToAsync(file);
        }

        public async Task<bool> TakePhotoAndSavePhotoToAsync(StorageFile storageFile)
        {
            await TakePhotoAsync();
            return await SavePhotoToAsync(storageFile);
        }

        public async Task TakePhotoAsync()
        {
            if ((await IsCameraAvailableInDevice()))
            {
                if (_selectedPhotoResolution == null)
                    await SetPhotoResolutionToAsync(PHOTO_RESOLUTION.HIGHEST);

                if (!_isInitialized)
                    await InitializeAsync();

                using (var photoRandomAccessStream = new InMemoryRandomAccessStream())
                {
                    Last_Photo_Captured = DateTime.Now;
                    await MyMediaCapture.CapturePhotoToStreamAsync(_selectedPhotoResolution, photoRandomAccessStream);
                    _photoBytes = await ConvertFromInMemoryRandomAccessStreamToByteArrayAsync(photoRandomAccessStream);
                    System.Diagnostics.Debug.WriteLine("Length: " + _photoBytes.Length);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No camera device detected!");
            }
        }
        #endregion Take Photo Methods

        #region Get Photo in Different Formats
        public static async Task<byte[]> ConvertFromInMemoryRandomAccessStreamToByteArrayAsync(
            InMemoryRandomAccessStream inMemoryRandomAccessStream)
        {
            using (var dataReader = new DataReader(inMemoryRandomAccessStream.GetInputStreamAt(0)))
            {
                var bytes = new byte[inMemoryRandomAccessStream.Size];
                await dataReader.LoadAsync((uint)inMemoryRandomAccessStream.Size);
                dataReader.ReadBytes(bytes);

                return bytes;
            }
        }

        public byte[] GetPhotoBytes()
        {
            return _photoBytes;
        }

        public async Task<InMemoryRandomAccessStream> GetPhotoInMemoryRandomAccessStreamAsync()
        {
            if (_photoBytes == null)
                return null;
            else
            {
                InMemoryRandomAccessStream inMemoryRandomAccessStream = new InMemoryRandomAccessStream();
                await inMemoryRandomAccessStream.WriteAsync(_photoBytes.AsBuffer());
                inMemoryRandomAccessStream.Seek(0);

                return inMemoryRandomAccessStream;
            }
        }

        public async Task<Texture2D> GetPhotoTexture2D()
        {
            var decoder = await BitmapDecoder.CreateAsync(await GetPhotoInMemoryRandomAccessStreamAsync());

            Texture2D texture = new Texture2D((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            texture.LoadImage(_photoBytes);

            return texture;
        }
        #endregion Get Photo in Different Formats

        #region Save Photo
        public async Task<bool> SavePhotoToPicturesLibraryAsync()
        {
            return await SavePhotoToPicturesLibraryAsync(getDefaultFileName());
        }

        public async Task<bool> SavePhotoToPicturesLibraryAsync(string fileNameIncludinFileExtenstion)
        {
            if (string.IsNullOrEmpty(fileNameIncludinFileExtenstion))
                fileNameIncludinFileExtenstion = getDefaultFileName();

            var picturesLibraryFolder = await StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            StorageFile file = await picturesLibraryFolder.SaveFolder.CreateFileAsync(fileNameIncludinFileExtenstion, 
                CreationCollisionOption.GenerateUniqueName);

            return await SavePhotoToAsync(file);
        }

        public async Task<bool> SavePhotoToAsync(StorageFile storageFile)
        {
            if(storageFile != null)
            {
                await Windows.Storage.FileIO.WriteBufferAsync(storageFile, _photoBytes.AsBuffer());
                return true;
            }

            return false;
        }
        #endregion Save Photo

        private string getDefaultFileName()
        {
            return "Default_Photo_" + DateTime.Now.ToString("MM_dd_yyyy hh-mm-ss-ttt") + ".jpg";
        }
    }
}
#endif