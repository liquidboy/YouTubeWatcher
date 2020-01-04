using SharedCode.SQLite;
using SharedCode.YT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace MediaLibraryLegacy
{
    public static class EntitiesHelper
    {
        public static void DeleteAllByYID(string yid) {
            var foundMediaMetadata = DBContext.Current.RetrieveEntities<MediaMetadata>($"YID='{yid}'");
            if (foundMediaMetadata.Count > 0)
            {
                var uniqueId = foundMediaMetadata[0].UniqueId;

                // - delete from MediaMetadata
                DBContext.Current.DeleteEntity<MediaMetadata>(uniqueId);

                // - delete from PlaylistMediaMetadata
                var foundPlaylistMediaMetadata = DBContext.Current.RetrieveEntities<PlaylistMediaMetadata>($"MediaUid='{uniqueId.ToString()}'");

                if (foundPlaylistMediaMetadata.Count > 0)
                {
                    foreach (var entity in foundPlaylistMediaMetadata)
                    {
                        DBContext.Current.DeleteEntity<PlaylistMediaMetadata>(entity.UniqueId);
                    }
                }
            }
        }

        public static void AddPlaylistMediaMetadata(Guid mediaUid, Guid playlistUid)
        {


            var foundEntities = DBContext.Current.RetrieveEntities<PlaylistMediaMetadata>($"MediaUid='{mediaUid.ToString()}' and PlaylistUid='{playlistUid.ToString()}'");

            if (foundEntities.Count == 0)
            {
                var newEntity = new PlaylistMediaMetadata()
                {
                    MediaUid = mediaUid,
                    PlaylistUid = playlistUid,
                    DateStamp = DateTime.UtcNow
                };
                DBContext.Current.Save(newEntity);
            }
        }

        public static void AddMediaMetadata(VideoDetails videoDetails, string mediaType, string quality, long size)
        {
            var newEntity = new MediaMetadata()
            {
                YID = videoDetails.id,
                Title = videoDetails.Title,
                DateStamp = DateTime.UtcNow,
                ThumbUrl = videoDetails.thumbnails.MediumResUrl,
                MediaType = mediaType,
                Quality = quality,
                Size = size
            };

            DBContext.Current.Save(newEntity);
        }

        public static void AddPlaylistMetadata(string title)
        {
            var newEntity = new PlaylistMetadata()
            {
                Title = title,
                DateStamp = DateTime.UtcNow
            };
            DBContext.Current.Save(newEntity);
        }

        public static int RetrievePlaylistMetadataAsCount()
        {
            var foundItems = DBContext.Current.RetrieveAllEntities<PlaylistMetadata>();
            return (foundItems == null) ? 0 : foundItems.Count;
        }

        public static int RetrieveMediaMetadataAsCount()
        {
            var foundItems = DBContext.Current.RetrieveAllEntities<MediaMetadata>();
            return (foundItems == null) ? 0 : foundItems.Count;
        }

        public static (ObservableCollection<ViewPlaylistMetadata> source, Guid lastSelectedPlaylistId) RetrievePlaylistMetadataAsViewCollection(Guid currentLastSelectedPlaylistId)
        {
            var items = new ObservableCollection<ViewPlaylistMetadata>();
            var foundItems = DBContext.Current.RetrieveAllEntities<PlaylistMetadata>();
            var orderedItems = foundItems.OrderBy(x => x.Title);
            var lastSelectedPlaylistId = currentLastSelectedPlaylistId;
            foreach (var foundItem in orderedItems)
            {
                items.Add(new ViewPlaylistMetadata()
                {
                    UniqueId = foundItem.UniqueId,
                    Title = foundItem.Title
                });
                if (lastSelectedPlaylistId == Guid.Empty) lastSelectedPlaylistId = foundItem.UniqueId;
            }
            return (items, lastSelectedPlaylistId);
        }

        public static ObservableCollection<ViewMediaMetadata> RetrieveMediaMetadataAsViewCollection(string mediaPath)
        {
            var mediaItems = new ObservableCollection<ViewMediaMetadata>();
            var foundItems = DBContext.Current.RetrieveAllEntities<MediaMetadata>();
            var orderedItems = foundItems.OrderBy(x => x.Title);
            //foundItems.Reverse();
            foreach (var foundItem in orderedItems)
            {
                mediaItems.Add(new ViewMediaMetadata()
                {
                    UniqueId = foundItem.UniqueId,
                    Title = foundItem.Title,
                    YID = foundItem.YID,
                    ThumbUri = new Uri($"{mediaPath}\\{foundItem.YID}-medium.jpg", UriKind.Absolute),
                    Quality = foundItem.Quality,
                    MediaType = foundItem.MediaType,
                    Size = foundItem.Size,
                });
            }
            return mediaItems;
        }

        public static (ObservableCollection<ViewMediaMetadata> source, Guid lastSelectedPlaylistId) RetrievePlaylistMediaMetadataAsViewCollection(Guid playlistUid, string mediaPath)
        {
            var items = new ObservableCollection<ViewMediaMetadata>();
            var foundItems = DBContext.Current.RetrieveEntities<PlaylistMediaMetadata>($"PlaylistUid='{playlistUid.ToString()}'");

            var sqlIn = string.Empty;
            foreach (var foundItem in foundItems)
            {
                sqlIn += $"'{foundItem.MediaUid}' ,";
            }

            if (sqlIn.Length > 0)
            {
                sqlIn = sqlIn.Substring(0, sqlIn.Length - 1);
                var foundItems2 = DBContext.Current.RetrieveEntities<MediaMetadata>($"UniqueId IN ({sqlIn})");
                var orderedItems2 = foundItems2.OrderBy(x => x.Title);
                foreach (var foundItem in orderedItems2)
                {
                    items.Add(new ViewMediaMetadata()
                    {
                        UniqueId = foundItem.UniqueId,
                        Title = foundItem.Title,
                        YID = foundItem.YID,
                        ThumbUri = new Uri($"{mediaPath}\\{foundItem.YID}-medium.jpg", UriKind.Absolute),
                        Quality = foundItem.Quality,
                        MediaType = foundItem.MediaType,
                        Size = foundItem.Size,
                    });
                }
            }
            return (items, playlistUid);
        }

        public static ObservableCollection<ViewPlaylistMetadata> RetrievePlaylistMetadataAsViewCollection()
        {
            var items = new ObservableCollection<ViewPlaylistMetadata>();
            var foundItems = DBContext.Current.RetrieveAllEntities<PlaylistMetadata>();
            var orderedItems = foundItems.OrderBy(x => x.Title);
            foreach (var foundItem in orderedItems)
            {
                items.Add(new ViewPlaylistMetadata()
                {
                    Title = foundItem.Title,
                    UniqueId = foundItem.UniqueId
                });

            }
            return items;
        }
    }
}
