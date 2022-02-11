using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Octokit;
using System.Diagnostics;
using System.Windows;

namespace StartNAV.Model
{

    internal class GitHub
    {
        public string GitUser { get; set; }         
        public string GitRepository { get; set; }   
        public string LastUpdateUri { get; set; }   //Letzte Updateurl
        public bool beta { get; set; }

        private string Log;
        private string updateuri;
        private string [,] Changelog;

        public string getLog() { return Log;}
        public string getUpdateUri() { return updateuri; }
        public string[,] getChangelog() 
        {
            ReadChangelog();
            return Changelog; 
        }



        public async Task UpdateApplicationAsync()
        {
            var client = new GitHubClient(new ProductHeaderValue(GitUser));
            var releases = await client.Repository.Release.GetAll(GitUser, GitRepository);

            updateuri = null;
            string version = "";
            string whatsnew = "";

            foreach (var temp in releases)
            {
                if (temp.Prerelease & !beta)
                    continue;   //Pre Release überstringen

                updateuri = temp.Assets[0].BrowserDownloadUrl;

                if (updateuri == LastUpdateUri)
                {
                    //Wenn die installierte Version mit der gefunden zusammen passt Ende
                    updateuri = null;
                    if (beta)
                        MessageBox.Show("Du besitzt bereits die neuerste Beta Version: " + temp.TagName, "Kein neues Update vorhanden", MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        MessageBox.Show("Du besitzt bereits die neuerste Version: " + temp.TagName, "Kein neues Update vorhanden", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;   //Wenn Updateversion gleich ist   
                }

                if (String.IsNullOrEmpty(updateuri))
                    continue;   //Wenn kein Release vorhanden ist

                version = temp.TagName;
                whatsnew = temp.Body;
                break;      
            }

            if (updateuri == null)
                return;

            MessageBoxResult res = MessageBox.Show("Neue Version " + version + " vorhanden, soll von " + updateuri + " heruntergeladen und installiert werden?\n\r"
                + "\n\r Das ist neu in der Version: \n\r"
                + whatsnew,
                "Update",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res == MessageBoxResult.No)
            {
                Log = "Update wurde übersprungen";
                updateuri = null;
                return;
            }

            Log = "Update von: " + updateuri;
        }

        private void ReadChangelog()
        {
            var client = new GitHubClient(new ProductHeaderValue(GitUser));
            Task<IReadOnlyList<Release>> releases = client.Repository.Release.GetAll(GitUser, GitRepository);
            releases.Wait();

            //string[,] ret = new string[releases.Result.Count,2];
            Changelog = new string[releases.Result.Count, 2];

            int i = 0;
            foreach (var temp in releases.Result)
            {
                if (temp.Draft == false)
                {

                    Changelog[i,0] = temp.TagName;
                    Changelog[i,1] = temp.Body;
                    i++;
                }
            }
        }
    }
}
