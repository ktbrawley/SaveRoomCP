### Overview
A class library which assists in the conversion of Youtube Videos to specific file format.

Built using [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode). Check it out!

### Usage
Single method that takes on a request object
```
ConvertYoutubeUriToFile(YoutubeToFileRequest request)
```

Request object is comprised of three properties:
1. URI of the Youtube video/playlist
1. Target Media Type you'd like to convert the video to (e.g. wav, mp3, mp4)