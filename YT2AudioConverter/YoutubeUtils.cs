using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using YT2AudioConverter.Services;
using YT2AudioConverter.Models;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;
using System.Reflection;
using YoutubeExplode.Videos;
using YoutubeExplode.Playlists;
using YoutubeExplode.Common;

namespace YT2AudioConverter
{
    public class YoutubeUtils : IUtils, IDisposable
    {
        private readonly List<string> _videoIds = new List<string> { };

        private readonly string FILE_BASE_PATH = $"{new DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent.FullName}\\Files";

        private NLog.ILogger _logger;

        private readonly YoutubeClient _youtube;

        public YoutubeUtils(IConfiguration configuration)
        {
            ServiceProvider.BuildDi(configuration);
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _youtube = new YoutubeClient();
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Save youtube video to specified file format
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ConvertResponse> ConvertYoutubeUriToFile(YoutubeToFileRequest request)
        {
            var videosConverted = 0;
            try
            {
                if (!ValidateRequestParams(request))
                {
                    _logger.Error($"Bad Request - model is invalid");
                    return GenerateResponse(videosConverted);
                }

                var isPlaylist = request.Uri.Contains("list");
                var requestId = ExtractRequestId(isPlaylist, request.Uri);

                if (!Directory.Exists(FILE_BASE_PATH))
                {
                    Directory.CreateDirectory(FILE_BASE_PATH);
                }

                if (isPlaylist)
                {
                    var response = await DownloadFilesFromPlaylist(request.Uri, request.TargetMediaType);
                    videosConverted = response.VideoConverted;
                }
                else
                {
                    var result = await RetrieveFile(request.Uri, request.TargetMediaType);
                    if (result.Succeeded)
                        videosConverted++;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing request: {ex}");
            }
            return GenerateResponse(videosConverted);
        }

        private bool ValidateRequestParams(YoutubeToFileRequest request)
        {
            return request != null
            && !string.IsNullOrEmpty(request.TargetMediaType)
            && !string.IsNullOrEmpty(request.Uri);
        }

        private string ExtractRequestId(bool isPlaylist, string url)
        {
            var requestId = string.Empty;
            switch (isPlaylist)
            {
                case true:
                    requestId = ExtractPlaylistIdFromRequestUri(url);
                    break;

                case false:
                    requestId = ExtractVideoIdFromRequestUri(url);
                    break;
            }
            return requestId;
        }

        private string ExtractPlaylistIdFromRequestUri(string uri)
        {
            var id = uri.Split(new string[] { "list=" }, StringSplitOptions.None)[1];

            if (id.Contains("index="))
            {
                id = uri.Split('=')[1];
            }

            return id;
        }

        private string ExtractVideoIdFromRequestUri(string uri)
        {
            var id = uri.Split(new string[] { "v=" }, StringSplitOptions.None)[1];
            return id;
        }

        private async Task<DownloadPlaylistResponse> DownloadFilesFromPlaylist(string playlistId, string mediaType)
        {
            var playlist = await _youtube.Playlists.GetAsync(playlistId);
            var videosConverted = 0;

            DownloadPlaylistResponse response = new DownloadPlaylistResponse()
            {
                VideoConverted = videosConverted,
                Successed = false
            };

            // Get all playlist videos
            var playlistVideos = await _youtube.Playlists.GetVideosAsync(playlist.Id);

            if (playlistVideos == null || !playlistVideos.Any())
            {
                return response;
            }

            // Make directory to house playlist files
            var playlistOutputDir = $"{FILE_BASE_PATH}\\{FormatFileName(playlist.Title)}";
            if (!Directory.Exists(playlistOutputDir))
            {
                Directory.CreateDirectory(playlistOutputDir);
            }

            playlistVideos.AsParallel()
                   .Select(video => RetrieveFile(video.Url, mediaType, playlistOutputDir).Result)
                   .ForAll(result => { if (result.Succeeded) videosConverted++; });

            response.Successed = true;
            response.VideoConverted = videosConverted;

            return response;
        }

        private async Task<ConvertResponse> RetrieveFile(string videoUrl, string mediaType, string outputDir = null)
        {
            outputDir = string.IsNullOrEmpty(outputDir) ? FILE_BASE_PATH : outputDir;
            var metaData = await _youtube.Videos.GetAsync(videoUrl);
            var newFileName = FormatFileName(metaData.Title);
            var newFilePath = $"{outputDir}\\{newFileName}.{mediaType}";

            ConvertResponse response = new ConvertResponse()
            {
                Succeeded = false,
                Error = string.Empty,
                Message = string.Empty
            };

            if (File.Exists(newFilePath))
            {
                var message = $"Download bypassed: {newFileName}.{mediaType} already exists.";

                _logger.Warn(message);
                response.Message = message;

                return response;
            }

            await DownloadFile(metaData, newFileName, mediaType, outputDir);
            response.Succeeded = true;
            return response;
        }

        private async Task DownloadFile(Video metaData, string newFileName, string mediaType, string outputDir)
        {
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(metaData.Id);

            if (streamManifest != null)
            {
                var status = $"Downloading {metaData.Title} as {mediaType}";
                Console.WriteLine(status);
                _logger.Info(status);

                await GetFileFromStreamManifest(streamManifest, newFileName, mediaType, outputDir);
                Console.WriteLine($"{newFileName} has been downloaded");
            }
        }

        private async Task GetFileFromStreamManifest(StreamManifest streamManifest, string newVidName, string mediaType, string outputDir)
        {
            // Select streams (highest video quality / highest bitrate audio)
            IVideoStreamInfo videoStreamInfo = null;

            var audioStreamInfo = streamManifest
                .GetAudioOnlyStreams()
                .GetWithHighestBitrate();

            if (mediaType.Contains("mp4"))
            {
                videoStreamInfo = streamManifest
                   .GetVideoOnlyStreams()
                   .Where(s => s.Container == Container.Mp4)
                   .GetWithHighestVideoQuality();
            }

            var streamInfos = new IStreamInfo[] { audioStreamInfo };
            if (videoStreamInfo != null)
            {
                streamInfos = streamInfos.Concat(new IStreamInfo[] { videoStreamInfo }).ToArray();
            }

            if (streamInfos != null)
            {
                // Download and process them into one file
                await _youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder($"{outputDir}\\{newVidName}.{mediaType}").Build());
            }
        }

        private static string FormatFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        private string FormatVideoUri(string id)
        {
            var videoUrl = $"https://www.youtube.com/watch?v=id";
            return videoUrl.Replace("id", id);
        }

        private ConvertResponse GenerateResponse(int videosConverted)
        {
            var newResponse = new ConvertResponse { Message = "", Succeeded = false };

            if (videosConverted > 0)
            {
                newResponse.Message = $"Converted {videosConverted} files successfully.";
                newResponse.Succeeded = true;
            }
            else
            {
                newResponse.Error = $"Unable to download file(s) for specified link";
            }

            return newResponse;
        }
    }
}