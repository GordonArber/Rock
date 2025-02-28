﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using Rock.Attribute;
using Rock.Model;
using Rock.Utility.Settings;

namespace Rock.Storage.AssetStorage
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Storage.AssetStorage.AssetStorageComponent" />
    [Description( "Amazon S3 Storage Service" )]
    [Export( typeof( AssetStorageComponent ) )]
    [ExportMetadata( "ComponentName", "AmazonS3" )]

    [TextField( "AWS Region",
        Description = "The AWS S3 Region in which the bucket is located. e.g. us-east-1",
        IsRequired = true,
        Order = 0,
        Key = AttributeKeys.AWSRegion )]

    [TextField( "Bucket",
        Description = "The name of the AWS S3 bucket where the files are stored.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKeys.Bucket )]

    [TextField( "Root Folder",
        Description = "Optional root folder. Must be the full path to the root folder starting from the first after the bucket name.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKeys.RootFolder )]

    [IntegerField( "Expiration",
        Description = "The time in minutes that the created public URL is available before being expired.",
        IsRequired = false,
        DefaultIntegerValue = 525600,
        Order = 3,
        Key = AttributeKeys.Expiration )]

    [TextField( "AWS Profile Name",
        Description = "Should be an AWS IAM user.",
        IsRequired = true,
        Order = 4,
        Key = AttributeKeys.AWSProfileName )]

    [TextField( "AWS Access Key",
        Description = "The access key for the user.",
        IsRequired = true,
        Order = 5,
        Key = AttributeKeys.AWSAccessKey )]

    [TextField( "AWS Secret Key",
        Description = "The seceret key for the user. Amazon only gives this when the user is created. If lost then a new user will need to be created.",
        IsRequired = true,
        Order = 6,
        Key = AttributeKeys.AWSSecretKey )]

    [BooleanField( "Generate Signed URLs",
        Description = "Signed URLs allow you to provide access to private buckets and to expire the generated links. Unsigned URLs rely on the ACLs of the bucket and do not expire.",
        TrueText = "Yes",
        FalseText = "No",
        IsRequired = true,
        DefaultBooleanValue = false,
        Order = 7,
        Key = AttributeKeys.GenerateSingedURLs )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.STORAGE_ASSETSTORAGE_AMAZONS3 )]
    public class AmazonS3Component : AssetStorageComponent
    {
        private static class AttributeKeys
        {
            public const string AWSRegion = "AWSRegion";
            public const string Bucket = "Bucket";
            public const string RootFolder = "RootFolder";
            public const string Expiration = "Expiration";
            public const string AWSProfileName = "AWSProfileName";
            public const string AWSAccessKey = "AWSAccessKey";
            public const string AWSSecretKey = "AWSSecretKey";
            public const string GenerateSingedURLs = "GenerateSingedURLs";
        }

        #region Properties

        /// <summary>
        /// Specify the icon for the AssetStorageComponent here. It will display in the folder tree.
        /// Default is server.png.
        /// </summary>
        /// <value>
        /// The component icon path.
        /// </value>
        public override string IconCssClass
        {
            get
            {
                return "fa fa-aws";
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonS3Component"/> class.
        /// </summary>
        public AmazonS3Component() : base()
        {
        }

        #endregion Constructors

        #region Override Methods

        /// <summary>
        /// Lists the objects from the current root folder.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageProvider assetStorageProvider )
        {
            return ListObjects( assetStorageProvider, new Asset { Type = AssetType.Folder } );
        }

        /// <summary>
        /// Lists the objects. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If key and name are not provided then list all objects from the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder is not used, and Name is not used.
        /// The last segment in Key is treated as a begins with search if it does not end in a '/'. e.g. to get all
        /// files starting with 'mr' in folder 'pictures/cats/' set key = 'pictures/cats/mr' to get 'mr. whiskers'
        /// and 'mrs. whiskers' but not 'fluffy' or 'carnage the attack cat'.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            return ListObjects( assetStorageProvider, asset, false );
        }

        /// <summary>
        /// Lists the objects. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If key and name are not provided then list all objects from the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder is not used, and Name is not used.
        /// The last segment in Key is treated as a begins with search if it does not end in a '/'. e.g. to get all
        /// files starting with 'mr' in folder 'pictures/cats/' set key = 'pictures/cats/mr' to get 'mr. whiskers'
        /// and 'mrs. whiskers' but not 'fluffy' or 'carnage the attack cat'.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset"></param>
        /// <param name="isRecursive">if set to <c>true</c> [is recursive].</param>
        /// <returns></returns>
        private List<Asset> ListObjects( AssetStorageProvider assetStorageProvider, Asset asset, bool isRecursive )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                AmazonS3Client client = GetAmazonS3Client( assetStorageProvider );

                ListObjectsV2Request request = new ListObjectsV2Request();
                request.BucketName = GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket );
                request.Prefix = asset.Key == "/" ? GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) : asset.Key;

                var assets = new List<Asset>();

                ListObjectsV2Response response;

                do
                {
                    response = client.ListObjectsV2( request );
                    foreach ( S3Object s3Object in response.S3Objects )
                    {
                        var objectName = s3Object.Key.Replace( request.Prefix, "" );
                        var slashIndex = objectName.IndexOf( "/" );
                        var isSubitem = slashIndex > 0 && slashIndex < ( objectName.Length - 1 );
                        var ignoreItem = isSubitem || string.IsNullOrWhiteSpace( objectName );

                        if ( s3Object.Key == null || ( !isRecursive && ignoreItem ) )
                        {
                            continue;
                        }

                        var responseAsset = CreateAssetFromS3Object( assetStorageProvider, s3Object, client.Config.RegionEndpoint.SystemName );
                        assets.Add( responseAsset );
                    }

                    request.ContinuationToken = response.NextContinuationToken;
                } while ( response.IsTruncated );

                return assets.OrderBy( a => a.Key, StringComparer.OrdinalIgnoreCase ).ToList();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Lists the files in AssetStorageProvider.RootFolder.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <returns></returns>
        public override List<Asset> ListFilesInFolder( AssetStorageProvider assetStorageProvider )
        {
            return ListFilesInFolder( assetStorageProvider, new Asset { Type = AssetType.Folder } );
        }

        /// <summary>
        /// Lists the files in folder. Asset.Key or Asset.Name is the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key and Name are not provided then list all files in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in the key is the folder name.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override List<Asset> ListFilesInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFolder( asset );

            try
            {
                AmazonS3Client client = GetAmazonS3Client( assetStorageProvider );

                ListObjectsV2Request request = new ListObjectsV2Request();
                request.BucketName = GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket );
                request.Prefix = asset.Key == "/" ? string.Empty : asset.Key;
                request.Delimiter = "/";

                var assets = new List<Asset>();

                ListObjectsV2Response response;

                // S3 will only return 1,000 keys per response and sets IsTruncated = true, the do-while loop will run and fetch keys until IsTruncated = false.
                do
                {
                    response = client.ListObjectsV2( request );
                    foreach ( S3Object s3Object in response.S3Objects )
                    {
                        if ( s3Object.Key == null || s3Object.Key.EndsWith( "/" ) )
                        {
                            continue;
                        }

                        var responseAsset = CreateAssetFromS3Object( assetStorageProvider, s3Object, client.Config.RegionEndpoint.SystemName );
                        assets.Add( responseAsset );
                    }

                    request.ContinuationToken = response.NextContinuationToken;

                } while ( response.IsTruncated );

                return assets.OrderBy( a => a.Key, StringComparer.OrdinalIgnoreCase ).ToList();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Lists the folders in AssetStorageProvider.Rootfolder.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <returns></returns>
        public override List<Asset> ListFoldersInFolder( AssetStorageProvider assetStorageProvider )
        {
            return ListFoldersInFolder( assetStorageProvider, new Asset { Type = AssetType.Folder } );
        }

        /// <summary>
        /// Lists the folder in folder. Asset.Key or Asset.Name is the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key and Name are not provided then list all files in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in the key is the folder name.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListFoldersInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) );
            string bucketName = GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket );

            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFolder( asset );

            try
            {
                AmazonS3Client client = GetAmazonS3Client( assetStorageProvider );

                ListObjectsV2Request request = new ListObjectsV2Request();
                request.BucketName = bucketName;
                request.Prefix = asset.Key == "/" ? string.Empty : asset.Key;
                request.Delimiter = "/";

                var assets = new List<Asset>();
                ListObjectsV2Response response;

                do
                {
                    response = client.ListObjectsV2( request );
                    foreach ( string subFolder in response.CommonPrefixes )
                    {
                        if ( subFolder.IsNotNullOrWhiteSpace() )
                        {
                            var subFolderAsset = CreateAssetFromCommonPrefix( subFolder, client.Config.RegionEndpoint.SystemName, bucketName );
                            assets.Add( subFolderAsset );
                        }
                    }

                    request.ContinuationToken = response.NextContinuationToken;

                } while ( response.IsTruncated );

                return assets.OrderBy( a => a.Key, StringComparer.OrdinalIgnoreCase ).ToList();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Returns an asset with the stream of the specified file and creates a thumbnail.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override Asset GetObject( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            return GetObject( assetStorageProvider, asset, true );
        }

        /// <summary>
        /// Returns an asset with the stream of the specified file with the option to create a thumbnail.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="createThumbnail">if set to <c>true</c> [create thumbnail].</param>
        /// <returns></returns>
        public override Asset GetObject( AssetStorageProvider assetStorageProvider, Asset asset, bool createThumbnail )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFile( asset );

            try
            {
                AmazonS3Client client = GetAmazonS3Client( assetStorageProvider );

                GetObjectResponse response = client.GetObject( GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket ), asset.Key );
                return CreateAssetFromGetObjectResponse( assetStorageProvider, response, client.Config.RegionEndpoint.SystemName, createThumbnail );

            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Uploads a file. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If a key is provided it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override bool UploadObject( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                AmazonS3Client client = GetAmazonS3Client( assetStorageProvider );

                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket );
                request.Key = asset.Key;
                request.InputStream = asset.AssetStream;

                PutObjectResponse response = client.PutObject( request );
                if ( response.HttpStatusCode == System.Net.HttpStatusCode.OK )
                {
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

            return false;
        }

        /// <summary>
        /// Creates a folder. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key is provided it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override bool CreateFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                AmazonS3Client client = GetAmazonS3Client( assetStorageProvider );

                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket );
                request.Key = asset.Key;

                PutObjectResponse response = client.PutObject( request );
                if ( response.HttpStatusCode == System.Net.HttpStatusCode.OK )
                {
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

            return false;
        }

        /// <summary>
        /// Deletes the asset. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key is provided then it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override bool DeleteAsset( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );
            AmazonS3Client client = GetAmazonS3Client( assetStorageProvider );

            if ( asset.Type == AssetType.File )
            {
                try
                {
                    DeleteObjectRequest request = new DeleteObjectRequest()
                    {
                        BucketName = GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket ),
                        Key = asset.Key
                    };

                    DeleteObjectResponse response = client.DeleteObject( request );
                    DeleteImageThumbnail( assetStorageProvider, asset );
                    return true;
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    throw;
                }
            }
            else
            {
                try
                {
                    MultipleObjectDelete( client, assetStorageProvider, asset );
                    DeleteImageThumbnail( assetStorageProvider, asset );
                    return true;
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    throw;
                }
            }
        }

        /// <summary>
        /// Renames the asset.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        public override bool RenameAsset( AssetStorageProvider assetStorageProvider, Asset asset, string newName )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) );
            asset.Key = asset.Key.IsNullOrWhiteSpace() ? rootFolder + asset.Name : asset.Key;
            string bucket = GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket );
            try
            {
                AmazonS3Client client = GetAmazonS3Client( assetStorageProvider );

                CopyObjectRequest copyRequest = new CopyObjectRequest();
                copyRequest.SourceBucket = bucket;
                copyRequest.DestinationBucket = bucket;
                copyRequest.SourceKey = asset.Key;
                copyRequest.DestinationKey = GetPathFromKey( asset.Key ) + newName;
                CopyObjectResponse copyResponse = client.CopyObject( copyRequest );
                if ( copyResponse.HttpStatusCode != System.Net.HttpStatusCode.OK )
                {
                    return false;
                }

                if ( DeleteAsset( assetStorageProvider, asset ) )
                {
                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

            return false;
        }

        /// <summary>
        /// Creates the download link.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override string CreateDownloadLink( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );
            string url = string.Empty;

            try
            {
                AmazonS3Client client = GetAmazonS3Client( assetStorageProvider );

                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket ),
                    Key = asset.Key,
                    Expires = RockDateTime.SystemDateTime.AddMinutes( GetAttributeValue( assetStorageProvider, AttributeKeys.Expiration ).AsDouble() )
                };

                url = client.GetPreSignedURL( request );

                if ( GetAttributeValue( assetStorageProvider, AttributeKeys.GenerateSingedURLs ).AsBooleanOrNull() ?? false )
                {
                    return url;
                }
                else
                {
                    return url.Left( url.IndexOf( '?' ) );
                }

            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Gets the objects in folder without recursion. i.e. will get the list of files
        /// and folders in the folder but not the contents of the subfolders. Subfolders
        /// will not have the ModifiedDate prop filled in as Amazon doesn't provide it in
        /// this context.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage Provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListObjectsInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKeys.RootFolder ) );
            string bucketName = GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFolder( asset );

            try
            {
                AmazonS3Client client = GetAmazonS3Client( assetStorageProvider );

                ListObjectsV2Request request = new ListObjectsV2Request();
                request.BucketName = bucketName;
                request.Prefix = asset.Key == "/" ? rootFolder : asset.Key;
                request.Delimiter = "/";

                var assets = new List<Asset>();
                var subFolders = new HashSet<string>();

                ListObjectsV2Response response;

                // S3 will only return 1,000 keys per response and sets IsTruncated = true, the do-while loop will run and fetch keys until IsTruncated = false.
                do
                {
                    response = client.ListObjectsV2( request );
                    foreach ( S3Object s3Object in response.S3Objects )
                    {
                        if ( s3Object.Key == null )
                        {
                            continue;
                        }

                        var responseAsset = CreateAssetFromS3Object( assetStorageProvider, s3Object, client.Config.RegionEndpoint.SystemName );
                        assets.Add( responseAsset );
                    }

                    // After setting the delimiter S3 will filter out any prefixes below that in response.S3Objects.
                    // So we need to inspect response.CommonPrefixes to get the prefixes inside the folder.
                    foreach ( string subFolder in response.CommonPrefixes )
                    {
                        if ( subFolder.IsNotNullOrWhiteSpace() )
                        {
                            subFolders.Add( subFolder );
                        }
                    }

                    request.ContinuationToken = response.NextContinuationToken;

                } while ( response.IsTruncated );

                // Add the subfolders to the asset collection
                foreach ( string subFolder in subFolders )
                {
                    var subFolderAsset = CreateAssetFromCommonPrefix( subFolder, client.Config.RegionEndpoint.SystemName, bucketName );
                    assets.Add( subFolderAsset );
                }

                return assets.OrderBy( a => a.Key, StringComparer.OrdinalIgnoreCase ).ToList();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Gets the thumbnail image for the provided Asset key. If one does not exist it will be created. If one exists but is older than the file
        /// a new thumbnail is created and the old one overwritten.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="assetKey">The asset key.</param>
        /// <param name="lastModifiedDateTime">The last modified date time.</param>
        /// <returns></returns>
        public override string GetThumbnail( AssetStorageProvider assetStorageProvider, string assetKey, DateTime? lastModifiedDateTime )
        {
            string name = GetNameFromKey( assetKey );
            string path = GetPathFromKey( assetKey );

            string mimeType = System.Web.MimeMapping.GetMimeMapping( name );
            if ( !mimeType.StartsWith( "image/" ) )
            {
                return GetFileTypeIcon( assetKey );
            }

            // check if thumbnail exists
            string thumbDir = $"{ThumbnailRootPath}/{assetStorageProvider.Id}/{path}";
            Directory.CreateDirectory( FileSystemComponentHttpContext.Server.MapPath( thumbDir ) );

            string virtualThumbPath = Path.Combine( thumbDir, name );
            string physicalThumbPath = FileSystemComponentHttpContext.Server.MapPath( virtualThumbPath );

            // Encode the name thumb path since it can contain special characters
            virtualThumbPath = virtualThumbPath.EncodeHtml();

            if ( File.Exists( physicalThumbPath ) )
            {
                var thumbLastModDate = File.GetLastWriteTimeUtc( physicalThumbPath );
                if ( lastModifiedDateTime <= thumbLastModDate )
                {
                    // thumbnail is still good so just return the virtual file path.
                    return virtualThumbPath;
                }
            }

            CreateImageThumbnail( assetStorageProvider, new Asset { Name = name, Key = assetKey, Type = AssetType.File }, physicalThumbPath, false );

            return virtualThumbPath;
        }

        /// <summary>
        /// Deletes the image thumbnail for the provided Asset. If the asset is a file then the singel thumbnail
        /// is deleted. If the asset is a directory then a recurrsive delete is done.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        protected override void DeleteImageThumbnail( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            base.DeleteImageThumbnail( assetStorageProvider, asset );
        }

        #endregion Override Methods

        #region Private Methods

        /// <summary>
        /// Deletes all of the S3Objects in the provided folder asset.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="assetStorageProvider">The asset storage Provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        private bool MultipleObjectDelete( AmazonS3Client client, AssetStorageProvider assetStorageProvider, Asset asset )
        {
            // The list of keys that will be passed into the multiple delete request
            List<KeyVersion> keys = new List<KeyVersion>();

            // Amazon only accepts 1000 keys per request, use this to keep track of how many already sent
            int keyIndex = 0;

            try
            {
                // Get a list of objest with prefix
                var assetDeleteList = ListObjects( assetStorageProvider, asset, true );

                // Create the list of keys
                foreach ( var assetDelete in assetDeleteList )
                {
                    keys.Add( new KeyVersion { Key = assetDelete.Key } );
                }

                while ( keyIndex < keys.Count() )
                {
                    int range = keys.Count() - keyIndex < 1000 ? keys.Count() - keyIndex : 1000;
                    var deleteObjectsRequest = new DeleteObjectsRequest
                    {
                        BucketName = GetAttributeValue( assetStorageProvider, AttributeKeys.Bucket ),
                        Objects = keys.GetRange( keyIndex, range )
                    };

                    DeleteObjectsResponse response = client.DeleteObjects( deleteObjectsRequest );
                    keyIndex += range;
                }

                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

        }

        /// <summary>
        /// Creates the asset from the AWS S3Object.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="s3Object">The s3 object.</param>
        /// <param name="regionEndpoint">The region endpoint.</param>
        /// <returns></returns>
        private Asset CreateAssetFromS3Object( AssetStorageProvider assetStorageProvider, S3Object s3Object, string regionEndpoint )
        {
            string name = GetNameFromKey( s3Object.Key );
            string uriKey = System.Web.HttpUtility.UrlPathEncode( s3Object.Key );
            AssetType assetType = GetAssetType( s3Object.Key );
            Asset asset = new Asset();

            try
            {
                asset = new Asset
                {
                    Name = name,
                    Key = s3Object.Key,
                    Uri = $"https://{s3Object.BucketName}.s3.{regionEndpoint}.amazonaws.com/{uriKey}",
                    Type = assetType,
                    IconPath = assetType == AssetType.Folder ? string.Empty : GetThumbnail( assetStorageProvider, s3Object.Key, s3Object.LastModified ),
                    FileSize = s3Object.Size,
                    LastModifiedDateTime = s3Object.LastModified,
                    Description = s3Object.StorageClass == null ? string.Empty : s3Object.StorageClass.ToString()
                };

            }
            catch ( Exception ex )
            {
                // log the exception and create an assett that describes the error, but don't throw it.
                ExceptionLogService.LogException( new AggregateException( $"Error creating S3 asset for key {s3Object.Key}", ex ) );

                asset = new Asset
                {
                    Name = name,
                    Key = s3Object.Key,
                    Uri = $"https://{s3Object.BucketName}.s3.{regionEndpoint}.amazonaws.com/{uriKey}",
                    Type = assetType,
                    IconPath = assetType == AssetType.Folder ? string.Empty : GetCorruptImageAssetImage(),
                    FileSize = s3Object.Size,
                    LastModifiedDateTime = s3Object.LastModified,
                    Description = s3Object.StorageClass == null ? string.Empty : s3Object.StorageClass.ToString(),
                    HasError = true
                };
            };

            return asset;
        }

        /// <summary>
        /// Creates the asset from AWS S3 Client GetObjectResponse.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="response">The response.</param>
        /// <param name="regionEndpoint">The region endpoint.</param>
        /// <param name="createThumbnail">if set to <c>true</c> [create thumbnail].</param>
        /// <returns></returns>
        private Asset CreateAssetFromGetObjectResponse( AssetStorageProvider assetStorageProvider, GetObjectResponse response, string regionEndpoint, bool createThumbnail )
        {
            string name = GetNameFromKey( response.Key );
            string uriKey = System.Web.HttpUtility.UrlPathEncode( response.Key );

            /*
                 2/24/2021 - NA

                 The response.ResponseStream wrapper class appears to change based
                 on file (perhaps the file size?) so in certain cases you cannot read
                 the response.ResponseStream.Length. But, the response.ContentLength
                 appears to *always* be available (and it matches the response.ResponseStream.Length
                 when it was available during my tests).

                 Reason: Amazon S3's Amazon.Runtime.Internal.Util.WrapperStream changes based on the file.
            */

            Asset asset = new Asset();

            try
            {
                asset = new Asset
                {
                    Name = name,
                    Key = response.Key,
                    Uri = $"https://{response.BucketName}.s3.{regionEndpoint}.amazonaws.com/{uriKey}",
                    Type = GetAssetType( response.Key ),
                    IconPath = createThumbnail == true ? GetThumbnail( assetStorageProvider, response.Key, response.LastModified ) : GetFileTypeIcon( response.Key ),
                    FileSize = response.ContentLength,
                    LastModifiedDateTime = response.LastModified,
                    Description = response.StorageClass == null ? string.Empty : response.StorageClass.ToString(),
                    AssetStream = response.ResponseStream,
                    HasError = true
                };
            }
            catch ( Exception ex )
            {
                // log the exception and create an assett that describes the error, but don't throw it.
                ExceptionLogService.LogException( new AggregateException( $"Error creating asset for key {response.Key}", ex ) );
                asset = new Asset
                {
                    Name = name,
                    Key = response.Key,
                    Uri = $"https://{response.BucketName}.s3.{regionEndpoint}.amazonaws.com/{uriKey}",
                    Type = GetAssetType( response.Key ),
                    IconPath = GetCorruptImageAssetImage(),
                    FileSize = response.ContentLength,
                    LastModifiedDateTime = response.LastModified,
                    Description = response.StorageClass == null ? string.Empty : response.StorageClass.ToString(),
                    AssetStream = response.ResponseStream,
                    HasError = true
                };
            }



            return asset;
        }

        /// <summary>
        /// Creates a folder asset using a commonPrefix from AWS.
        /// </summary>
        /// <param name="commonPrefix">The common prefix.</param>
        /// <param name="regionEndpoint">The region endpoint.</param>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <returns></returns>
        private Asset CreateAssetFromCommonPrefix( string commonPrefix, string regionEndpoint, string bucketName )
        {
            string uriKey = System.Web.HttpUtility.UrlPathEncode( commonPrefix );
            string name = GetNameFromKey( commonPrefix );

            return new Asset
            {
                Name = name,
                Key = commonPrefix,
                Uri = $"https://{bucketName}.s3.{regionEndpoint}.amazonaws.com/{uriKey}",
                Type = AssetType.Folder,
                IconPath = string.Empty,
                FileSize = 0,
                LastModifiedDateTime = null,
                Description = string.Empty
            };
        }

        /// <summary>
        /// Determines whether Asset has the required elements for an AWS file.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <exception cref="Exception">Asset Type is set to 'Folder' instead of 'File.'</exception>
        private void HasRequirementsFile( Asset asset )
        {
            if ( asset.Type == AssetType.Folder )
            {
                throw new Exception( "Asset Type is set to 'Folder' instead of 'File.'" );
            }
        }

        /// <summary>
        /// Determines whether the Asset has the required elements for a AWS folder.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <exception cref="Exception">
        /// Asset Type is set to 'File' instead of 'Folder.'
        /// or
        /// Name and key cannot both be null or empty.
        /// </exception>
        private void HasRequirementsFolder( Asset asset )
        {
            if ( asset.Type == AssetType.File )
            {
                throw new Exception( "Asset Type is set to 'File' instead of 'Folder.'" );
            }

            if ( asset.Name.IsNullOrWhiteSpace() && asset.Key.IsNullOrWhiteSpace() )
            {
                throw new Exception( "Name and key cannot both be null or empty." );
            }
        }

        /// <summary>
        /// Makes adjustments to the Key string based on the root folder, the name, and the AssetType.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="rootFolder">The root folder.</param>
        /// <returns></returns>
        private string FixKey( Asset asset, string rootFolder )
        {
            if ( asset.Key.IsNullOrWhiteSpace() && asset.Name.IsNullOrWhiteSpace() )
            {
                asset.Key = rootFolder;
            }
            else if ( asset.Key.IsNullOrWhiteSpace() && asset.Name.IsNotNullOrWhiteSpace() )
            {
                asset.Key = rootFolder + asset.Name;
            }

            if ( asset.Type == AssetType.Folder && !asset.Key.EndsWith( "/" ) )
            {
                asset.Key += "/";
            }

            return asset.Key;
        }

        /// <summary>
        /// Determine the correct AssetType based on the provided name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private AssetType GetAssetType( string name )
        {
            if ( name.EndsWith( "/" ) )
            {
                return AssetType.Folder;
            }

            return AssetType.File;
        }

        /// <summary>
        /// Returns an Amazon S3 Client obj using the settings in the provided AssetStorageProvider obj.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <returns></returns>
        private AmazonS3Client GetAmazonS3Client( AssetStorageProvider assetStorageProvider )
        {

            string awsAccessKey = GetAttributeValue( assetStorageProvider, AttributeKeys.AWSAccessKey );
            string awsSecretKey = GetAttributeValue( assetStorageProvider, AttributeKeys.AWSSecretKey );
            string awsRegion = GetAttributeValue( assetStorageProvider, AttributeKeys.AWSRegion );
            RegionEndpoint regionEndPoint = Amazon.RegionEndpoint.GetBySystemName( awsRegion );

            return new AmazonS3Client( awsAccessKey, awsSecretKey, regionEndPoint );
        }

        /// <summary>
        /// Gets the name from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetNameFromKey( string key )
        {
            if ( key.LastIndexOf( '/' ) < 1 )
            {
                return key;
            }

            string[] pathSegments = key.Split( '/' );

            if ( key.EndsWith( "/" ) )
            {
                return pathSegments[pathSegments.Length - 2];
            }

            return pathSegments[pathSegments.Length - 1];
        }

        /// <summary>
        /// Gets the path from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The file prefix used by AWS to mimic a folder structure.</returns>
        private string GetPathFromKey( string key )
        {
            int i = key.LastIndexOf( '/' );
            if ( i < 1 )
            {
                return string.Empty;
            }

            return key.Substring( 0, i + 1 );
        }

        #endregion Private Methods

    }
}
