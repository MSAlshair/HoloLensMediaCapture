# HoloLensMediaCapture
### Why?
Few days ago I discovered that UnityEngine.XR.WSA.WebCam.**PhotoCapture** is NOT working in HoloLens when I tried to upgrade my unity and discovered that other people have the same issue. I was not able to find an alternative workaround I don't know when Unity will be able to fix it. **Links to issues below in the "What is the issue?" section.**
### Workaround:
I created a workaround to be able to use camera in HoloLens until Unity fixes the problem. The only two things that you need from this project are:
**1. MediaCaptureImplementation.cs:** This is a class that implements MediaCapture. This will allow you to **take, save, and display** photos in HoloLens. It focuses on the functions that I need for my own project, but I think that this could help few other people as well.
**1. MediaCaptureExample.cs:** This is a script that attached to GameObject with implemented examples of how to use MediaCaptureImplementation class. The examples ready for you to copy and paste and tweak based on your needs. This will allow you to copy to fullfil your needs and help you avoid wasting your time trying to decipher my MediaCaptureImplementation class.
### Bug
It seems that the camera does NOT work when you install the app for the first time at the Start method when HoloLens display propmts to ask for permission to use the camera and the microphone. Unity silenty throws exceptions that I saw in debug log stating that resources are busy.
### Do NOT forget to add capabilities:
1. WebCam
2. Microphone: I am not sure if it needed since we are only taking photos or not, but I added it anyway.
3. Pictures Library if you want to save to it

### This project tested on:
1. Unity **2018.2.12f1**
Windows SDK: **10.0.17134.0**
**Unity Configuration:**
Scripting Runtime Version: **.NET 4.X Equivalent**
Scripting Backend: **.NET**
Api Compatibility Level*: **.NET 4.X**
1. Unity **2018.2.12f1**
Windows SDK: **10.0.17134.0**
**Unity Configuration:**
Scripting Runtime Version: **.NET 4.X Equivalent**
Scripting Backend: **IL2CPP**
Api Compatibility Level*: **.NET Standard 2.0**
1. **My problem with IL2CPP** is that I don't know how to debug in C#. That wasted my time to find the camera problem. I figuredout how to do attach the debugger, but it will not hit my C# code. I know how make it compile correctly, but not debug it. It fails to attach unity debugger to the hololens device. But visual studio allows me to attach debugger to the deployed application, but it will not hit the C# code, I chose managed and then i switched to native, then switched to both and none worked. According to this video from unity, it should work:
https://oc.unity3d.com/index.php/s/rx7KD0SYeQXr6qn
and this is the unity thread that I got the video from
https://oc.unity3d.com/index.php/s/rx7KD0SYeQXr6qn
### What is the issue?
1. **Unity public issue tracker:**
https://issuetracker.unity3d.com/issues/windowsmr-failure-to-take-photo-capture-in-hololens
2. **Unity thread 1 discussing this problem:**
https://forum.unity.com/threads/hololens-photo-capturing-failing.548845/ 
3. **Unity thread 2:**
https://forum.unity.com/threads/photocapture-not-called-while-running-in-hololens.541825/
4. **Microsoft GitHub thread:** PosterCalibration Camera Not Working in Unity 2018 #299
https://github.com/Microsoft/MixedRealityCompanionKit/issues/299 
