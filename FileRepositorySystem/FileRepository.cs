﻿using System;
using System.Collections.Generic;
using System.IO;
using TerminologyLauncher.Configs;
using TerminologyLauncher.Entities.FileRepository;
using TerminologyLauncher.Entities.InstanceManagement.Remote;
using TerminologyLauncher.Entities.SerializeUtils;
using TerminologyLauncher.Logging;
using TerminologyLauncher.Utils;
using TerminologyLauncher.Utils.ProgressService;

namespace TerminologyLauncher.FileRepositorySystem
{
    public class FileRepository
    {
        public String RepoUrl { get; set; }
        public Config Config { get; set; }
        private Dictionary<String, OfficialFileEntity> OfficialProviRdeFilesRepo { get; set; }
        public FileRepository(String configPath)
        {
            Logger.GetLogger().Info("Initializing file repo...");
            this.Config = new Config(new FileInfo(configPath));
            this.RepoUrl = this.Config.GetConfig("fileRepositoryUrl");
            this.OfficialProviRdeFilesRepo = new Dictionary<string, OfficialFileEntity>();
            Logger.GetLogger().Info("Initialized file repo!");

            DownloadUtils.DownloadFile(this.RepoUrl, this.Config.GetConfig("repoFilePath"));
            var repo = JsonConverter.Parse<FileRepositoryEntity>(File.ReadAllText(this.Config.GetConfig("repoFilePath")));
            foreach (var officialProvideFile in repo.Files)
            {
                this.OfficialProviRdeFilesRepo.Add(officialProvideFile.ProvideId, officialProvideFile);
            }

        }

        public OfficialFileEntity GetOfficialFile(String id)
        {
            if (!this.OfficialProviRdeFilesRepo.ContainsKey(id))
            {
                throw new Exception(String.Format("Can not find official file with id:{0} at current repo. Try to change repo or contact instance author to correct provide info.", id));
            }
            return this.OfficialProviRdeFilesRepo[id];
        }

    }
}
