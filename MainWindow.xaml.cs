using Microsoft.Toolkit.Wpf.UI.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace YouTubeWatcher
{
    public partial class MainWindow : Window
    {
        private IYoutubeClientHelper clientHelper;
        private string workingPath = "d:\\deleteme\\downloadedMedia";
        private string installLocation = System.AppDomain.CurrentDomain.BaseDirectory;
        private WebView wvMain;
        private VideoDetails selectedVideoDetail;



        public class MediaMetadata : BaseEntity
        {
            public string Title { get; set; }
            public DateTime DateStamp { get; set; }
            public string YID { get; set; }
            public string ThumbUrl { get; set; }
        }


        public MainWindow()
        {
            InitializeComponent();
            
            AppDatabase.Current(workingPath).Init();

            clientHelper = new YoutubeClientHelper(new YoutubeClient(), installLocation);

            Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess process = new Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess();
            //process.ProcessExited += Process_ProcessExited;
            wvMain = new WebView(process);
            wvMain.Margin = new Thickness(0, 0, 0, 30);
            wvMain.ContentLoading += WvMain_ContentLoading;
            layoutRoot.Children.Add(wvMain);
            wvMain.Source = new Uri("https://www.youtube.com");

            TestData();
        }

        private void TestData() {
            //create new
            var oh = new MediaMetadata()
            {
                YID = Guid.NewGuid().ToString(),
                Title = "test title",
                DateStamp = DateTime.UtcNow,
                ThumbUrl = string.Empty,
            };
            var newid = DBContext.Current.Save(oh);


            //search
            var resultOrderHeader = DBContext.Current.RetrieveEntity<MediaMetadata>(oh.UniqueId);

            var foundItems = DBContext.Current.RetrieveEntities<MediaMetadata>($"Title=='test title'");

            var foundItems2 = DBContext.Current.RetrieveAllEntities<MediaMetadata>();


            ////load 
            //if (DBContext.Current.RetrieveEntity<OrderHeader>(oh.UniqueId) != null)
            //{
            //    //delete
            //    DBContext.Current.Delete<OrderHeader>(newid);
            //}

            ////delete
            //DBContext.Current.DeleteAll<OrderHeader>();

            ////delete from manager
            //DBContext.Current.Manager.DeleteAllDatabases();
        }

        private async void WvMain_ContentLoading(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlContentLoadingEventArgs e)
        {
            var url = await wvMain.InvokeScriptAsync("eval", new String[] { "document.location.href;" });
            tbUrl.Text = url;
            VideoChanging();
            await GetVideoDetails();
        }

        private void VideoChanging() {
            cbFormats.Items.Clear();
            butLoad.IsEnabled = false;
        }

        private void VideoChanged() {
            butLoad.IsEnabled = true;
        }

        private async Task GetVideoDetails() {
            if (!IsValidUrl()) return;
            selectedVideoDetail = null;
            try {
                selectedVideoDetail = await clientHelper.GetVideoMetadata(clientHelper.GetVideoID(tbUrl.Text));
                if (selectedVideoDetail != null)
                {
                    foreach (var mt in selectedVideoDetail.qualities)
                    {
                        cbFormats.Items.Add(new ComboBoxItem() { Content = mt });
                    }
                }
                VideoChanged();
            }
            catch (Exception ex) {
                // todo: handle error
            }
        }

        bool isDownloadingThumb = false;
        private async Task DownloadThumbnails() {
            if (selectedVideoDetail == null) return;
            if (isDownloadingThumb) return;
            isDownloadingThumb = true;
            await DownloadImageAsync($"{selectedVideoDetail.id}-low" , new Uri(selectedVideoDetail.thumbnails.LowResUrl));
            await DownloadImageAsync($"{selectedVideoDetail.id}-medium", new Uri(selectedVideoDetail.thumbnails.MediumResUrl));
            await DownloadImageAsync($"{selectedVideoDetail.id}-standard", new Uri(selectedVideoDetail.thumbnails.StandardResUrl));
            await DownloadImageAsync($"{selectedVideoDetail.id}-high", new Uri(selectedVideoDetail.thumbnails.HighResUrl));
            await DownloadImageAsync($"{selectedVideoDetail.id}-max", new Uri(selectedVideoDetail.thumbnails.MaxResUrl));
            isDownloadingThumb = false;
        }

        private async Task DownloadImageAsync(string fileName, Uri uri)
        {
            using var httpClient = new System.Net.Http.HttpClient();

            // Get the file extension
            var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
            var fileExtension = System.IO.Path.GetExtension(uriWithoutQuery);

            // Download the image and write to the file
            var path = System.IO.Path.Combine(workingPath, $"{fileName}{fileExtension}");
            var imageBytes = await httpClient.GetByteArrayAsync(uri);
            await File.WriteAllBytesAsync(path, imageBytes);
        }

        private bool IsValidUrl() {
            if (tbUrl.Text == "https://www.youtube.com/") return false;
            if (string.IsNullOrEmpty(tbUrl.Text)) return false;
            return true;
        }

        bool isDownloadingVideo = false;
        private async void butLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidUrl()) return;
            if (selectedVideoDetail == null) return;
            if (isDownloadingVideo) return;

            await DownloadThumbnails();

            var mediaType = (string)((ComboBoxItem)cbMediaType.SelectedValue).Content;
            var quality = (mediaType != "mp3") ? (string)((ComboBoxItem)cbFormats.SelectedValue).Content : string.Empty;
            var mediaPath = workingPath + $"\\{selectedVideoDetail.id}.{mediaType}";

            try
            {
                isDownloadingVideo = true;
                if (File.Exists(mediaPath)) File.Delete(mediaPath);
                await clientHelper.DownloadMedia(selectedVideoDetail.id, quality, mediaPath, mediaType);
            }
            catch (Exception ex)
            {
                // todo: handle error
            }

            isDownloadingVideo = false;
        }
    }

    public class YoutubeClientHelper : IYoutubeClientHelper
    {
        IYoutubeClient client;
        IYoutubeConverter converter;

        public YoutubeClientHelper(IYoutubeClient client, string installLocation)
        {
            this.client = client;

            string ffmpegExePath = installLocation + "\\ffmpeg.exe"; //Path to the ffmpeg.exe file used to mux audio&video stream. It should be located in wwwrooot/ffmpeg.exe
            converter = new YoutubeConverter(client, ffmpegExePath);

        }

        public string GetVideoID(string videoUrl)
        {
            return YoutubeClient.ParseVideoId(videoUrl);
        }

        public async Task<VideoDetails> GetVideoMetadata(string videoId)
        {
            var video = await client.GetVideoAsync(videoId);
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);
            var qualities = SortQualities(streamInfoSet.GetAllVideoQualityLabels());

            return new VideoDetails() { id = videoId, ChannelName = video.Author, Title = video.Title, qualities = qualities, thumbnails = video.Thumbnails };
        }

        public async Task DownloadMedia(string id, string quality, string videoPath, string mediaType)
        {
            MediaStreamInfoSet streamInfoSet;
            streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);
            var audioStreamInfo = streamInfoSet.Audio.WithHighestBitrate();
            var videoStreamInfo = streamInfoSet.Video.FirstOrDefault(c => c.VideoQualityLabel == quality);

            if (mediaType == "mp4")
            {
                var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo, videoStreamInfo };
                await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, videoPath, "mp4");
            }
            else if (mediaType == "mp3")
            {
                var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo };
                await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, videoPath, "mp3");
            }
            else {
                throw new ArgumentException("mediaType not supported");
            }
        }

        IEnumerable<string> SortQualities(IEnumerable<string> qualities)
        {
            var sortedStrings = qualities.ToList();
            return sortedStrings
                .Select(s => new { str = s, split = s.Split('p') })
                .OrderBy(x => int.Parse(x.split[0]))
                .ThenBy(x => x.split[1])
                .Select(x => x.str)
                .ToList();
        }
    }



    public class MediaStream
    {
        public async Task<MemoryStream> prepareMediaStream(string path)
        {
            if (!File.Exists(path))
                return null;
            var memory = new MemoryStream(); // No need to dispose MemoryStream, GC will take care of this

            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return memory;
        }
    }

    public class VideoDetails
    {
        public string id { get; set; }
        public string ChannelName { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> qualities { get; set; }
        public ThumbnailSet thumbnails { get; set; }
    }

    public interface IYoutubeClientHelper
    {
        public Task<VideoDetails> GetVideoMetadata(string videoId);
        public string GetVideoID(string videoUrl);
        public Task DownloadMedia(string id, string quality, string videoPath, string mediaType);
    }

    public abstract class BaseEntity
    {
        [SQLite.PrimaryKey]
        public Guid UniqueId { get; set; }
        public int _internalRowId;
    }


    public abstract class SqliteDatabase
    {
        protected static object lockobj = new object();

        public SQLiteConnection Connection { get; private set; }

        public string Name { get; set; }
        public string Location { get; set; }

        public SqliteDatabase(string dbName, string dbLocation)
        {
            Name = dbName;
            //Location = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, dbName);
            Location = System.IO.Path.Combine(dbLocation, dbName);
            if (!DBManager.Current.DoesDatabaseExist(dbName))
            {
                this.Connection = new SQLiteConnection(Location);
                DBManager.Current.RegisterDatabase(dbName, this);
            }
        }

        public void ExecuteStatement(string sql)
        {

            if (this.Connection != null && !this.Connection.IsInTransaction)
            {

                //Statement statement = this._sqlitedb.PrepareStatement(sql);
                //statement.Execute();
                this.Connection.Execute(sql);

            }
        }

        public void Close()
        {
            Connection.Close();
            Connection = null;
            Name = string.Empty;
            System.IO.File.Delete(Location);
            Location = string.Empty;
        }
    }

    public class DBManager
    {
        private static DBManager _instance = null;
        private static object lockobj = new object();

        public static DBManager Current
        {
            get
            {
                DBManager result;
                lock (lockobj)
                {
                    if (_instance == null)
                    {
                        _instance = new DBManager("xappdbs");
                    }
                    result = _instance;
                }
                return result;
            }
        }

        private Dictionary<string, SqliteDatabase> _databases;

        public SqliteDatabase GetDatabase(string name) { return _databases[name]; }
        public bool DoesDatabaseExist(string name) { return _databases.ContainsKey(name); }
        public void RegisterDatabase(string dbName, SqliteDatabase db)
        {
            if (!DoesDatabaseExist(dbName))
            {
                _databases.Add(dbName, db);
            }
        }
        private DBManager(string instanceName) { _databases = new Dictionary<string, SqliteDatabase>(); }

        public void DeleteDatabase(string dbName)
        {
            if (DoesDatabaseExist(dbName))
            {
                var db = _databases[dbName];
                db.Close();
                _databases.Remove(dbName);
            }
        }

        public void DeleteAllDatabases()
        {
            string[] keys = new string[_databases.Keys.Count];
            _databases.Keys.CopyTo(keys, 0);
            foreach (var dbName in keys)
            {
                DeleteDatabase(dbName);
            }
        }
    }

    public partial class AppDatabase : SqliteDatabase
    {
        private static AppDatabase _database = null;

        public static AppDatabase CurrentInstance
        {
            get {
                if (_database == null)
                {
                    throw new Exception("db has not been initialized .....");
                }
                return _database;

            }
        }

        public static AppDatabase Current(string dbLocation)
        {
                AppDatabase result;
                lock (lockobj)
                {
                    if (_database == null)
                    {
                        _database = new AppDatabase(dbLocation);
                    }
                    result = _database;
                }
                return result;
            
        }


        public Dictionary<string, TableSameDatabase> Tables;

        private AppDatabase(string dbLocation) : base("xapp.db", dbLocation) { }

        public void Init()
        {
            Tables = new Dictionary<string, TableSameDatabase>();
            this.Connection.CreateTable<Table>();
            refreshTables();
        }

        public void Unload()
        {
            _database.Connection.Close();
            _database.Connection.Dispose();
            _database = null;
        }

        public void AddTable(string tableName, string userName)
        {
            var exists = DoesTableExist(tableName);

            if (exists) return;

            var newTable = new Table()
            {
                Name = tableName,
                Type = (int)eTableType.UserDefined,
                CreatedBy = userName,
                CreatedDate = DateTime.UtcNow.ToUniversalTime()
            };

            this.Connection.Insert(newTable);
            try
            {
                refreshTables();
            }
            catch (Exception ex)
            {

            }

        }

        public bool DoesTableExist(string name)
        {
            var row = this.Connection.Query<Table>("SELECT * FROM 'Table' WHERE Name = ?", name);
            return row.Count() > 0;
        }

        private void refreshTables()
        {
            var tables = this.Connection.Query<Table>("SELECT * FROM 'Table'");

            foreach (var table in tables)
            {
                if (!Tables.ContainsKey(table.Name))
                {
                    TableSameDatabase tblDB = new TableSameDatabase(table.Name, this.Connection);
                    Tables.Add(table.Name, tblDB);
                }

            }
        }
    }

    public partial class TableSameDatabase
    {
        private SQLiteConnection _connection;

        public TableSameDatabase(string tableName, SQLiteConnection dbConnection)
        {
            _connection = dbConnection;
        }

        public int AddEntity<T>(T newEntity)
        {
            //this.Connection.CreateTable<T>(CreateFlags.FullTextSearch4);
            this._connection.CreateTable<T>();
            var createdEntityId = this._connection.Insert(newEntity);
            return createdEntityId;
        }

        public T GetEntity<T>(Guid uniqueId) where T : new()
        {
            //var resultsCount = this.Connection.Table<T>().Count();
            var name = typeof(T).Name;
            var qry = $"SELECT * FROM '{name}' WHERE uniqueid = '{uniqueId}'";
            var found = _connection.Query<T>(qry);
            return found.FirstOrDefault();
        }
        public List<T> GetEntities<T>(string where) where T : new()
        {
            //var resultsCount = this.Connection.Table<T>().Count();
            var name = typeof(T).Name;
            var qry = $"SELECT * FROM '{name}' WHERE {where}";
            return _connection.Query<T>(qry);
        }
        public List<T> GetAllEntities<T>() where T : new()
        {
            //var resultsCount = this.Connection.Table<T>().Count();
            var name = typeof(T).Name;
            var qry = $"SELECT ROWID as _rowId, * FROM '{name}'";
            try
            {
                return _connection.Query<T>(qry);
            }
            catch { return new List<T>(); }
        }
        public void DeleteAllEntities<T>() => _connection.DeleteAll<T>();
        public void UpdateEntity<T>(Guid id, T entityToUpdate) => _connection.Update(entityToUpdate);

        public void DeleteEntity<T>(Guid uniqueId) where T : new()
        {
            //var name = typeof(T).Name;
            //var qry = $"SELECT * FROM '{name}' WHERE uniqueid='{uniqueId}'";
            //var found = this.Connection.Query<T>(qry);
            //var found = GetEntity<T>(uniqueId);
            var result = _connection.Delete<T>(uniqueId);


        }


    }

    public class Table : BaseClass
    {
        public string Name { get; set; }
    }

    public class BaseClass
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int Type { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public enum eTableType
    {
        System = 0,
        UserDefined = 1
    }

    [DefaultProperty("Current")]
    public class DBContext
    {
        private static DBContext _instance = null;
        private static object lockobj = new object();

        public static DBContext Current
        {
            get
            {
                DBContext result;
                lock (lockobj)
                {
                    if (_instance == null)
                    {
                        _instance = new DBContext("xappdbs");
                    }
                    result = _instance;
                }
                return result;
            }
        }

        public DBManager Manager { get { return DBManager.Current; } }

        private Dictionary<string, object> _entities;

        private DBContext(string instanceName)
        {
            _entities = new Dictionary<string, object>();
            // todo :   use reflection to go through ALL classes that inherity abstract class BaseEntity
            //          and Register that entity
            var typesFound = ReflectiveEnumerator.GetEnumerableOfType<BaseEntity>();
            foreach (var type in typesFound)
            {
                MethodInfo method = typeof(DBContext).GetMethod("RegisterContext");
                MethodInfo generic = method.MakeGenericMethod(type.GetType());
                generic.Invoke(this, null);
            }

        }

        public bool DoesContextExist(string name) { return _entities.ContainsKey(name); }
        public bool DoesContextExist<T>() { return _entities.ContainsKey(typeof(T).Name); }

        public void RegisterContext<T>()
        {
            //eg. inline c# looks like this
            //      public class OrderHeaderContext : DataEntity<OrderHeader> { }
            //codify it looks like ...

            if (!DoesContextExist<T>())
            {
                var d1 = typeof(DataEntity<>);
                Type[] typeArgs = { typeof(T) };
                var makeme = d1.MakeGenericType(typeArgs);
                object o = Activator.CreateInstance(makeme);
                _entities.Add(typeof(T).Name, o);
            }
        }

        private IDataEntity<T> retrieveContext<T>()
        {
            return (IDataEntity<T>)_entities[typeof(T).Name];
        }

        public int Save<T>(T entityToSave)
        {
            return retrieveContext<T>().Save(entityToSave);
        }

        //public T Retrieve<T>(int idToRetrieve) {
        //    return retrieveContext<T>().Retrieve(idToRetrieve);
        //}
        public T RetrieveEntity<T>(Guid idToRetrieve)
        {
            return retrieveContext<T>().RetrieveEntity(idToRetrieve);
        }
        public List<T> RetrieveEntities<T>(string where)
        {
            return retrieveContext<T>().RetrieveEntities(where);
        }
        public List<T> RetrieveAllEntities<T>()
        {
            return retrieveContext<T>().RetrieveAllEntities();
        }
        //public int Find<T>(string query)
        //{
        //    return retrieveContext<T>().Find(query);
        //}

        //public int Find<T>(string query, params object[] args) where T : new()
        //{
        //    return retrieveContext<T>().Find<T>(query, args);
        //}

        public void DeleteAll<T>()
        {
            retrieveContext<T>().DeleteAll();
            retrieveContext<T>().DeleteAllEntities();
        }

        public void Delete<T>(int idToDelete)
        {
            retrieveContext<T>().Delete(idToDelete);
        }

        public void DeleteEntity<T>(Guid guid)
        {
            retrieveContext<T>().DeleteEntity(guid);
        }
    }


    public interface IDataEntity<T>
    {
        int Save(T instance);
        T RetrieveEntity(Guid id);
        List<T> RetrieveEntities(string where);
        List<T> RetrieveAllEntities();
        T Retrieve(int id);
        int Find(string whereQuery);
        int FindAll();
        void Delete(T instance);
        void Delete(int id);
        void DeleteEntity(Guid uniqueId);
        void DeleteAll();
        void DeleteAllEntities();
    }

    public class DataEntity<T> : IDataEntity<T>
        where T : BaseEntity, new()
    {
        public string _defaultCreator = "admin";
        private bool _isReadOnly = false;

        string _tableName;
        TableSameDatabase _table;

        public DataEntity() { SQLiteDataEntityImpl(true); }
        public DataEntity(bool initDb, bool isReadOnly) { SQLiteDataEntityImpl(initDb, isReadOnly); }
        private void SQLiteDataEntityImpl(bool initDb = true, bool isReadOnly = false)
        {
            _tableName = typeof(T).Name;
            _isReadOnly = isReadOnly;
            if (initDb) { InitEntityDatabase(); }

            else { _table = AppDatabase.CurrentInstance.Tables[_tableName]; }
            //Context.Current.RegisterEntity<T>();
        }

        public static DataEntity<T> Create => new DataEntity<T>();
        public static DataEntity<T> CreateReadOnly => new DataEntity<T>(false, true);

        //todo: optimization on columns to determine if they changed and thus do a DeleteAllColumns and rebuild
        private void InitEntityDatabase()
        {
            AppDatabase.CurrentInstance.AddTable(_tableName, _defaultCreator);
            _table = AppDatabase.CurrentInstance.Tables[_tableName];
        }

        public int Save(T instance)
        {
            if (_isReadOnly) return 0;


            if (instance.UniqueId != Guid.Empty)
            {
                _table.UpdateEntity(instance.UniqueId, instance);
            }
            else
            {
                if (instance.UniqueId == Guid.Empty) instance.UniqueId = Guid.NewGuid();
                _table.AddEntity(instance);
            }

            return instance._internalRowId;
        }

        public T RetrieveEntity(Guid id) => _table.GetEntity<T>(id);
        public List<T> RetrieveEntities(string where) => _table.GetEntities<T>(where);
        public List<T> RetrieveAllEntities() => _table.GetAllEntities<T>();
        public void DeleteAllEntities() => _table.DeleteAllEntities<T>();
        public void DeleteEntity(Guid uniqueId) => _table.DeleteEntity<T>(uniqueId);

        private void clear(T instance)
        {
            var props = typeof(T).GetTypeInfo().DeclaredProperties;
            foreach (var prop in props)
            {
                prop.SetValue(instance, null);
            }

            instance._internalRowId = 0;
            instance.UniqueId = Guid.Empty;
        }

        public T Retrieve(int id)
        {
            throw new NotImplementedException();
        }

        public int Find(string whereQuery)
        {
            throw new NotImplementedException();
        }

        public int FindAll()
        {
            throw new NotImplementedException();
        }

        public void Delete(T instance)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll()
        {
            //throw new NotImplementedException();
        }
    }

    public static class ReflectiveEnumerator
    {
        static ReflectiveEnumerator() { }

        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
        {
            List<T> objects = new List<T>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes()
                        .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                    {
                        objects.Add((T)Activator.CreateInstance(type, constructorArgs));
                    }
                }
                catch { }
            }


            //foreach (Type type in
            //    //Assembly.GetAssembly(typeof(T)).GetTypes()  <== ONLY THIS ASSEMBLY
            //    AppDomain.CurrentDomain.GetAssemblies().SelectMany(a=> a.GetTypes())  // <== ACROSS ALL LOADED ASSEMBLIES
            //    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            //{
            //    objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            //}
            //objects.Sort();
            return objects;
        }
    }
}
