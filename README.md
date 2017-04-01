# MusicPlayer
WPF Music Player utilizing MVVM and NAudio

This app was built as a demo to show using MVVM in a WPF app that has a few cool front end tools. All graphics were done in XAML, no images were used. The EQ knobs have a center detent, making returning them to zero much easier.

NAudio was used to open MP3 files and play them and includes the FFT transforms; NAudio is the basic Model of the app. The ViewModel class receives property updates from the model, transforms them to data for the View, and then emits property changes for the View bindings.

You can download an installer for the exe here: https://github.com/PaulKeefe/MusicPlayer/blob/master/MusicPlayer.zip 

Here is an image of the app in use (the song playing is by Jeff Beck):

![alt tag](https://github.com/PaulKeefe/MusicPlayer/blob/master/wpf_music_player.png)


Comments, no matter how cruel, are always welcome!
