# MusicPlayer
WPF Music Player utilizing MVVM and NAudio

This app was built as a demo to show using MVVM in a WPF app that has a few cool front end tools. All graphics were done in XAML, no images were used. The EQ knobs have a center detent, making returning them to zero much easier.

NAudio was used to open MP3 files and play them and includes the FFT transforms; NAudio is the basic Model of the app. The ViewModel class receives property updates from the model, transforms them to data for the View, and then emits property changes for the View bindings.

You can download an installer for the exe here: https://github.com/PaulKeefe/MusicPlayer/blob/master/MusicPlayer.zip 

# Super Q
Added a draggy style semi-parametric EQ. Simply drag and listen to the frequency (x axis) and gain (y axis) change simultaneously. Very cool (to me anyway). The frequencies are calculated logarithmically, unable to find a way to set the Q to make it fully parametric!

![Super Q](https://github.com/PaulKeefe/MusicPlayer/blob/master/superQ.png)

Here is an image of the app in use (the song playing is from "Blow by Blow" by Jeff Beck):

![Keefe Music Player](https://github.com/PaulKeefe/MusicPlayer/blob/master/wpf_music_player.png)


Comments, no matter how cruel, are always welcome!
