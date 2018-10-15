using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if WINDOWS_UWP
using UWPExample;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

public class MediaCaptureExample : MonoBehaviour {
    public RawImage image;
    public Canvas canvas;
    // Use this for initialization
    void Start () {
#if WINDOWS_UWP
        //Minimum needed to take a photo and SAVE file to pictures library with desired name
        TakePhotoBasicAndSaveAsync();

        //Minimum neeed to take a photo and SAVE and DISPLAY
        TakePhotoBasicAndSaveAndDisplay();

        //Below method has examples of other available options to call
        //TakePhotoMoreOptionsAsync();
#endif
    }

#if WINDOWS_UWP
    public async void TakePhotoBasicAndSaveAsync()
    {
        await new MediaCaptureImplementation().TakePhotoAndSavePhotoToPicturesLibraryAsync(
            "Demo_" + DateTime.Now.ToString("MM_dd_yyyy hh-mm-ss-tt") + ".jpg");
    }

    public async void TakePhotoBasicAndSaveAndDisplay()
    {
        var photoImplementation = new MediaCaptureImplementation();
        Texture2D texture = await photoImplementation.TakePhotoTexture2DAsync();

        if (image != null)
        {
            image.texture = texture;
            image.SetNativeSize();

            if(canvas != null)
            {
                CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();

                if(canvasScaler != null)
                {
                    canvasScaler.referenceResolution = new Vector2(texture.width*1.25f, texture.height*1.25f);
                }
            }
        }

        await photoImplementation.SavePhotoToPicturesLibraryAsync();
    }
    public async void TakePhotoMoreOptionsAsync()
    {
        #region Step 1: Needed (Constructors)
        //Step 1: Needed
        //Constructor 1
        var photoImplementation = new MediaCaptureImplementation();

        //Constructor 2
        var photoImplementation1 = new MediaCaptureImplementation(processExtraTasksAfterWhenPhotoCaptureFailsMethod);
        #endregion  Step 1: Needed (Constructors)

        #region Optional Settings Before Taking Photo
        //Optional Setting: Setting photo resolution to highest available resolution that your device has
        await photoImplementation.SetPhotoResolutionToAsync(MediaCaptureImplementation.PHOTO_RESOLUTION.HIGHEST);

        //Optional Setting: Setting photo resolution to lowest available resolution that your device has
        await photoImplementation.SetPhotoResolutionToAsync(MediaCaptureImplementation.PHOTO_RESOLUTION.LOWEST);

        //Optional Setting: Get a list of all available resolutions to choose from
        var allAvailableResolutions = await photoImplementation.GetPhotoResolutionsAsync();
        //Optinal Setting: Example of how to select medium resolution after sorting
        photoImplementation.SetPhotoResolutionTo(allAvailableResolutions.
            OrderByDescending(item => item.Height * item.Width).ToList()[(allAvailableResolutions.Count / 2)]);
        #endregion Optional Settings Before Taking Photo

        #region Step 2: Needed [Take Photo]
        //Step 2: Needed [Use one of the the methods below to take the photo]
        //Step 2: Method 1
        await photoImplementation.TakePhotoAsync();

        //Step 2: Method 2
        byte[] photoBytes = await photoImplementation.TakePhotoBytesAsync();

        //Step 2: Method 3
        InMemoryRandomAccessStream photoInMemoryRandomAccessStream = 
            await photoImplementation.TakePhotoInMemoryRandomAccessStreamAsync();

        //Step 2: Method 4
        Texture2D photoTexture2D =
            await photoImplementation.TakePhotoTexture2DAsync();

        //Step 2: Method 5 & Save
        bool photoAndSavoPicturesLibraryWithDefaultName =
            await photoImplementation.TakePhotoAndSavePhotoToPicturesLibraryAsync();

        //Step 2: Method 6 & Save
        bool photoAndSavoPicturesLibrary =
            await photoImplementation.TakePhotoAndSavePhotoToPicturesLibraryAsync(
                "FileName_" + DateTime.Now.ToString("MM_dd_yyyy hh-mm-ss-tt") + ".jpg");

        //Step 2: Method 7 & Save
        bool photoAndSavoWhereEverYouWant =
            await photoImplementation.TakePhotoAndSavePhotoToAsync(
                await (await StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures)).
                    SaveFolder.CreateFileAsync("AnyWhere_" + DateTime.Now.ToString("MM_dd_yyyy hh-mm-ss-tt") + ".jpg", 
                    CreationCollisionOption.GenerateUniqueName));
        #endregion  Step 2: Needed [Take Photo]

        #region Save Photo
        //Optional: Save Photo
        //Method 1
        bool savoPicturesLibraryWithDefaultName =
            await photoImplementation.SavePhotoToPicturesLibraryAsync();

        //Method 2
        bool savoPicturesLibrary =
            await photoImplementation.SavePhotoToPicturesLibraryAsync(
                "FileName_" + DateTime.Now.ToString("MM_dd_yyyy hh-mm-ss-tt") +  ".jpg");

        //Method 3
        bool savoWhereEverYouWant =
            await photoImplementation.SavePhotoToAsync(
                await (await StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures)).
                    SaveFolder.CreateFileAsync("AnyWhere_" + DateTime.Now.ToString("MM_dd_yyyy hh-mm-ss-tt") + ".jpg", 
                    CreationCollisionOption.GenerateUniqueName));
        #endregion Save Photo

        #region Optional Get Photo in Different Formats
        byte[] getPhotoBytes = photoImplementation.GetPhotoBytes();

        InMemoryRandomAccessStream getPhotoInMemoryRandomAccessStream =
            await photoImplementation.GetPhotoInMemoryRandomAccessStreamAsync();

        Texture2D getPhotoTexture2D =
            await photoImplementation.GetPhotoTexture2D();
        #endregion Optional Get Photo in Different Formats
    }

    private void processExtraTasksAfterWhenPhotoCaptureFailsMethod(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
    {
        //Do your process after failing to take photo
    }
#endif
    // Update is called once per frame
    void Update () {
		
	}
}
