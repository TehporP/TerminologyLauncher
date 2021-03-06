﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TerminologyLauncher.Configs;
using TerminologyLauncher.Entities.FileRepository;
using TerminologyLauncher.Logging;
using TerminologyLauncher.Utils;
using TerminologyLauncher.Utils.ProgressService;
using TerminologyLauncher.Utils.SerializeUtils;

namespace TerminologyLauncher.FileRepositorySystem
{
    public class FileRepository
    {
        public string RepoUrl { get; set; }
        public Config Config { get; set; }
        private Dictionary<string, RepositoryItemEntity> OfficialProviRdeFilesRepo { get; set; }
        private DirectoryInfo CacheDirectoryInfo { get; set; }
        private DirectoryInfo CacheRootDirectoryInfo { get; set; }
        public FileRepository(string configPath)
        {
            TerminologyLogger.GetLogger().Info("Initializing file repo...");
            this.Config = new Config(new FileInfo(configPath));
            this.RepoUrl = this.Config.GetConfigString("fileRepositoryUrl");
            this.OfficialProviRdeFilesRepo = new Dictionary<string, RepositoryItemEntity>();
            TerminologyLogger.GetLogger().Info("Initialized file repo!");
            this.CacheRootDirectoryInfo = new DirectoryInfo(Config.GetConfigString("repoCacheFolder"));
            if (!this.CacheRootDirectoryInfo.Exists)
            {
                this.CacheRootDirectoryInfo.Create();
            }
            TerminologyLogger.GetLogger().Info("Finished create repo repository folder.");
            //Different repo url will not share same cache folder.
            this.CacheDirectoryInfo = new DirectoryInfo(Path.Combine(this.CacheRootDirectoryInfo.FullName, EncodeUtils.CalculateStringMd5(this.Config.GetConfigString("fileRepositoryUrl"))));
            if (!this.CacheDirectoryInfo.Exists)
            {
                this.CacheDirectoryInfo.Create();
                TerminologyLogger.GetLogger().InfoFormat($"Created cache directory {this.CacheDirectoryInfo.Name}");
            }
            else
            {
                TerminologyLogger.GetLogger().InfoFormat($"Using cache directory {this.CacheDirectoryInfo.Name}");
            }
           
            TerminologyLogger.GetLogger().Info($"Start to fetch repo from url {RepoUrl}");
            try
            {
                var progress = new LeafNodeProgress("Fetch repo");
                DownloadUtils.DownloadFile(progress, this.RepoUrl, this.Config.GetConfigString("repoFilePath"));
                var repo =
                    JsonConverter.Parse<FileRepositoryEntity>(File.ReadAllText(this.Config.GetConfigString("repoFilePath")));
                foreach (var officialProvideFile in repo.Files)
                {
                    this.OfficialProviRdeFilesRepo.Add(officialProvideFile.ProvideId, officialProvideFile);
                }

            }
            catch (WebException)
            {
                TerminologyLogger.GetLogger().Error("Unable to receive repo right now. Trying to using local repo list.");
                if (!File.Exists(this.Config.GetConfigString("repoFilePath")))
                {
                    TerminologyLogger.GetLogger().Error("No local repo list available.");
                    return;
                }
                var repo =
                   JsonConverter.Parse<FileRepositoryEntity>(File.ReadAllText(this.Config.GetConfigString("repoFilePath")));
                foreach (var officialProvideFile in repo.Files)
                {
                    this.OfficialProviRdeFilesRepo.Add(officialProvideFile.ProvideId, officialProvideFile);
                }
                TerminologyLogger.GetLogger().Info("Used local repo list.");
            }
            catch (Exception)
            {
                throw;
            }

        }

        public FileInfo GetOfficialFile(LeafNodeProgress progress, string id)
        {
            if (!this.OfficialProviRdeFilesRepo.ContainsKey(id))
            {
                throw new Exception(
                    $"Cannot find official file with id:{id} at current repo. Try to change repo or contact instance author to correct provide info.");
            }
            var repositoryFile = this.OfficialProviRdeFilesRepo[id];
            var cacheFile = new FileInfo(Path.Combine(this.CacheDirectoryInfo.FullName, repositoryFile.ProvideId));
            if (cacheFile.Exists && (EncodeUtils.CalculateFileMd5(cacheFile.FullName).Equals(repositoryFile.Md5)))
            {
                progress.Percent = 100D;
                TerminologyLogger.GetLogger()
                    .InfoFormat($"File {repositoryFile.Name} exists and md5 check successful. Using cache file.");
            }
            else
            {
                TerminologyLogger.GetLogger()
                    .InfoFormat($"File {repositoryFile.Name} not exists or md5 check fault. Download new file.");

                cacheFile.Delete();
                DownloadUtils.DownloadFile(progress, repositoryFile.DownloadPath, cacheFile.FullName);
                TerminologyLogger.GetLogger()
                    .InfoFormat($"Successful download file {repositoryFile.Name} from url {repositoryFile.DownloadPath}");
            }
            return cacheFile;

        }

    }
}
